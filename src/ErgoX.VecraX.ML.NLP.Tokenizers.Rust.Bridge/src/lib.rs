use std::cell::RefCell;
use std::ffi::{CStr, CString};
use std::os::raw::{c_char, c_int};
use std::ptr;

use tokenizers::tokenizer::EncodeInput;
use tokenizers::utils::padding::{PaddingDirection, PaddingParams, PaddingStrategy};
use tokenizers::utils::truncation::{TruncationDirection, TruncationParams, TruncationStrategy};
use tokenizers::{Encoding, Tokenizer};

mod chat;
use chat::{render_chat_template, ChatTemplateError};

#[cfg(not(any(target_family = "wasm", target_os = "ios", target_os = "android")))]
use tokenizers::FromPretrainedParameters;

pub struct CTokenizer {
    inner: Tokenizer,
}

#[derive(Clone)]
pub struct CEncoding {
    ids: Vec<u32>,
    tokens: Vec<String>,
    offsets: Vec<(u32, u32)>,
    type_ids: Vec<u32>,
    attention_mask: Vec<u32>,
    special_tokens_mask: Vec<u32>,
    word_ids: Vec<Option<u32>>,
    sequence_ids: Vec<Option<usize>>,
    overflowing: Vec<CEncoding>,
}

#[repr(C)]
pub struct CEncodingOffset {
    pub start: u32,
    pub end: u32,
}

#[repr(C)]
pub struct CEncodingNumericDest {
    pub ids: *mut u32,
    pub type_ids: *mut u32,
    pub attention_mask: *mut u32,
    pub special_tokens_mask: *mut u32,
    pub offsets: *mut CEncodingOffset,
    pub word_ids: *mut i32,
    pub sequence_ids: *mut i32,
}

thread_local! {
    static LAST_ERROR: RefCell<Option<CString>> = RefCell::new(None);
}

fn set_status(status: *mut c_int, value: c_int) {
    if !status.is_null() {
        unsafe {
            *status = value;
        }
    }
}

fn set_length(target: *mut usize, value: usize) {
    if !target.is_null() {
        unsafe {
            *target = value;
        }
    }
}

fn store_error(message: &str) {
    LAST_ERROR.with(|cell| {
    let fallback = "tokenx bridge error";
        let cstring = CString::new(message)
            .unwrap_or_else(|_| CString::new(fallback).expect("fallback string is valid"));
        *cell.borrow_mut() = Some(cstring);
    });
}

fn clear_error() {
    LAST_ERROR.with(|cell| {
        *cell.borrow_mut() = None;
    });
}

fn read_required_utf8(ptr_value: *const c_char) -> Result<String, &'static str> {
    if ptr_value.is_null() {
        return Err("received null pointer");
    }

    let source = unsafe { CStr::from_ptr(ptr_value) };
    source
        .to_str()
        .map(|s| s.to_owned())
        .map_err(|_| "invalid UTF-8 data")
}

fn read_optional_utf8(ptr_value: *const c_char) -> Result<Option<String>, &'static str> {
    if ptr_value.is_null() {
        Ok(None)
    } else {
        read_required_utf8(ptr_value).map(Some)
    }
}

impl CEncoding {
    fn from_encoding(encoding: Encoding) -> Self {
        let ids = encoding.get_ids().to_vec();
        let tokens = encoding.get_tokens().to_vec();
        let offsets = encoding
            .get_offsets()
            .iter()
            .map(|(start, end)| (*start as u32, *end as u32))
            .collect();
        let type_ids = encoding.get_type_ids().to_vec();
        let attention_mask = encoding.get_attention_mask().to_vec();
        let special_tokens_mask = encoding.get_special_tokens_mask().to_vec();
        let word_ids = encoding.get_word_ids().to_vec();
        let sequence_ids = encoding.get_sequence_ids();
        let overflowing = encoding
            .get_overflowing()
            .iter()
            .cloned()
            .map(Self::from_encoding)
            .collect();

        Self {
            ids,
            tokens,
            offsets,
            type_ids,
            attention_mask,
            special_tokens_mask,
            word_ids,
            sequence_ids,
            overflowing,
        }
    }

    fn len(&self) -> usize {
        self.ids.len()
    }
}

#[no_mangle]
pub extern "C" fn tokenizers_get_last_error() -> *const c_char {
    LAST_ERROR.with(|cell| match &*cell.borrow() {
        Some(err) => err.as_ptr(),
        None => ptr::null(),
    })
}

#[cfg(not(any(target_family = "wasm", target_os = "ios", target_os = "android")))]
#[no_mangle]
pub extern "C" fn tokenizers_from_pretrained(
    identifier: *const c_char,
    revision: *const c_char,
    auth_token: *const c_char,
    status: *mut c_int,
) -> *mut CTokenizer {
    let name = match read_required_utf8(identifier) {
        Ok(value) => value,
        Err(message) => {
            store_error(message);
            set_status(status, 1);
            return ptr::null_mut();
        }
    };

    let revision_value = match read_optional_utf8(revision) {
        Ok(value) => value,
        Err(message) => {
            store_error(message);
            set_status(status, 2);
            return ptr::null_mut();
        }
    };

    let token_value = match read_optional_utf8(auth_token) {
        Ok(value) => value,
        Err(message) => {
            store_error(message);
            set_status(status, 3);
            return ptr::null_mut();
        }
    };

    let mut params = FromPretrainedParameters::default();
    if let Some(revision) = revision_value {
        if !revision.is_empty() {
            params.revision = revision;
        }
    }
    params.token = token_value;

    match Tokenizer::from_pretrained(name.as_str(), Some(params)) {
        Ok(tokenizer) => {
            clear_error();
            set_status(status, 0);
            Box::into_raw(Box::new(CTokenizer { inner: tokenizer }))
        }
        Err(err) => {
            store_error(&format!("tokenizers_from_pretrained failed: {err}"));
            set_status(status, 4);
            ptr::null_mut()
        }
    }
}

#[cfg(any(target_family = "wasm", target_os = "ios", target_os = "android"))]
#[no_mangle]
pub extern "C" fn tokenizers_from_pretrained(
    _identifier: *const c_char,
    _revision: *const c_char,
    _auth_token: *const c_char,
    status: *mut c_int,
) -> *mut CTokenizer {
    store_error("tokenizers_from_pretrained requires the 'http' feature");
    set_status(status, 1);
    ptr::null_mut()
}

#[no_mangle]
pub extern "C" fn tokenizers_create(json: *const c_char, status: *mut c_int) -> *mut CTokenizer {
    match read_required_utf8(json) {
        Ok(content) => match Tokenizer::from_bytes(content.into_bytes()) {
            Ok(tokenizer) => {
                clear_error();
                set_status(status, 0);
                Box::into_raw(Box::new(CTokenizer { inner: tokenizer }))
            }
            Err(err) => {
                store_error(&format!("tokenizers_create failed: {err}"));
                set_status(status, 2);
                ptr::null_mut()
            }
        },
        Err(message) => {
            store_error(message);
            set_status(status, 1);
            ptr::null_mut()
        }
    }
}

#[no_mangle]
pub extern "C" fn tokenizers_free(tokenizer: *mut CTokenizer) {
    if tokenizer.is_null() {
        return;
    }

    unsafe {
        drop(Box::from_raw(tokenizer));
    }
}

#[no_mangle]
pub extern "C" fn tokenizers_encode(
    tokenizer: *mut CTokenizer,
    sequence: *const c_char,
    pair: *const c_char,
    add_special_tokens: bool,
    length: *mut usize,
    status: *mut c_int,
) -> *mut CEncoding {
    if tokenizer.is_null() {
        store_error("tokenizers_encode received null tokenizer");
        set_status(status, 1);
        return ptr::null_mut();
    }

    let primary = match read_required_utf8(sequence) {
        Ok(value) => value,
        Err(message) => {
            store_error(message);
            set_status(status, 2);
            return ptr::null_mut();
        }
    };

    let pair_text = match read_optional_utf8(pair) {
        Ok(text) => text,
        Err(message) => {
            store_error(message);
            set_status(status, 3);
            return ptr::null_mut();
        }
    };

    let tokenizer = unsafe { &mut *tokenizer };
    let input = match pair_text.as_ref() {
        Some(pair_value) => EncodeInput::from((primary.as_str(), pair_value.as_str())),
        None => EncodeInput::from(primary.as_str()),
    };

    match tokenizer.inner.encode_char_offsets(input, add_special_tokens) {
        Ok(encoding) => {
            let managed = CEncoding::from_encoding(encoding);
            set_length(length, managed.len());
            clear_error();
            set_status(status, 0);
            Box::into_raw(Box::new(managed))
        }
        Err(err) => {
            store_error(&format!("tokenizers_encode failed: {err}"));
            set_status(status, 4);
            ptr::null_mut()
        }
    }
}

#[no_mangle]
pub extern "C" fn tokenizers_encoding_free(encoding: *mut CEncoding) {
    if encoding.is_null() {
        return;
    }

    unsafe {
        drop(Box::from_raw(encoding));
    }
}

fn copy_slice<T: Copy>(source: &[T], destination: *mut T, length: usize) {
    if destination.is_null() || length == 0 {
        return;
    }

    let copy_len = length.min(source.len());
    unsafe {
        ptr::copy_nonoverlapping(source.as_ptr(), destination, copy_len);
    }
}

#[no_mangle]
pub extern "C" fn tokenizers_encoding_get_ids(
    encoding: *const CEncoding,
    buffer: *mut u32,
    length: usize,
) {
    if encoding.is_null() {
        return;
    }

    let encoding = unsafe { &*encoding };
    copy_slice(&encoding.ids, buffer, length);
}

#[no_mangle]
pub extern "C" fn tokenizers_encoding_get_tokens(
    encoding: *const CEncoding,
    buffer: *mut *mut c_char,
    length: usize,
) {
    if encoding.is_null() || buffer.is_null() {
        return;
    }

    let encoding = unsafe { &*encoding };
    let max = length.min(encoding.tokens.len());

    for index in 0..length {
        unsafe {
            *buffer.add(index) = ptr::null_mut();
        }
    }

    for (index, token) in encoding.tokens.iter().take(max).enumerate() {
        match CString::new(token.as_str()) {
            Ok(value) => unsafe {
                *buffer.add(index) = value.into_raw();
            },
            Err(_) => unsafe {
                *buffer.add(index) = ptr::null_mut();
            },
        }
    }
}

#[no_mangle]
pub extern "C" fn tokenizers_encoding_get_offsets(
    encoding: *const CEncoding,
    buffer: *mut u32,
    length: usize,
) {
    if encoding.is_null() || buffer.is_null() {
        return;
    }

    let encoding = unsafe { &*encoding };
    let required = encoding.offsets.len() * 2;
    let max = length.min(required);

    for (index, (start, end)) in encoding.offsets.iter().enumerate() {
        let position = index * 2;
        if position + 1 >= max {
            break;
        }

        unsafe {
            *buffer.add(position) = *start;
            *buffer.add(position + 1) = *end;
        }
    }
}

#[no_mangle]
pub extern "C" fn tokenizers_encoding_get_type_ids(
    encoding: *const CEncoding,
    buffer: *mut u32,
    length: usize,
) {
    if encoding.is_null() {
        return;
    }

    let encoding = unsafe { &*encoding };
    copy_slice(&encoding.type_ids, buffer, length);
}

#[no_mangle]
pub extern "C" fn tokenizers_encoding_get_attention_mask(
    encoding: *const CEncoding,
    buffer: *mut u32,
    length: usize,
) {
    if encoding.is_null() {
        return;
    }

    let encoding = unsafe { &*encoding };
    copy_slice(&encoding.attention_mask, buffer, length);
}

#[no_mangle]
pub extern "C" fn tokenizers_encoding_get_special_tokens_mask(
    encoding: *const CEncoding,
    buffer: *mut u32,
    length: usize,
) {
    if encoding.is_null() {
        return;
    }

    let encoding = unsafe { &*encoding };
    copy_slice(&encoding.special_tokens_mask, buffer, length);
}

#[no_mangle]
pub extern "C" fn tokenizers_encoding_get_word_ids(
    encoding: *const CEncoding,
    buffer: *mut i32,
    length: usize,
) {
    if encoding.is_null() || buffer.is_null() {
        return;
    }

    let encoding = unsafe { &*encoding };
    let max = length.min(encoding.word_ids.len());

    for (index, value) in encoding.word_ids.iter().take(max).enumerate() {
        let mapped = value.map(|id| id as i32).unwrap_or(-1);
        unsafe {
            *buffer.add(index) = mapped;
        }
    }
}

#[no_mangle]
pub extern "C" fn tokenizers_encoding_get_sequence_ids(
    encoding: *const CEncoding,
    buffer: *mut i32,
    length: usize,
) {
    if encoding.is_null() || buffer.is_null() {
        return;
    }

    let encoding = unsafe { &*encoding };
    let max = length.min(encoding.sequence_ids.len());

    for (index, value) in encoding.sequence_ids.iter().take(max).enumerate() {
        let mapped = value.map(|id| id as i32).unwrap_or(-1);
        unsafe {
            *buffer.add(index) = mapped;
        }
    }
}

#[no_mangle]
pub extern "C" fn tokenizers_encoding_copy_numeric(
    encoding: *const CEncoding,
    destination: *mut CEncodingNumericDest,
    length: usize,
    status: *mut c_int,
) -> c_int {
    if encoding.is_null() || destination.is_null() {
        store_error("tokenizers_encoding_copy_numeric received null pointer");
        set_status(status, 1);
        return 0;
    }

    let encoding = unsafe { &*encoding };
    let count = encoding.len();

    if count > length {
        store_error("tokenizers_encoding_copy_numeric received insufficient destination length");
        set_status(status, 2);
        return 0;
    }

    let destination = unsafe { &mut *destination };

    if destination.ids.is_null()
        || destination.type_ids.is_null()
        || destination.attention_mask.is_null()
        || destination.special_tokens_mask.is_null()
        || destination.offsets.is_null()
        || destination.word_ids.is_null()
        || destination.sequence_ids.is_null()
    {
        store_error("tokenizers_encoding_copy_numeric received null destination buffer");
        set_status(status, 3);
        return 0;
    }

    copy_slice(&encoding.ids, destination.ids, count);
    copy_slice(&encoding.type_ids, destination.type_ids, count);
    copy_slice(&encoding.attention_mask, destination.attention_mask, count);
    copy_slice(&encoding.special_tokens_mask, destination.special_tokens_mask, count);

    for (index, (start, end)) in encoding.offsets.iter().enumerate() {
        unsafe {
            *destination.offsets.add(index) = CEncodingOffset {
                start: *start,
                end: *end,
            };
        }
    }

    for (index, word_id) in encoding.word_ids.iter().enumerate() {
        let mapped = match word_id {
            Some(value) => {
                if *value > i32::MAX as u32 {
                    store_error("tokenizers_encoding_copy_numeric encountered word id overflow");
                    set_status(status, 4);
                    return 0;
                }
                *value as i32
            }
            None => -1,
        };

        unsafe {
            *destination.word_ids.add(index) = mapped;
        }
    }

    for (index, sequence_id) in encoding.sequence_ids.iter().enumerate() {
        let mapped = match sequence_id {
            Some(value) => {
                if *value > i32::MAX as usize {
                    store_error("tokenizers_encoding_copy_numeric encountered sequence id overflow");
                    set_status(status, 5);
                    return 0;
                }
                *value as i32
            }
            None => -1,
        };

        unsafe {
            *destination.sequence_ids.add(index) = mapped;
        }
    }

    clear_error();
    set_status(status, 0);
    count as c_int
}

#[no_mangle]
pub extern "C" fn tokenizers_encoding_get_overflowing_count(encoding: *const CEncoding) -> usize {
    if encoding.is_null() {
        return 0;
    }

    let encoding = unsafe { &*encoding };
    encoding.overflowing.len()
}

#[no_mangle]
pub extern "C" fn tokenizers_encoding_get_overflowing(
    encoding: *const CEncoding,
    index: usize,
    length: *mut usize,
    status: *mut c_int,
) -> *mut CEncoding {
    if encoding.is_null() {
        store_error("tokenizers_encoding_get_overflowing received null pointer");
        set_status(status, 1);
        return ptr::null_mut();
    }

    let encoding = unsafe { &*encoding };

    match encoding.overflowing.get(index).cloned() {
        Some(value) => {
            set_length(length, value.len());
            clear_error();
            set_status(status, 0);
            Box::into_raw(Box::new(value))
        }
        None => {
            store_error("tokenizers_encoding_get_overflowing index out of range");
            set_status(status, 2);
            ptr::null_mut()
        }
    }
}

#[no_mangle]
pub extern "C" fn tokenizers_token_to_id(
    tokenizer: *const CTokenizer,
    token: *const c_char,
    status: *mut c_int,
) -> c_int {
    if tokenizer.is_null() {
        store_error("tokenizers_token_to_id received null tokenizer");
        set_status(status, 1);
        return -1;
    }

    let token_text = match read_required_utf8(token) {
        Ok(text) => text,
        Err(message) => {
            store_error(message);
            set_status(status, 2);
            return -1;
        }
    };

    let tokenizer = unsafe { &*tokenizer };

    match tokenizer.inner.token_to_id(&token_text) {
        Some(id) => {
            clear_error();
            set_status(status, 0);
            id as c_int
        }
        None => {
            store_error("tokenizers_token_to_id: token not found");
            set_status(status, 3);
            -1
        }
    }
}

#[no_mangle]
pub extern "C" fn tokenizers_id_to_token(
    tokenizer: *const CTokenizer,
    id: u32,
    status: *mut c_int,
) -> *mut c_char {
    if tokenizer.is_null() {
        store_error("tokenizers_id_to_token received null tokenizer");
        set_status(status, 1);
        return ptr::null_mut();
    }

    let tokenizer = unsafe { &*tokenizer };

    match tokenizer.inner.id_to_token(id) {
        Some(token) => match CString::new(token) {
            Ok(value) => {
                clear_error();
                set_status(status, 0);
                value.into_raw()
            }
            Err(_) => {
                store_error("tokenizers_id_to_token failed to allocate CString");
                set_status(status, 2);
                ptr::null_mut()
            }
        },
        None => {
            store_error("tokenizers_id_to_token: id not found");
            set_status(status, 3);
            ptr::null_mut()
        }
    }
}

#[no_mangle]
pub extern "C" fn tokenizers_get_config(
    tokenizer: *const CTokenizer,
    pretty: bool,
    status: *mut c_int,
) -> *mut c_char {
    if tokenizer.is_null() {
        store_error("tokenizers_get_config received null tokenizer");
        set_status(status, 1);
        return ptr::null_mut();
    }

    let tokenizer = unsafe { &*tokenizer };

    match tokenizer.inner.to_string(pretty) {
        Ok(json) => match CString::new(json) {
            Ok(value) => {
                clear_error();
                set_status(status, 0);
                value.into_raw()
            }
            Err(_) => {
                store_error("tokenizers_get_config failed to allocate CString");
                set_status(status, 2);
                ptr::null_mut()
            }
        },
        Err(err) => {
            store_error(&format!("tokenizers_get_config failed: {err}"));
            set_status(status, 3);
            ptr::null_mut()
        }
    }
}

#[no_mangle]
pub extern "C" fn tokenizers_decode(
    tokenizer: *const CTokenizer,
    ids: *const u32,
    length: usize,
    skip_special_tokens: bool,
    status: *mut c_int,
) -> *mut c_char {
    if tokenizer.is_null() {
        store_error("tokenizers_decode received null tokenizer");
        set_status(status, 1);
        return ptr::null_mut();
    }

    if length > 0 && ids.is_null() {
        store_error("tokenizers_decode received null ids pointer");
        set_status(status, 2);
        return ptr::null_mut();
    }

    let tokenizer = unsafe { &*tokenizer };
    let tokens = if length == 0 {
        &[][..]
    } else {
        unsafe { std::slice::from_raw_parts(ids, length) }
    };

    match tokenizer.inner.decode(tokens, skip_special_tokens) {
        Ok(result) => match CString::new(result) {
            Ok(value) => {
                clear_error();
                set_status(status, 0);
                value.into_raw()
            }
            Err(_) => {
                store_error("tokenizers_decode failed to allocate CString");
                set_status(status, 3);
                ptr::null_mut()
            }
        },
        Err(err) => {
            store_error(&format!("tokenizers_decode failed: {err}"));
            set_status(status, 4);
            ptr::null_mut()
        }
    }
}

#[no_mangle]
pub extern "C" fn tokenizers_decode_batch_flat(
    tokenizer: *const CTokenizer,
    tokens: *const u32,
    total_length: usize,
    lengths: *const usize,
    count: usize,
    skip_special_tokens: bool,
    output: *mut *mut c_char,
    status: *mut c_int,
) -> c_int {
    if tokenizer.is_null() {
        store_error("tokenizers_decode_batch_flat received null tokenizer");
        set_status(status, 1);
        return 0;
    }

    if output.is_null() || lengths.is_null() {
        store_error("tokenizers_decode_batch_flat received null buffer");
        set_status(status, 2);
        return 0;
    }

    if total_length > 0 && tokens.is_null() {
        store_error("tokenizers_decode_batch_flat received null tokens pointer");
        set_status(status, 3);
        return 0;
    }

    let tokenizer = unsafe { &*tokenizer };
    let lengths_slice = unsafe { std::slice::from_raw_parts(lengths, count) };
    let output_slice = unsafe { std::slice::from_raw_parts_mut(output, count) };

    for slot in output_slice.iter_mut() {
        *slot = ptr::null_mut();
    }

    let tokens_slice = if total_length == 0 {
        &[][..]
    } else {
        unsafe { std::slice::from_raw_parts(tokens, total_length) }
    };

    let mut borrowed = Vec::with_capacity(count);
    let mut offset = 0usize;

    for &length in lengths_slice.iter() {
        if length == 0 {
            borrowed.push(&tokens_slice[offset..offset]);
            continue;
        }

        let end = offset.saturating_add(length);
        if end > tokens_slice.len() {
            store_error("tokenizers_decode_batch_flat detected inconsistent lengths");
            set_status(status, 4);
            return 0;
        }

        borrowed.push(&tokens_slice[offset..end]);
        offset = end;
    }

    match tokenizer
        .inner
        .decode_batch(borrowed.as_slice(), skip_special_tokens)
    {
        Ok(results) => {
            if results.len() != count {
                store_error("tokenizers_decode_batch_flat returned unexpected result count");
                set_status(status, 5);
                return 0;
            }

            for (index, text) in results.into_iter().enumerate() {
                match CString::new(text) {
                    Ok(value) => {
                        output_slice[index] = value.into_raw();
                    }
                    Err(_) => {
                        for allocated in output_slice.iter_mut().take(index) {
                            if !allocated.is_null() {
                                unsafe {
                                    drop(CString::from_raw(*allocated));
                                }
                                *allocated = ptr::null_mut();
                            }
                        }

                        store_error("tokenizers_decode_batch_flat failed to allocate CString");
                        set_status(status, 6);
                        return 0;
                    }
                }
            }

            clear_error();
            set_status(status, 0);
            count as c_int
        }
        Err(err) => {
            store_error(&format!("tokenizers_decode_batch_flat failed: {err}"));
            set_status(status, 7);
            0
        }
    }
}

#[no_mangle]
pub extern "C" fn tokenizers_apply_chat_template(
    tokenizer: *const CTokenizer,
    template: *const c_char,
    messages: *const c_char,
    variables: *const c_char,
    add_generation_prompt: bool,
    status: *mut c_int,
) -> *mut c_char {
    if tokenizer.is_null() {
        store_error("tokenizers_apply_chat_template received null tokenizer");
        set_status(status, 1);
        return ptr::null_mut();
    }

    let template_source = match read_required_utf8(template) {
        Ok(value) => value,
        Err(message) => {
            store_error(message);
            set_status(status, 2);
            return ptr::null_mut();
        }
    };

    let messages_payload = match read_required_utf8(messages) {
        Ok(value) => value,
        Err(message) => {
            store_error(message);
            set_status(status, 3);
            return ptr::null_mut();
        }
    };

    let variables_payload = match read_optional_utf8(variables) {
        Ok(value) => value,
        Err(message) => {
            store_error(message);
            set_status(status, 4);
            return ptr::null_mut();
        }
    };

    match render_chat_template(
        template_source.as_str(),
        messages_payload.as_str(),
        variables_payload.as_deref(),
        add_generation_prompt,
    ) {
        Ok(result) => match CString::new(result) {
            Ok(value) => {
                clear_error();
                set_status(status, 0);
                value.into_raw()
            }
            Err(_) => {
                store_error("tokenizers_apply_chat_template failed to allocate CString");
                set_status(status, 5);
                ptr::null_mut()
            }
        },
        Err(err) => {
            let message = match err {
                ChatTemplateError::InvalidMessages(reason)
                | ChatTemplateError::InvalidVariables(reason)
                | ChatTemplateError::Template(reason) => reason,
            };
            store_error(&message);
            set_status(status, 6);
            ptr::null_mut()
        }
    }
}

#[no_mangle]
pub extern "C" fn tokenizers_free_string(value: *mut c_char) {
    if value.is_null() {
        return;
    }

    unsafe {
        drop(CString::from_raw(value));
    }
}

#[no_mangle]
pub extern "C" fn tokenizers_enable_padding(
    tokenizer: *mut CTokenizer,
    direction: c_int,
    pad_id: u32,
    pad_type_id: u32,
    pad_token: *const c_char,
    length: c_int,
    pad_to_multiple_of: c_int,
    status: *mut c_int,
) -> c_int {
    if tokenizer.is_null() {
        store_error("tokenizers_enable_padding received null tokenizer");
        set_status(status, 1);
        return 0;
    }

    let direction = match direction {
        0 => PaddingDirection::Left,
        1 => PaddingDirection::Right,
        other => {
            store_error(&format!("tokenizers_enable_padding unknown direction: {other}"));
            set_status(status, 2);
            return 0;
        }
    };

    let pad_token_value = match read_optional_utf8(pad_token) {
        Ok(Some(value)) => value,
        Ok(None) => String::from("[PAD]"),
        Err(message) => {
            store_error(message);
            set_status(status, 3);
            return 0;
        }
    };

    let strategy = if length >= 0 {
        PaddingStrategy::Fixed(length as usize)
    } else {
        PaddingStrategy::BatchLongest
    };

    let pad_to_multiple = if pad_to_multiple_of > 0 {
        Some(pad_to_multiple_of as usize)
    } else {
        None
    };

    let params = PaddingParams {
        strategy,
        direction,
        pad_id,
        pad_type_id,
        pad_token: pad_token_value,
        pad_to_multiple_of: pad_to_multiple,
    };

    let tokenizer = unsafe { &mut *tokenizer };
    tokenizer.inner.with_padding(Some(params));
    clear_error();
    set_status(status, 0);
    1
}

#[no_mangle]
pub extern "C" fn tokenizers_disable_padding(
    tokenizer: *mut CTokenizer,
    status: *mut c_int,
) -> c_int {
    if tokenizer.is_null() {
        store_error("tokenizers_disable_padding received null tokenizer");
        set_status(status, 1);
        return 0;
    }

    let tokenizer = unsafe { &mut *tokenizer };
    tokenizer.inner.with_padding(None);
    clear_error();
    set_status(status, 0);
    1
}

#[no_mangle]
pub extern "C" fn tokenizers_get_padding(
    tokenizer: *const CTokenizer,
    status: *mut c_int,
) -> *mut c_char {
    if tokenizer.is_null() {
        store_error("tokenizers_get_padding received null tokenizer");
        set_status(status, 1);
        return ptr::null_mut();
    }

    let tokenizer = unsafe { &*tokenizer };

    match tokenizer.inner.get_padding() {
        Some(params) => match serde_json::to_string(params) {
            Ok(json) => match CString::new(json) {
                Ok(value) => {
                    clear_error();
                    set_status(status, 0);
                    value.into_raw()
                }
                Err(_) => {
                    store_error("tokenizers_get_padding failed to allocate CString");
                    set_status(status, 2);
                    ptr::null_mut()
                }
            },
            Err(err) => {
                store_error(&format!("tokenizers_get_padding failed: {err}"));
                set_status(status, 3);
                ptr::null_mut()
            }
        },
        None => {
            clear_error();
            set_status(status, 0);
            ptr::null_mut()
        }
    }
}

#[no_mangle]
pub extern "C" fn tokenizers_enable_truncation(
    tokenizer: *mut CTokenizer,
    max_length: usize,
    stride: usize,
    strategy: c_int,
    direction: c_int,
    status: *mut c_int,
) -> c_int {
    if tokenizer.is_null() {
        store_error("tokenizers_enable_truncation received null tokenizer");
        set_status(status, 1);
        return 0;
    }

    let strategy = match strategy {
        0 => TruncationStrategy::LongestFirst,
        1 => TruncationStrategy::OnlyFirst,
        2 => TruncationStrategy::OnlySecond,
        other => {
            store_error(&format!("tokenizers_enable_truncation unknown strategy: {other}"));
            set_status(status, 2);
            return 0;
        }
    };

    let direction = match direction {
        0 => TruncationDirection::Left,
        1 => TruncationDirection::Right,
        other => {
            store_error(&format!("tokenizers_enable_truncation unknown direction: {other}"));
            set_status(status, 3);
            return 0;
        }
    };

    let tokenizer = unsafe { &mut *tokenizer };
    let params = TruncationParams {
        max_length,
        stride,
        strategy,
        direction,
    };

    match tokenizer.inner.with_truncation(Some(params)) {
        Ok(_) => {
            clear_error();
            set_status(status, 0);
            1
        }
        Err(err) => {
            store_error(&format!("tokenizers_enable_truncation failed: {err}"));
            set_status(status, 4);
            0
        }
    }
}

#[no_mangle]
pub extern "C" fn tokenizers_disable_truncation(
    tokenizer: *mut CTokenizer,
    status: *mut c_int,
) -> c_int {
    if tokenizer.is_null() {
        store_error("tokenizers_disable_truncation received null tokenizer");
        set_status(status, 1);
        return 0;
    }

    let tokenizer = unsafe { &mut *tokenizer };

    match tokenizer.inner.with_truncation(None) {
        Ok(_) => {
            clear_error();
            set_status(status, 0);
            1
        }
        Err(err) => {
            store_error(&format!("tokenizers_disable_truncation failed: {err}"));
            set_status(status, 2);
            0
        }
    }
}

#[no_mangle]
pub extern "C" fn tokenizers_get_truncation(
    tokenizer: *const CTokenizer,
    status: *mut c_int,
) -> *mut c_char {
    if tokenizer.is_null() {
        store_error("tokenizers_get_truncation received null tokenizer");
        set_status(status, 1);
        return ptr::null_mut();
    }

    let tokenizer = unsafe { &*tokenizer };

    match tokenizer.inner.get_truncation() {
        Some(params) => match serde_json::to_string(params) {
            Ok(json) => match CString::new(json) {
                Ok(value) => {
                    clear_error();
                    set_status(status, 0);
                    value.into_raw()
                }
                Err(_) => {
                    store_error("tokenizers_get_truncation failed to allocate CString");
                    set_status(status, 2);
                    ptr::null_mut()
                }
            },
            Err(err) => {
                store_error(&format!("tokenizers_get_truncation failed: {err}"));
                set_status(status, 3);
                ptr::null_mut()
            }
        },
        None => {
            clear_error();
            set_status(status, 0);
            ptr::null_mut()
        }
    }
}

use std::ffi::CString;
use std::os::raw::{c_char, c_int};
use std::ptr;

use tokenizers::tokenizer::EncodeInput;

use crate::encoding::{CEncoding, CEncodingNumericDest};
use crate::error::{clear_error, store_error};
use crate::tokenizer::CTokenizer;

use super::utils::{copy_slice, read_optional_utf8, read_required_utf8, set_length, set_status};

/// # Safety
/// `tokenizer`, `sequence`, `pair`, `length`, and `status` must be valid pointers, and string inputs must be UTF-8 encoded.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_encode(
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
        Ok(value) => value,
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

    match tokenizer
        .inner_mut()
        .encode_char_offsets(input, add_special_tokens)
    {
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

/// # Safety
/// `encoding` must be null or a pointer previously returned by this module and not freed by the caller.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_encoding_free(encoding: *mut CEncoding) {
    if encoding.is_null() {
        return;
    }

    unsafe {
        drop(Box::from_raw(encoding));
    }
}

/// # Safety
/// `encoding` must be a valid pointer and `buffer` must reference space for at least `length` elements.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_encoding_get_ids(
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

/// # Safety
/// `encoding` must be valid and `buffer` must provide storage for `length` pointers.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_encoding_get_tokens(
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

/// # Safety
/// `encoding` must be valid and `buffer` must reference space for at least `length` integers.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_encoding_get_offsets(
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

/// # Safety
/// `encoding` must be valid and `buffer` must contain space for at least `length` elements.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_encoding_get_type_ids(
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

/// # Safety
/// `encoding` must be valid and `buffer` must contain space for at least `length` elements.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_encoding_get_attention_mask(
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

/// # Safety
/// `encoding` must be valid and `buffer` must contain space for at least `length` elements.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_encoding_get_special_tokens_mask(
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

/// # Safety
/// `encoding` must be valid and `buffer` must have capacity for at least `length` elements.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_encoding_get_word_ids(
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

/// # Safety
/// `encoding` must be valid and `buffer` must have capacity for at least `length` elements.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_encoding_get_sequence_ids(
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

/// # Safety
/// `encoding`, `destination`, and `status` must be valid pointers and the destination buffers must be sized by the caller.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_encoding_copy_numeric(
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

    let count = match encoding.numeric_len_and_validate(length) {
        Ok(value) => value,
        Err(message) => {
            store_error(message);
            set_status(status, 2);
            return 0;
        }
    };

    let destination = unsafe { &mut *destination };
    if let Err(message) = destination.validate() {
        store_error(message);
        set_status(status, 3);
        return 0;
    }

    copy_slice(&encoding.ids, destination.ids, count);
    copy_slice(&encoding.type_ids, destination.type_ids, count);
    copy_slice(&encoding.attention_mask, destination.attention_mask, count);
    copy_slice(
        &encoding.special_tokens_mask,
        destination.special_tokens_mask,
        count,
    );

    encoding.fill_offsets(destination.offsets);

    if let Err(message) = encoding.fill_word_ids(destination.word_ids) {
        store_error(message);
        set_status(status, 4);
        return 0;
    }

    if let Err(message) = encoding.fill_sequence_ids(destination.sequence_ids) {
        store_error(message);
        set_status(status, 5);
        return 0;
    }

    clear_error();
    set_status(status, 0);
    count as c_int
}

/// # Safety
/// `encoding` must be a valid pointer to an encoding produced by this library.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_encoding_get_overflowing_count(
    encoding: *const CEncoding,
) -> usize {
    if encoding.is_null() {
        return 0;
    }

    let encoding = unsafe { &*encoding };
    encoding.overflowing.len()
}

/// # Safety
/// `encoding`, `length`, and `status` must be valid pointers; the caller is responsible for freeing the returned encoding.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_encoding_get_overflowing(
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

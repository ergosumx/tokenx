use std::collections::HashSet;
use std::ffi::CStr;
use std::os::raw::c_char;
use std::ptr;
use std::slice;

use anyhow::{Context, Result};
use tiktoken::{CoreBPE, Rank};

use crate::error::{clear_error, store_error};

#[repr(C)]
pub struct TtBytes {
    data: *const u8,
    len: usize,
}

#[repr(C)]
pub struct TtMergeEntry {
    bytes: TtBytes,
    rank: u32,
}

#[repr(C)]
pub struct TtSpecialToken {
    text: *const c_char,
    rank: u32,
}

pub struct CoreBpeHandle {
    inner: CoreBPE,
}

pub struct TtEncoding {
    tokens: Vec<Rank>,
}

pub struct TtString {
    value: Vec<u8>,
}

fn read_bytes(bytes: &TtBytes) -> Result<Vec<u8>> {
    if bytes.len == 0 {
        return Ok(Vec::new());
    }
    if bytes.data.is_null() {
        anyhow::bail!("Received null pointer for byte slice");
    }
    let data = unsafe { slice::from_raw_parts(bytes.data, bytes.len) };
    Ok(data.to_vec())
}

fn read_cstr(ptr: *const c_char) -> Result<String> {
    if ptr.is_null() {
        anyhow::bail!("Received null pointer for string");
    }
    let value = unsafe { CStr::from_ptr(ptr) };
    Ok(value.to_str().context("String was not valid UTF-8")?.to_owned())
}

fn convert_merges(raw: *const TtMergeEntry, len: usize) -> Result<Vec<(Vec<u8>, Rank)>> {
    if len == 0 {
        return Ok(Vec::new());
    }
    if raw.is_null() {
        anyhow::bail!("Received null pointer for merges array");
    }
    let items = unsafe { slice::from_raw_parts(raw, len) };
    let mut merges = Vec::with_capacity(len);
    for entry in items {
        let bytes = read_bytes(&entry.bytes)?;
        merges.push((bytes, entry.rank));
    }
    Ok(merges)
}

fn convert_specials(raw: *const TtSpecialToken, len: usize) -> Result<Vec<(String, Rank)>> {
    if len == 0 {
        return Ok(Vec::new());
    }
    if raw.is_null() {
        anyhow::bail!("Received null pointer for special tokens array");
    }
    let items = unsafe { slice::from_raw_parts(raw, len) };
    let mut specials = Vec::with_capacity(len);
    for entry in items {
        let text = read_cstr(entry.text)?;
        specials.push((text, entry.rank));
    }
    Ok(specials)
}

fn convert_allowed(raw: *const *const c_char, len: usize) -> Result<HashSet<String>> {
    if len == 0 {
        return Ok(HashSet::new());
    }
    if raw.is_null() {
        anyhow::bail!("Received null pointer for allowed special array");
    }
    let items = unsafe { slice::from_raw_parts(raw, len) };
    let mut values = HashSet::with_capacity(len);
    for ptr in items {
        let value = read_cstr(*ptr)?;
        values.insert(value);
    }
    Ok(values)
}

fn with_handle<F, R>(handle: *const CoreBpeHandle, action: F) -> Option<R>
where
    F: FnOnce(&CoreBpeHandle) -> Result<R>,
{
    if handle.is_null() {
        store_error("CoreBPE handle was null");
        return None;
    }
    let handle = unsafe { &*handle };
    match action(handle) {
        Ok(result) => {
            clear_error();
            Some(result)
        }
        Err(err) => {
            store_error(&err.to_string());
            None
        }
    }
}

#[no_mangle]
pub extern "C" fn ttk_core_bpe_new(
    pattern: *const c_char,
    merges: *const TtMergeEntry,
    merges_len: usize,
    specials: *const TtSpecialToken,
    specials_len: usize,
    explicit_vocab_size: u32,
) -> *mut CoreBpeHandle {
    clear_error();
    let pattern = match read_cstr(pattern) {
        Ok(value) => value,
        Err(err) => {
            store_error(&err.to_string());
            return ptr::null_mut();
        }
    };

    let merges = match convert_merges(merges, merges_len) {
        Ok(value) => value,
        Err(err) => {
            store_error(&err.to_string());
            return ptr::null_mut();
        }
    };

    let specials_vec = match convert_specials(specials, specials_len) {
        Ok(value) => value,
        Err(err) => {
            store_error(&err.to_string());
            return ptr::null_mut();
        }
    };

    if explicit_vocab_size > 0 {
        let expected = explicit_vocab_size as usize;
        let actual = merges.len() + specials_vec.len();
        if actual != expected {
            store_error(&format!(
                "explicit vocabulary size mismatch. expected {expected}, got {actual}"
            ));
            return ptr::null_mut();
        }

        let highest_rank = merges
            .iter()
            .map(|(_, rank)| *rank)
            .chain(specials_vec.iter().map(|(_, rank)| *rank))
            .max()
            .unwrap_or(0);

        if highest_rank + 1 != explicit_vocab_size {
            store_error(&format!(
                "explicit vocabulary size requires highest rank {expected_minus_one}, got {highest_rank}",
                expected_minus_one = explicit_vocab_size.checked_sub(1).unwrap_or(0)
            ));
            return ptr::null_mut();
        }
    }

    let creation = CoreBPE::new::<_, _, std::iter::Empty<(String, (Rank, Rank))>>(
        merges,
        specials_vec.clone(),
        &pattern,
    );

    let inner = match creation {
        Ok(value) => value,
        Err(err) => {
            store_error(&err.to_string());
            return ptr::null_mut();
        }
    };

    let handle = CoreBpeHandle { inner };

    Box::into_raw(Box::new(handle))
}

#[no_mangle]
pub extern "C" fn ttk_core_bpe_free(handle: *mut CoreBpeHandle) {
    if handle.is_null() {
        return;
    }
    unsafe { drop(Box::from_raw(handle)) };
}

#[no_mangle]
pub extern "C" fn ttk_core_bpe_encode_ordinary(
    handle: *const CoreBpeHandle,
    text: *const c_char,
) -> *mut TtEncoding {
    let text = match read_cstr(text) {
        Ok(value) => value,
        Err(err) => {
            store_error(&err.to_string());
            return ptr::null_mut();
        }
    };

    with_handle(handle, |core| {
        let tokens = core.inner.encode_ordinary(&text);
        Ok(Box::into_raw(Box::new(TtEncoding { tokens })))
    })
    .unwrap_or(ptr::null_mut())
}

#[no_mangle]
pub extern "C" fn ttk_core_bpe_encode(
    handle: *const CoreBpeHandle,
    text: *const c_char,
    allowed_special: *const *const c_char,
    allowed_special_len: usize,
) -> *mut TtEncoding {
    let text = match read_cstr(text) {
        Ok(value) => value,
        Err(err) => {
            store_error(&err.to_string());
            return ptr::null_mut();
        }
    };

    let allowed_special = match convert_allowed(allowed_special, allowed_special_len) {
        Ok(set) => set,
        Err(err) => {
            store_error(&err.to_string());
            return ptr::null_mut();
        }
    };

    with_handle(handle, |core| {
        let allowed: HashSet<&str> = allowed_special.iter().map(|s| s.as_str()).collect();
        let tokens = core
            .inner
            .encode(&text, &allowed)
            .map(|(tokens, _)| tokens)
            .map_err(|err| anyhow::anyhow!(err.message))?;
        Ok(Box::into_raw(Box::new(TtEncoding { tokens })))
    })
    .unwrap_or(ptr::null_mut())
}

#[no_mangle]
pub extern "C" fn ttk_core_bpe_decode(
    handle: *const CoreBpeHandle,
    tokens: *const u32,
    tokens_len: usize,
) -> *mut TtString {
    if tokens_len > 0 && tokens.is_null() {
        store_error("Received null pointer for tokens array");
        return ptr::null_mut();
    }
    let slice = if tokens_len == 0 {
        &[]
    } else {
        unsafe { slice::from_raw_parts(tokens, tokens_len) }
    };

    with_handle(handle, |core| {
        let bytes = core
            .inner
            .decode_bytes(slice)
            .map_err(|err| anyhow::anyhow!(err.to_string()))?;
        let text = String::from_utf8_lossy(&bytes).into_owned();
        Ok(Box::into_raw(Box::new(TtString {
            value: text.into_bytes(),
        })))
    })
    .unwrap_or(ptr::null_mut())
}

#[no_mangle]
pub extern "C" fn ttk_core_bpe_decode_bytes(
    handle: *const CoreBpeHandle,
    tokens: *const u32,
    tokens_len: usize,
) -> *mut TtString {
    if tokens_len > 0 && tokens.is_null() {
        store_error("Received null pointer for tokens array");
        return ptr::null_mut();
    }
    let slice = if tokens_len == 0 {
        &[]
    } else {
        unsafe { slice::from_raw_parts(tokens, tokens_len) }
    };

    with_handle(handle, |core| {
        let bytes = core
            .inner
            .decode_bytes(slice)
            .map_err(|err| anyhow::anyhow!(err.to_string()))?;
        Ok(Box::into_raw(Box::new(TtString { value: bytes })))
    })
    .unwrap_or(ptr::null_mut())
}


#[no_mangle]
pub extern "C" fn ttk_encoding_get_len(encoding: *const TtEncoding) -> usize {
    if encoding.is_null() {
        return 0;
    }
    let encoding = unsafe { &*encoding };
    encoding.tokens.len()
}

#[no_mangle]
pub extern "C" fn ttk_encoding_try_copy_tokens(
    encoding: *const TtEncoding,
    dest: *mut u32,
    len: usize,
) -> bool {
    if encoding.is_null() || dest.is_null() {
        return false;
    }
    let encoding = unsafe { &*encoding };
    if encoding.tokens.len() != len {
        return false;
    }
    unsafe {
        ptr::copy_nonoverlapping(encoding.tokens.as_ptr(), dest, len);
    }
    true
}

#[no_mangle]
pub extern "C" fn ttk_encoding_free(encoding: *mut TtEncoding) {
    if encoding.is_null() {
        return;
    }
    unsafe { drop(Box::from_raw(encoding)) };
}

#[no_mangle]
pub extern "C" fn ttk_string_get_len(value: *const TtString) -> usize {
    if value.is_null() {
        return 0;
    }
    let value = unsafe { &*value };
    value.value.len()
}

#[no_mangle]
pub extern "C" fn ttk_string_try_copy_bytes(
    value: *const TtString,
    dest: *mut u8,
    len: usize,
) -> bool {
    if value.is_null() || dest.is_null() {
        return false;
    }
    let value = unsafe { &*value };
    if value.value.len() != len {
        return false;
    }
    unsafe { ptr::copy_nonoverlapping(value.value.as_ptr(), dest, len) };
    true
}

#[no_mangle]
pub extern "C" fn ttk_string_free(value: *mut TtString) {
    if value.is_null() {
        return;
    }
    unsafe { drop(Box::from_raw(value)) };
}

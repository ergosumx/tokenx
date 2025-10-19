use std::ffi::CString;
use std::os::raw::{c_char, c_int};
use std::ptr;

use crate::error::{clear_error, store_error};
use crate::ffi::utils::{read_optional_utf8, read_required_utf8, set_status};
use crate::tokenizer::CTokenizer;

#[cfg(not(any(target_family = "wasm", target_os = "ios", target_os = "android")))]
use tokenizers::FromPretrainedParameters;
use tokenizers::Tokenizer;

/// # Safety
/// `json` must reference a null-terminated UTF-8 string and `status` must be a writable pointer.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_create(
    json: *const c_char,
    status: *mut c_int,
) -> *mut CTokenizer {
    match read_required_utf8(json) {
        Ok(content) => match Tokenizer::from_bytes(content.into_bytes()) {
            Ok(tokenizer) => {
                clear_error();
                set_status(status, 0);
                Box::into_raw(Box::new(CTokenizer::new(tokenizer)))
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

#[cfg(not(any(target_family = "wasm", target_os = "ios", target_os = "android")))]
/// # Safety
/// All pointer arguments must be valid and reference UTF-8 strings when non-null; `status` must be writable.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_from_pretrained(
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
    if let Some(revision_text) = revision_value {
        if !revision_text.is_empty() {
            params.revision = revision_text;
        }
    }
    params.token = token_value;

    match Tokenizer::from_pretrained(name.as_str(), Some(params)) {
        Ok(tokenizer) => {
            clear_error();
            set_status(status, 0);
            Box::into_raw(Box::new(CTokenizer::new(tokenizer)))
        }
        Err(err) => {
            store_error(&format!("tokenizers_from_pretrained failed: {err}"));
            set_status(status, 4);
            ptr::null_mut()
        }
    }
}

#[cfg(any(target_family = "wasm", target_os = "ios", target_os = "android"))]
/// # Safety
/// `status` must be a writable pointer supplied by the caller.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_from_pretrained(
    _identifier: *const c_char,
    _revision: *const c_char,
    _auth_token: *const c_char,
    status: *mut c_int,
) -> *mut CTokenizer {
    store_error("tokenizers_from_pretrained requires the 'http' feature");
    set_status(status, 1);
    ptr::null_mut()
}

/// # Safety
/// `tokenizer` must be null or a pointer previously returned by this library and not yet freed.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_free(tokenizer: *mut CTokenizer) {
    if tokenizer.is_null() {
        return;
    }

    unsafe {
        drop(Box::from_raw(tokenizer));
    }
}

/// # Safety
/// `value` must be null or a pointer previously obtained from this library via ownership transfer.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_free_string(value: *mut c_char) {
    if value.is_null() {
        return;
    }

    unsafe {
        drop(CString::from_raw(value));
    }
}

use std::ffi::CString;
use std::os::raw::{c_char, c_int};
use std::ptr;

use serde_json::to_string;

use crate::error::{clear_error, store_error};
use crate::tokenizer::CTokenizer;

use super::utils::{read_required_utf8, set_status};

/// # Safety
/// `tokenizer` must be a valid pointer, `token` must reference a null-terminated UTF-8 string, and `status` must be writable.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_token_to_id(
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

    let tokenizer = &*tokenizer;

    match tokenizer.inner().token_to_id(&token_text) {
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

/// # Safety
/// `tokenizer` and `status` must be valid writable pointers; the tokenizer must outlive this call.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_id_to_token(
    tokenizer: *const CTokenizer,
    id: u32,
    status: *mut c_int,
) -> *mut c_char {
    if tokenizer.is_null() {
        store_error("tokenizers_id_to_token received null tokenizer");
        set_status(status, 1);
        return ptr::null_mut();
    }

    let tokenizer = &*tokenizer;

    match tokenizer.inner().id_to_token(id) {
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

/// # Safety
/// `tokenizer` must be a valid pointer and `status` must be writable for the duration of the call.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_get_config(
    tokenizer: *const CTokenizer,
    pretty: bool,
    status: *mut c_int,
) -> *mut c_char {
    if tokenizer.is_null() {
        store_error("tokenizers_get_config received null tokenizer");
        set_status(status, 1);
        return ptr::null_mut();
    }

    let tokenizer = &*tokenizer;

    match tokenizer.inner().to_string(pretty) {
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

/// # Safety
/// `tokenizer` must be valid and `status` must be a writable pointer supplied by the caller.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_get_padding(
    tokenizer: *const CTokenizer,
    status: *mut c_int,
) -> *mut c_char {
    if tokenizer.is_null() {
        store_error("tokenizers_get_padding received null tokenizer");
        set_status(status, 1);
        return ptr::null_mut();
    }

    let tokenizer = &*tokenizer;

    match tokenizer.inner().get_padding() {
        Some(params) => match to_string(params) {
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

/// # Safety
/// `tokenizer` must be valid and `status` must be a writable pointer supplied by the caller.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_get_truncation(
    tokenizer: *const CTokenizer,
    status: *mut c_int,
) -> *mut c_char {
    if tokenizer.is_null() {
        store_error("tokenizers_get_truncation received null tokenizer");
        set_status(status, 1);
        return ptr::null_mut();
    }

    let tokenizer = &*tokenizer;

    match tokenizer.inner().get_truncation() {
        Some(params) => match to_string(params) {
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

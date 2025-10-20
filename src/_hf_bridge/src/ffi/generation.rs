use std::ffi::CString;
use std::os::raw::{c_char, c_int};
use std::ptr;

use crate::error::{clear_error, store_error};
use crate::generation::{
    normalize_generation_config, plan_logits_processors, plan_stopping_criteria,
};

use super::utils::{read_required_utf8, set_status};

#[no_mangle]
pub extern "C" fn tokenizers_normalize_generation_config(
    source: *const c_char,
    status: *mut c_int,
) -> *mut c_char {
    let payload = match read_required_utf8(source) {
        Ok(value) => value,
        Err(message) => {
            store_error(message);
            set_status(status, 1);
            return ptr::null_mut();
        }
    };

    match normalize_generation_config(payload.as_str()) {
        Ok(normalized) => match CString::new(normalized) {
            Ok(value) => {
                clear_error();
                set_status(status, 0);
                value.into_raw()
            }
            Err(_) => {
                store_error("tokenizers_normalize_generation_config failed to allocate CString");
                set_status(status, 2);
                ptr::null_mut()
            }
        },
        Err(err) => {
            store_error(&err.into_message());
            set_status(status, 3);
            ptr::null_mut()
        }
    }
}

#[no_mangle]
pub extern "C" fn tokenizers_plan_logits_processors(
    source: *const c_char,
    status: *mut c_int,
) -> *mut c_char {
    let payload = match read_required_utf8(source) {
        Ok(value) => value,
        Err(message) => {
            store_error(message);
            set_status(status, 1);
            return ptr::null_mut();
        }
    };

    match plan_logits_processors(payload.as_str()) {
        Ok(plan) => match CString::new(plan) {
            Ok(value) => {
                clear_error();
                set_status(status, 0);
                value.into_raw()
            }
            Err(_) => {
                store_error("tokenizers_plan_logits_processors failed to allocate CString");
                set_status(status, 2);
                ptr::null_mut()
            }
        },
        Err(err) => {
            store_error(&err.into_message());
            set_status(status, 3);
            ptr::null_mut()
        }
    }
}

#[no_mangle]
pub extern "C" fn tokenizers_plan_stopping_criteria(
    source: *const c_char,
    status: *mut c_int,
) -> *mut c_char {
    let payload = match read_required_utf8(source) {
        Ok(value) => value,
        Err(message) => {
            store_error(message);
            set_status(status, 1);
            return ptr::null_mut();
        }
    };

    match plan_stopping_criteria(payload.as_str()) {
        Ok(plan) => match CString::new(plan) {
            Ok(value) => {
                clear_error();
                set_status(status, 0);
                value.into_raw()
            }
            Err(_) => {
                store_error("tokenizers_plan_stopping_criteria failed to allocate CString");
                set_status(status, 2);
                ptr::null_mut()
            }
        },
        Err(err) => {
            store_error(&err.into_message());
            set_status(status, 3);
            ptr::null_mut()
        }
    }
}

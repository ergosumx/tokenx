use std::ffi::CString;
use std::os::raw::{c_char, c_int};
use std::ptr;

use crate::chat::{render_chat_template, ChatTemplateError};
use crate::error::{clear_error, store_error};
use crate::tokenizer::CTokenizer;

use super::utils::{read_optional_utf8, read_required_utf8, set_status};

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

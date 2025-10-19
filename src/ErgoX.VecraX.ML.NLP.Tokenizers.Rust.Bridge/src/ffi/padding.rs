use std::os::raw::{c_char, c_int};

use tokenizers::utils::padding::{PaddingDirection, PaddingParams, PaddingStrategy};

use crate::error::{clear_error, store_error};
use crate::tokenizer::CTokenizer;

use super::utils::{read_optional_utf8, set_status};

/// # Safety
/// `tokenizer`, `pad_token`, and `status` must be valid pointers supplied by the caller.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_enable_padding(
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
            store_error(&format!(
                "tokenizers_enable_padding unknown direction: {other}"
            ));
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
    tokenizer.inner_mut().with_padding(Some(params));
    clear_error();
    set_status(status, 0);
    1
}

/// # Safety
/// `tokenizer` and `status` must be valid pointers supplied by the caller.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_disable_padding(
    tokenizer: *mut CTokenizer,
    status: *mut c_int,
) -> c_int {
    if tokenizer.is_null() {
        store_error("tokenizers_disable_padding received null tokenizer");
        set_status(status, 1);
        return 0;
    }

    let tokenizer = unsafe { &mut *tokenizer };
    tokenizer.inner_mut().with_padding(None);
    clear_error();
    set_status(status, 0);
    1
}

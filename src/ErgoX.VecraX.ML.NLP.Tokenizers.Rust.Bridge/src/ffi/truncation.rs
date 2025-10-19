use std::os::raw::c_int;

use tokenizers::utils::truncation::{TruncationDirection, TruncationParams, TruncationStrategy};

use crate::error::{clear_error, store_error};
use crate::tokenizer::CTokenizer;

use super::utils::set_status;

/// # Safety
/// `tokenizer` and `status` must be valid pointers supplied by the caller.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_enable_truncation(
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
            store_error(&format!(
                "tokenizers_enable_truncation unknown strategy: {other}"
            ));
            set_status(status, 2);
            return 0;
        }
    };

    let direction = match direction {
        0 => TruncationDirection::Left,
        1 => TruncationDirection::Right,
        other => {
            store_error(&format!(
                "tokenizers_enable_truncation unknown direction: {other}"
            ));
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

    match tokenizer.inner_mut().with_truncation(Some(params)) {
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

/// # Safety
/// `tokenizer` and `status` must be valid pointers supplied by the caller.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_disable_truncation(
    tokenizer: *mut CTokenizer,
    status: *mut c_int,
) -> c_int {
    if tokenizer.is_null() {
        store_error("tokenizers_disable_truncation received null tokenizer");
        set_status(status, 1);
        return 0;
    }

    let tokenizer = unsafe { &mut *tokenizer };

    match tokenizer.inner_mut().with_truncation(None) {
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

use std::ffi::CString;
use std::os::raw::{c_char, c_int};
use std::ptr;

use crate::error::{clear_error, store_error};
use crate::tokenizer::CTokenizer;

use super::utils::set_status;

/// # Safety
/// `tokenizer`, `ids`, and `status` must be valid pointers; `ids` must reference at least `length` elements when `length > 0`.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_decode(
    tokenizer: *const CTokenizer,
    ids: *const u32,
    length: usize,
    skip_special_tokens: bool,
    status: *mut c_int,
) -> *mut c_char {
    let Some(tokenizer) = (unsafe { tokenizer.as_ref() }) else {
        store_error("tokenizers_decode received null tokenizer");
        set_status(status, 1);
        return ptr::null_mut();
    };

    if length > 0 && ids.is_null() {
        store_error("tokenizers_decode received null ids pointer");
        set_status(status, 2);
        return ptr::null_mut();
    }

    let tokens = if length == 0 {
        &[][..]
    } else {
        unsafe { std::slice::from_raw_parts(ids, length) }
    };

    match tokenizer.inner().decode(tokens, skip_special_tokens) {
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

/// # Safety
/// All pointer arguments must be valid; `tokens` must contain `total_length` elements and the `lengths` slice must describe `count` sequences.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_decode_batch_flat(
    tokenizer: *const CTokenizer,
    tokens: *const u32,
    total_length: usize,
    lengths: *const usize,
    count: usize,
    skip_special_tokens: bool,
    output: *mut *mut c_char,
    status: *mut c_int,
) -> c_int {
    let Some(tokenizer) = (unsafe { tokenizer.as_ref() }) else {
        store_error("tokenizers_decode_batch_flat received null tokenizer");
        set_status(status, 1);
        return 0;
    };

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
        .inner()
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

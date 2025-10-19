use std::ffi::CStr;
use tokenx_bridge::error_test_support::{clear_error, store_error};
use tokenx_bridge::tokenizers_get_last_error;

#[test]
fn store_error_makes_message_available() {
    store_error("boom");

    let pointer = tokenizers_get_last_error();
    assert!(!pointer.is_null(), "error pointer should not be null");

    let slice = unsafe { CStr::from_ptr(pointer) };
    assert_eq!(slice.to_str().unwrap(), "boom");
}

#[test]
fn clear_error_resets_state() {
    store_error("transient");
    clear_error();

    let pointer = tokenizers_get_last_error();
    assert!(pointer.is_null(), "clearing should remove error message");
}

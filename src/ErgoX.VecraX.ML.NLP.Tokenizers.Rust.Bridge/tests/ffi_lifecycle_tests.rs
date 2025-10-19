use std::ffi::CString;
use tokenx_bridge::ffi::lifecycle::{
    tokenizers_create,
    tokenizers_free,
    tokenizers_free_string,
};
use tokenx_bridge::ffi::test_helpers;

#[test]
fn tokenizers_create_builds_tokenizer_from_json() {
    let json = test_helpers::tokenizer_config_json();
    let mut status = -1;
    let tokenizer = tokenizers_create(json.as_ptr(), &mut status);

    assert_eq!(status, 0);
    assert!(!tokenizer.is_null());

    tokenizers_free(tokenizer);
}

#[test]
fn tokenizers_create_reports_invalid_pointer() {
    let mut status = -1;
    let tokenizer = tokenizers_create(std::ptr::null(), &mut status);
    assert!(tokenizer.is_null());
    assert_ne!(status, 0);
}

#[test]
fn tokenizers_free_string_drops_allocated_value() {
    let value = CString::new("sample").unwrap().into_raw();
    tokenizers_free_string(value);
}

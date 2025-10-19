use std::ffi::CString;
use std::ptr;
use tokenx_bridge::ffi::config::{
    tokenizers_get_config,
    tokenizers_get_padding,
    tokenizers_id_to_token,
    tokenizers_token_to_id,
};
use tokenx_bridge::ffi::lifecycle::tokenizers_free_string;
use tokenx_bridge::ffi::test_helpers;
use tokenx_bridge::CTokenizer;

#[test]
fn tokenizers_token_to_id_resolves_known_token() {
    let tokenizer = test_helpers::create_tokenizer();
    let mut tokenizer = tokenizer;
    let mut status = -1;
    let token = CString::new("hello").unwrap();
    let id = tokenizers_token_to_id(
        &mut tokenizer as *const CTokenizer,
        token.as_ptr(),
        ptr::addr_of_mut!(status),
    );

    assert_eq!(status, 0);
    assert!(id >= 0);
}

#[test]
fn tokenizers_token_to_id_returns_error_for_unknown_token() {
    let tokenizer = test_helpers::create_tokenizer();
    let mut tokenizer = tokenizer;
    let mut status = -1;
    let token = CString::new("unknown").unwrap();
    let id = tokenizers_token_to_id(
        &mut tokenizer as *const CTokenizer,
        token.as_ptr(),
        ptr::addr_of_mut!(status),
    );

    assert_eq!(id, -1);
    assert_ne!(status, 0);
}

#[test]
fn tokenizers_id_to_token_returns_expected_token() {
    let tokenizer = test_helpers::create_tokenizer();
    let mut tokenizer = tokenizer;
    let mut status = -1;
    let token_ptr = tokenizers_id_to_token(
        &mut tokenizer as *const CTokenizer,
        1,
        ptr::addr_of_mut!(status),
    );

    assert_eq!(status, 0);
    assert!(!token_ptr.is_null());

    tokenizers_free_string(token_ptr);
}

#[test]
fn tokenizers_get_config_returns_serialized_json() {
    let tokenizer = test_helpers::create_tokenizer();
    let mut tokenizer = tokenizer;
    let mut status = -1;
    let json_ptr = tokenizers_get_config(
        &mut tokenizer as *const CTokenizer,
        true,
        ptr::addr_of_mut!(status),
    );

    assert_eq!(status, 0);
    assert!(!json_ptr.is_null());

    tokenizers_free_string(json_ptr);
}

#[test]
fn tokenizers_get_padding_returns_null_when_not_configured() {
    let tokenizer = test_helpers::create_tokenizer();
    let mut tokenizer = tokenizer;
    let mut status = -1;
    let padding_ptr = tokenizers_get_padding(
        &mut tokenizer as *const CTokenizer,
        ptr::addr_of_mut!(status),
    );

    assert!(padding_ptr.is_null());
    assert_eq!(status, 0);
}

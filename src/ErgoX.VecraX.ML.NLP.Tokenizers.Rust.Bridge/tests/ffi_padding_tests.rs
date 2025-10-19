use std::ffi::CString;
use std::ptr;
use tokenx_bridge::ffi::config::tokenizers_get_padding;
use tokenx_bridge::ffi::lifecycle::tokenizers_free_string;
use tokenx_bridge::ffi::padding::{
    tokenizers_disable_padding,
    tokenizers_enable_padding,
};
use tokenx_bridge::ffi::test_helpers;
use tokenx_bridge::CTokenizer;

#[test]
fn tokenizers_enable_padding_configures_tokenizer() {
    let mut tokenizer = test_helpers::create_tokenizer();
    let pad_token = CString::new("[PAD]").unwrap();
    let mut status = -1;

    let result = tokenizers_enable_padding(
        &mut tokenizer as *mut CTokenizer,
        1,
        42,
        0,
        pad_token.as_ptr(),
        8,
        0,
        ptr::addr_of_mut!(status),
    );

    assert_eq!(result, 1);
    assert_eq!(status, 0);

    let mut padding_status = -1;
    let padding_ptr = tokenizers_get_padding(
        &tokenizer as *const CTokenizer,
        ptr::addr_of_mut!(padding_status),
    );

    assert_eq!(padding_status, 0);
    assert!(!padding_ptr.is_null());
    tokenizers_free_string(padding_ptr);
}

#[test]
fn tokenizers_disable_padding_clears_configuration() {
    let mut tokenizer = test_helpers::create_tokenizer();
    tokenizers_enable_padding(
        &mut tokenizer as *mut CTokenizer,
        0,
        0,
        0,
        ptr::null(),
        -1,
        0,
        ptr::null_mut(),
    );

    let mut status = -1;
    let result = tokenizers_disable_padding(
        &mut tokenizer as *mut CTokenizer,
        ptr::addr_of_mut!(status),
    );

    assert_eq!(result, 1);
    assert_eq!(status, 0);

    let mut padding_status = -1;
    let padding_ptr = tokenizers_get_padding(
        &tokenizer as *const CTokenizer,
        ptr::addr_of_mut!(padding_status),
    );

    assert_eq!(padding_status, 0);
    assert!(padding_ptr.is_null());
}

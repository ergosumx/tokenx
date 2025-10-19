use std::ffi::{c_char, CString};
use std::ptr;
use tokenx_bridge::ffi::config::tokenizers_token_to_id;
use tokenx_bridge::ffi::decode::{
    tokenizers_decode,
    tokenizers_decode_batch_flat,
};
use tokenx_bridge::ffi::lifecycle::tokenizers_free_string;
use tokenx_bridge::ffi::test_helpers;
use tokenx_bridge::CTokenizer;

#[test]
fn tokenizers_decode_round_trips_tokens() {
    let tokenizer = test_helpers::create_tokenizer();
    let ids: Vec<u32> = ["hello", "world"]
        .iter()
        .map(|token| {
            let token_cstr = CString::new(*token).unwrap();
            let mut lookup_status = -1;
            let id = tokenizers_token_to_id(
                &tokenizer as *const CTokenizer,
                token_cstr.as_ptr(),
                ptr::addr_of_mut!(lookup_status),
            );
            assert_eq!(lookup_status, 0, "token lookup should succeed");
            id as u32
        })
        .collect();

    let mut status = -1;
    let decoded = tokenizers_decode(
        &tokenizer as *const CTokenizer,
        ids.as_ptr(),
        ids.len(),
        true,
        ptr::addr_of_mut!(status),
    );

    assert_eq!(status, 0);
    assert!(!decoded.is_null());

    tokenizers_free_string(decoded);
}

#[test]
fn tokenizers_decode_batch_flat_decodes_multiple_sequences() {
    let tokenizer = test_helpers::create_tokenizer();
    let hello_token = CString::new("hello").unwrap();
    let world_token = CString::new("world").unwrap();
    let mut lookup_status = -1;
    let hello = tokenizers_token_to_id(
        &tokenizer as *const CTokenizer,
        hello_token.as_ptr(),
        ptr::addr_of_mut!(lookup_status),
    ) as u32;
    assert_eq!(lookup_status, 0, "hello token lookup should succeed");
    lookup_status = -1;
    let world = tokenizers_token_to_id(
        &tokenizer as *const CTokenizer,
        world_token.as_ptr(),
        ptr::addr_of_mut!(lookup_status),
    ) as u32;
    assert_eq!(lookup_status, 0, "world token lookup should succeed");

    let tokens = vec![hello, world, hello];
    let lengths = vec![2usize, 1];
    let mut output: Vec<*mut c_char> = vec![ptr::null_mut(); lengths.len()];
    let mut status = -1;

    let decoded_count = tokenizers_decode_batch_flat(
        &tokenizer as *const CTokenizer,
        tokens.as_ptr(),
        tokens.len(),
        lengths.as_ptr(),
        lengths.len(),
        true,
        output.as_mut_ptr(),
        ptr::addr_of_mut!(status),
    );

    assert_eq!(decoded_count as usize, lengths.len());
    assert_eq!(status, 0);

    for value in output {
        if !value.is_null() {
            tokenizers_free_string(value);
        }
    }
}

#[test]
fn tokenizers_decode_handles_null_tokenizer() {
    let mut status = -1;
    let result = tokenizers_decode(
        ptr::null(),
        ptr::null(),
        0,
        true,
        ptr::addr_of_mut!(status),
    );

    assert!(result.is_null());
    assert_ne!(status, 0);
}

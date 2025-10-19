use std::ffi::{c_char, CString};
use std::ptr;
use tokenx_bridge::ffi::encoding::{
    tokenizers_encode,
    tokenizers_encoding_copy_numeric,
    tokenizers_encoding_free,
    tokenizers_encoding_get_ids,
    tokenizers_encoding_get_overflowing,
    tokenizers_encoding_get_tokens,
};
use tokenx_bridge::ffi::lifecycle::tokenizers_free_string;
use tokenx_bridge::ffi::test_helpers;
use tokenx_bridge::{CEncoding, CEncodingNumericDest, CEncodingOffset, CTokenizer};

fn encode_sample_text(add_special_tokens: bool) -> (*mut CEncoding, usize, i32, CTokenizer) {
    let mut tokenizer = test_helpers::create_tokenizer();
    let mut status = -1;
    let mut length = 0usize;
    let text = CString::new("hello world").unwrap();
    let encoding = tokenizers_encode(
        &mut tokenizer as *mut CTokenizer,
        text.as_ptr(),
        ptr::null(),
        add_special_tokens,
        ptr::addr_of_mut!(length),
        ptr::addr_of_mut!(status),
    );

    (encoding, length, status, tokenizer)
}

#[test]
fn tokenizers_encode_produces_encoding() {
    let (encoding, length, status, _) = encode_sample_text(true);
    assert_eq!(status, 0);
    assert!(length > 0);
    assert!(!encoding.is_null());

    tokenizers_encoding_free(encoding);
}

#[test]
fn tokenizers_encoding_get_ids_populates_buffer() {
    let (encoding, length, status, _) = encode_sample_text(true);
    assert_eq!(status, 0);

    let mut ids = vec![0u32; length];
    tokenizers_encoding_get_ids(encoding, ids.as_mut_ptr(), ids.len());
    tokenizers_encoding_free(encoding);

    assert!(ids.iter().any(|id| *id != 0));
}

#[test]
fn tokenizers_encoding_get_tokens_allocates_cstrings() {
    let (encoding, length, status, _) = encode_sample_text(true);
    assert_eq!(status, 0);

    let mut tokens: Vec<*mut c_char> = vec![ptr::null_mut(); length];
    tokenizers_encoding_get_tokens(encoding, tokens.as_mut_ptr(), tokens.len());

    for token_ptr in tokens {
        if !token_ptr.is_null() {
            tokenizers_free_string(token_ptr);
        }
    }

    tokenizers_encoding_free(encoding);
}

#[test]
fn tokenizers_encoding_copy_numeric_copies_fields() {
    let (encoding, length, status, _) = encode_sample_text(true);
    assert_eq!(status, 0);

    let mut ids = vec![0u32; length];
    let mut type_ids = vec![0u32; length];
    let mut attention_mask = vec![0u32; length];
    let mut special_tokens_mask = vec![0u32; length];
    let mut offsets = vec![CEncodingOffset { start: 0, end: 0 }; length];
    let mut word_ids = vec![0i32; length];
    let mut sequence_ids = vec![0i32; length];
    let mut status = -1;

    let mut destination = CEncodingNumericDest {
        ids: ids.as_mut_ptr(),
        type_ids: type_ids.as_mut_ptr(),
        attention_mask: attention_mask.as_mut_ptr(),
        special_tokens_mask: special_tokens_mask.as_mut_ptr(),
        offsets: offsets.as_mut_ptr(),
        word_ids: word_ids.as_mut_ptr(),
        sequence_ids: sequence_ids.as_mut_ptr(),
    };

    let copied = tokenizers_encoding_copy_numeric(
        encoding,
        ptr::addr_of_mut!(destination),
        length,
        ptr::addr_of_mut!(status),
    );

    assert_eq!(copied as usize, length);
    assert_eq!(status, 0);
    assert!(ids.iter().any(|id| *id != 0));
    assert!(offsets.iter().any(|offset| offset.end > offset.start));

    tokenizers_encoding_free(encoding);
}

#[test]
fn tokenizers_encoding_get_overflowing_reports_missing_index() {
    let (encoding, _, status, _) = encode_sample_text(true);
    assert_eq!(status, 0);

    let mut length = 0usize;
    let mut call_status = -1;
    let overflow = tokenizers_encoding_get_overflowing(
        encoding,
        5,
        ptr::addr_of_mut!(length),
        ptr::addr_of_mut!(call_status),
    );

    assert!(overflow.is_null());
    assert_ne!(call_status, 0);

    tokenizers_encoding_free(encoding);
}

use std::ptr;
use tokenx_bridge::encoding_test_support as encoding_support;
use tokenx_bridge::{CEncodingNumericDest, CEncodingOffset};

#[test]
fn from_encoding_copies_core_fields() {
    let encoding = encoding_support::build_encoding_from_tokenizer();
    let ids = encoding_support::ids(&encoding);
    let tokens = encoding_support::tokens(&encoding);

    assert_eq!(ids.len(), 2);
    assert_eq!(tokens[0], "Hello");
    assert_eq!(tokens[1], "world");
}

#[test]
fn numeric_len_and_validate_detects_insufficient_capacity() {
    let encoding = encoding_support::build_encoding_from_tokenizer();
    let len = encoding_support::len(&encoding);
    let result = encoding_support::numeric_len_and_validate(&encoding, len - 1);
    assert!(result.is_err());
}

#[test]
fn fill_offsets_populates_destination_buffer() {
    let encoding = encoding_support::build_encoding_from_tokenizer();
    let mut buffer = vec![CEncodingOffset { start: 0, end: 0 }; encoding_support::len(&encoding)];
    let ptr = buffer.as_mut_ptr();
    encoding_support::fill_offsets(&encoding, ptr);
    assert!(buffer.iter().all(|offset| offset.end >= offset.start));
}

#[test]
fn fill_word_ids_detects_overflow() {
    let mut encoding = encoding_support::build_encoding_from_tokenizer();
    let len = encoding_support::len(&encoding);
    encoding_support::set_word_ids(&mut encoding, vec![Some(u32::MAX); len]);
    let mut buffer = [0i32; 1];
    let result = encoding_support::fill_word_ids(&encoding, buffer.as_mut_ptr());
    assert!(result.is_err());
}

#[test]
fn fill_sequence_ids_detects_overflow() {
    let mut encoding = encoding_support::build_encoding_from_tokenizer();
    let len = encoding_support::len(&encoding);
    encoding_support::set_sequence_ids(&mut encoding, vec![Some(usize::MAX); len]);
    let mut buffer = [0i32; 1];
    let result = encoding_support::fill_sequence_ids(&encoding, buffer.as_mut_ptr());
    assert!(result.is_err());
}

#[test]
fn validate_detects_null_pointers() {
    let mut destination = CEncodingNumericDest {
        ids: ptr::null_mut(),
        type_ids: ptr::null_mut(),
        attention_mask: ptr::null_mut(),
        special_tokens_mask: ptr::null_mut(),
        offsets: ptr::null_mut(),
        word_ids: ptr::null_mut(),
        sequence_ids: ptr::null_mut(),
    };

    assert!(encoding_support::validate_destination(&destination).is_err());

    let mut ids = vec![0u32; 1];
    let mut type_ids = vec![0u32; 1];
    let mut attention_mask = vec![0u32; 1];
    let mut special_tokens_mask = vec![0u32; 1];
    let mut offsets = vec![CEncodingOffset::default(); 1];
    let mut word_ids = vec![0i32; 1];
    let mut sequence_ids = vec![0i32; 1];

    destination.ids = ids.as_mut_ptr();
    destination.type_ids = type_ids.as_mut_ptr();
    destination.attention_mask = attention_mask.as_mut_ptr();
    destination.special_tokens_mask = special_tokens_mask.as_mut_ptr();
    destination.offsets = offsets.as_mut_ptr();
    destination.word_ids = word_ids.as_mut_ptr();
    destination.sequence_ids = sequence_ids.as_mut_ptr();

    assert!(encoding_support::validate_destination(&destination).is_ok());
}

#[test]
fn copy_numeric_honors_destination_lengths() {
    let encoding = encoding_support::build_encoding_from_tokenizer();
    let length = encoding_support::len(&encoding);
    let mut ids = vec![0u32; length];
    let mut type_ids = vec![0u32; length];
    let mut attention_mask = vec![0u32; length];
    let mut special_tokens_mask = vec![0u32; length];
    let mut offsets = vec![CEncodingOffset { start: 0, end: 0 }; length];
    let mut word_ids = vec![0i32; length];
    let mut sequence_ids = vec![0i32; length];

    let destination = CEncodingNumericDest {
        ids: ids.as_mut_ptr(),
        type_ids: type_ids.as_mut_ptr(),
        attention_mask: attention_mask.as_mut_ptr(),
        special_tokens_mask: special_tokens_mask.as_mut_ptr(),
        offsets: offsets.as_mut_ptr(),
        word_ids: word_ids.as_mut_ptr(),
        sequence_ids: sequence_ids.as_mut_ptr(),
    };

    let result = encoding_support::numeric_len_and_validate(&encoding, length);
    assert!(result.is_ok());

    let copy_result = encoding_support::fill_word_ids(&encoding, word_ids.as_mut_ptr());
    assert!(copy_result.is_ok());

    let sequence_result = encoding_support::fill_sequence_ids(&encoding, sequence_ids.as_mut_ptr());
    assert!(sequence_result.is_ok());

    assert_eq!(destination.ids, ids.as_mut_ptr());
    assert_eq!(destination.word_ids, word_ids.as_mut_ptr());
}

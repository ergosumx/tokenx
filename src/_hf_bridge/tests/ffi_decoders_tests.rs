use std::ffi::{CStr, CString};
use std::ptr;

use serde_json::Value;
use tokenizers::decoders::bpe::BPEDecoder;
use tokenizers::decoders::byte_fallback::ByteFallback;
use tokenizers::decoders::byte_level::ByteLevel;
use tokenizers::decoders::ctc::CTC;
use tokenizers::decoders::fuse::Fuse;
use tokenizers::decoders::metaspace::Metaspace;
use tokenizers::decoders::sequence::Sequence;
use tokenizers::decoders::wordpiece::WordPiece;
use tokenizers::decoders::DecoderWrapper;
use tokenizers::normalizers::replace::Replace;
use tokenizers::pre_tokenizers::metaspace::PrependScheme;

use tokenx_bridge::ffi::config::tokenizers_get_config;
use tokenx_bridge::ffi::decoders::{
    tokenizers_decoder_free, tokenizers_decoder_from_json, tokenizers_decoder_get_type,
    tokenizers_decoder_to_json, tokenizers_tokenizer_clear_decoder,
    tokenizers_tokenizer_set_decoder,
};
use tokenx_bridge::ffi::lifecycle::tokenizers_free_string;
use tokenx_bridge::ffi::test_helpers;
use tokenx_bridge::CTokenizer;

fn decoder_json(wrapper: DecoderWrapper) -> CString {
    let json = serde_json::to_string(&wrapper).expect("decoder serialization must succeed");
    CString::new(json).expect("json must not contain null bytes")
}

fn bpe_decoder_json() -> CString {
    decoder_json(DecoderWrapper::from(BPEDecoder::default()))
}

fn byte_level_decoder_json() -> CString {
    decoder_json(DecoderWrapper::from(ByteLevel::default()))
}

fn word_piece_decoder_json() -> CString {
    decoder_json(DecoderWrapper::from(WordPiece::default()))
}

fn metaspace_decoder_json() -> CString {
    decoder_json(DecoderWrapper::from(Metaspace::new(
        '▁',
        PrependScheme::Always,
        true,
    )))
}

fn ctc_decoder_json() -> CString {
    decoder_json(DecoderWrapper::from(CTC::new("_".into(), " ".into(), true)))
}

fn fuse_decoder_json() -> CString {
    decoder_json(DecoderWrapper::from(Fuse::default()))
}

fn replace_decoder_json() -> CString {
    let replace = Replace::new("_", " ").expect("replace construction must succeed");
    decoder_json(DecoderWrapper::from(replace))
}

fn strip_decoder_json() -> CString {
    decoder_json(DecoderWrapper::from(
        tokenizers::decoders::strip::Strip::default(),
    ))
}

fn byte_fallback_decoder_json() -> CString {
    decoder_json(DecoderWrapper::from(ByteFallback::default()))
}

fn sequence_decoder_json() -> CString {
    let sequence = Sequence::new(vec![
        DecoderWrapper::from(ByteFallback::default()),
        DecoderWrapper::from(WordPiece::default()),
    ]);
    decoder_json(DecoderWrapper::from(sequence))
}

fn assert_decoder_type(tokenizer: &CTokenizer, expected: Option<&str>) {
    let mut status = -1;
    let config_ptr = unsafe {
        tokenizers_get_config(
            tokenizer as *const CTokenizer,
            false,
            ptr::addr_of_mut!(status),
        )
    };
    assert_eq!(status, 0, "tokenizer config retrieval should succeed");

    let config = unsafe {
        CStr::from_ptr(config_ptr)
            .to_str()
            .expect("tokenizer config should be utf-8")
            .to_owned()
    };
    unsafe {
        tokenizers_free_string(config_ptr);
    }

    let value: Value = serde_json::from_str(&config).expect("tokenizer json must be valid");

    match expected {
        Some(expected_type) => {
            let decoder_type = value["decoder"]["type"]
                .as_str()
                .expect("decoder type must be present");
            assert_eq!(decoder_type, expected_type);
        }
        None => {
            assert!(
                value.get("decoder").is_none_or(Value::is_null),
                "decoder entry should be null when cleared"
            );
        }
    }
}

fn exercise_decoder(json: CString, expected_config_type: &str, expected_runtime_type: &str) {
    let mut status = -1;
    let decoder_ptr =
        unsafe { tokenizers_decoder_from_json(json.as_ptr(), ptr::addr_of_mut!(status)) };
    assert_eq!(status, 0, "decoder creation should succeed");
    assert!(!decoder_ptr.is_null(), "decoder pointer must not be null");

    let mut tokenizer = test_helpers::create_tokenizer();
    let set_result = unsafe {
        tokenizers_tokenizer_set_decoder(
            &mut tokenizer as *mut CTokenizer,
            decoder_ptr,
            ptr::addr_of_mut!(status),
        )
    };
    assert_eq!(set_result, 1);
    assert_eq!(status, 0);

    assert_decoder_type(&tokenizer, Some(expected_config_type));

    let mut type_status = -1;
    let type_ptr =
        unsafe { tokenizers_decoder_get_type(decoder_ptr, ptr::addr_of_mut!(type_status)) };
    assert_eq!(type_status, 0);
    let decoder_type = unsafe {
        std::ffi::CStr::from_ptr(type_ptr)
            .to_str()
            .expect("decoder type should be utf-8")
            .to_owned()
    };
    assert_eq!(decoder_type, expected_runtime_type);
    unsafe {
        tokenizers_free_string(type_ptr);
    }

    unsafe {
        tokenizers_decoder_free(decoder_ptr);
    }
}

#[test]
fn tokenizers_decoder_supports_bpe() {
    exercise_decoder(bpe_decoder_json(), "BPEDecoder", "BPE");
}

#[test]
fn tokenizers_decoder_supports_byte_level() {
    exercise_decoder(byte_level_decoder_json(), "ByteLevel", "ByteLevel");
}

#[test]
fn tokenizers_decoder_supports_word_piece() {
    exercise_decoder(word_piece_decoder_json(), "WordPiece", "WordPiece");
}

#[test]
fn tokenizers_decoder_supports_metaspace() {
    exercise_decoder(metaspace_decoder_json(), "Metaspace", "Metaspace");
}

#[test]
fn tokenizers_decoder_supports_ctc() {
    exercise_decoder(ctc_decoder_json(), "CTC", "CTC");
}

#[test]
fn tokenizers_decoder_supports_fuse() {
    exercise_decoder(fuse_decoder_json(), "Fuse", "Fuse");
}

#[test]
fn tokenizers_decoder_supports_replace() {
    exercise_decoder(replace_decoder_json(), "Replace", "Replace");
}

#[test]
fn tokenizers_decoder_supports_strip() {
    exercise_decoder(strip_decoder_json(), "Strip", "Strip");
}

#[test]
fn tokenizers_decoder_supports_byte_fallback() {
    exercise_decoder(byte_fallback_decoder_json(), "ByteFallback", "ByteFallback");
}

#[test]
fn tokenizers_decoder_supports_sequence() {
    exercise_decoder(sequence_decoder_json(), "Sequence", "Sequence");
}

#[test]
fn tokenizers_decoder_can_be_cleared() {
    let json = byte_level_decoder_json();
    let mut status = -1;
    let decoder_ptr =
        unsafe { tokenizers_decoder_from_json(json.as_ptr(), ptr::addr_of_mut!(status)) };
    assert_eq!(status, 0);

    let mut tokenizer = test_helpers::create_tokenizer();
    let set_result = unsafe {
        tokenizers_tokenizer_set_decoder(
            &mut tokenizer as *mut CTokenizer,
            decoder_ptr,
            ptr::addr_of_mut!(status),
        )
    };
    assert_eq!(set_result, 1);
    assert_eq!(status, 0);

    let clear_result = unsafe {
        tokenizers_tokenizer_clear_decoder(
            &mut tokenizer as *mut CTokenizer,
            ptr::addr_of_mut!(status),
        )
    };
    assert_eq!(clear_result, 1);
    assert_eq!(status, 0);

    assert_decoder_type(&tokenizer, None);

    unsafe {
        tokenizers_decoder_free(decoder_ptr);
    }
}

#[test]
fn tokenizers_decoder_to_json_roundtrip() {
    let source = word_piece_decoder_json();
    let mut status = -1;
    let decoder_ptr =
        unsafe { tokenizers_decoder_from_json(source.as_ptr(), ptr::addr_of_mut!(status)) };
    assert_eq!(status, 0);

    let mut roundtrip_status = -1;
    let serialized_ptr = unsafe {
        tokenizers_decoder_to_json(decoder_ptr, true, ptr::addr_of_mut!(roundtrip_status))
    };
    assert_eq!(roundtrip_status, 0);

    let serialized = unsafe { std::ffi::CStr::from_ptr(serialized_ptr) }
        .to_str()
        .expect("serialized decoder should be utf-8")
        .to_owned();
    assert!(serialized.contains("WordPiece"));
    unsafe {
        tokenizers_free_string(serialized_ptr);
        tokenizers_decoder_free(decoder_ptr);
    }
}

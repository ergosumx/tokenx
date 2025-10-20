use std::ffi::{CStr, CString};
use std::ptr;

use ahash::AHashMap;
use serde_json::Value;
use tokenizers::models::bpe::BPE;
use tokenizers::models::unigram::Unigram;
use tokenizers::models::wordlevel::WordLevel;
use tokenizers::models::wordpiece::WordPiece;
use tokenizers::models::ModelWrapper;

use tokenx_bridge::ffi::config::tokenizers_get_config;
use tokenx_bridge::ffi::lifecycle::tokenizers_free_string;
use tokenx_bridge::ffi::models::{
    tokenizers_model_free, tokenizers_model_from_json, tokenizers_model_get_type,
    tokenizers_model_to_json, tokenizers_tokenizer_set_model,
};
use tokenx_bridge::ffi::test_helpers;
use tokenx_bridge::CTokenizer;

fn word_level_json() -> CString {
    let mut vocab = AHashMap::new();
    vocab.insert("[UNK]".to_string(), 0);
    vocab.insert("hello".to_string(), 1);
    vocab.insert("world".to_string(), 2);

    let model = WordLevel::builder()
        .vocab(vocab)
        .unk_token("[UNK]".into())
        .build()
        .expect("wordlevel model should build");

    let wrapper = ModelWrapper::from(model);
    let json = serde_json::to_string(&wrapper).expect("model serialization must succeed");
    CString::new(json).expect("json must not contain null bytes")
}

fn word_piece_json() -> CString {
    let mut vocab = AHashMap::new();
    vocab.insert("[UNK]".to_string(), 0);
    vocab.insert("he".to_string(), 1);
    vocab.insert("##llo".to_string(), 2);
    vocab.insert("world".to_string(), 3);

    let model = WordPiece::builder()
        .vocab(vocab)
        .unk_token("[UNK]".into())
        .continuing_subword_prefix("##".into())
        .build()
        .expect("wordpiece model should build");

    let wrapper = ModelWrapper::from(model);
    let json = serde_json::to_string(&wrapper).expect("model serialization must succeed");
    CString::new(json).expect("json must not contain null bytes")
}

fn bpe_json() -> CString {
    let vocab: AHashMap<_, _> = vec![
        ("[UNK]".to_string(), 0),
        ("h".to_string(), 1),
        ("e".to_string(), 2),
        ("l".to_string(), 3),
        ("o".to_string(), 4),
        ("lo".to_string(), 5),
        ("he".to_string(), 6),
        ("hel".to_string(), 7),
        ("hello".to_string(), 8),
    ]
    .into_iter()
    .collect();

    let merges = vec![
        ("h".to_string(), "e".to_string()),
        ("he".to_string(), "l".to_string()),
        ("hel".to_string(), "lo".to_string()),
    ];

    let model = BPE::new(vocab, merges);
    let wrapper = ModelWrapper::from(model);
    let json = serde_json::to_string(&wrapper).expect("model serialization must succeed");
    CString::new(json).expect("json must not contain null bytes")
}

fn unigram_json() -> CString {
    let vocab = vec![
        ("[UNK]".to_string(), 0.0),
        ("he".to_string(), -0.1),
        ("llo".to_string(), -0.2),
        ("world".to_string(), -0.3),
        ("hello".to_string(), -0.4),
    ];

    let model = Unigram::from(vocab, Some(0), false).expect("unigram model should build");
    let wrapper = ModelWrapper::from(model);
    let json = serde_json::to_string(&wrapper).expect("model serialization must succeed");
    CString::new(json).expect("json must not contain null bytes")
}

fn assert_model_type(tokenizer: &CTokenizer, expected: &str) {
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
    let model_type = value["model"]["type"]
        .as_str()
        .expect("model type must be present");
    assert_eq!(model_type, expected);
}

fn exercise_model(json: CString, expected_type: &str) {
    let mut status = -1;
    let model_ptr = unsafe { tokenizers_model_from_json(json.as_ptr(), ptr::addr_of_mut!(status)) };
    assert_eq!(status, 0, "model creation status should be zero");
    assert!(!model_ptr.is_null(), "model pointer should not be null");

    let mut tokenizer = test_helpers::create_tokenizer();
    let set_result = unsafe {
        tokenizers_tokenizer_set_model(
            &mut tokenizer as *mut CTokenizer,
            model_ptr,
            ptr::addr_of_mut!(status),
        )
    };

    assert_eq!(set_result, 1, "setting the model should return success");
    assert_eq!(status, 0, "setting the model status should be zero");

    assert_model_type(&tokenizer, expected_type);

    let mut type_status = -1;
    let type_ptr = unsafe { tokenizers_model_get_type(model_ptr, ptr::addr_of_mut!(type_status)) };
    assert_eq!(type_status, 0);
    let model_type = unsafe {
        std::ffi::CStr::from_ptr(type_ptr)
            .to_str()
            .expect("model type should be valid utf-8")
            .to_owned()
    };
    assert_eq!(model_type, expected_type);
    unsafe {
        tokenizers_free_string(type_ptr);
    }

    unsafe {
        tokenizers_model_free(model_ptr);
    }
}

#[test]
fn tokenizers_model_supports_word_level() {
    exercise_model(word_level_json(), "WordLevel");
}

#[test]
fn tokenizers_model_supports_word_piece() {
    exercise_model(word_piece_json(), "WordPiece");
}

#[test]
fn tokenizers_model_supports_bpe() {
    exercise_model(bpe_json(), "BPE");
}

#[test]
fn tokenizers_model_supports_unigram() {
    exercise_model(unigram_json(), "Unigram");
}

#[test]
fn tokenizers_model_to_json_roundtrip() {
    let source = word_level_json();
    let mut status = -1;
    let model_ptr =
        unsafe { tokenizers_model_from_json(source.as_ptr(), ptr::addr_of_mut!(status)) };
    assert_eq!(status, 0);

    let mut roundtrip_status = -1;
    let serialized_ptr =
        unsafe { tokenizers_model_to_json(model_ptr, false, ptr::addr_of_mut!(roundtrip_status)) };
    assert_eq!(roundtrip_status, 0);

    let serialized = unsafe { CStr::from_ptr(serialized_ptr) };
    assert!(!serialized.to_str().unwrap().is_empty());
    unsafe {
        tokenizers_free_string(serialized_ptr);
    }

    unsafe {
        tokenizers_model_free(model_ptr);
    }
}

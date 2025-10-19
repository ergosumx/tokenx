use serde_json::Value;
use std::ffi::{c_char, CString};
use std::ptr;
use tokenx_bridge::ffi::generation::{
    tokenizers_normalize_generation_config, tokenizers_plan_logits_processors,
    tokenizers_plan_stopping_criteria,
};

fn read_string(ptr_value: *mut c_char) -> String {
    assert!(!ptr_value.is_null());
    unsafe { CString::from_raw(ptr_value) }
        .into_string()
        .expect("result should be valid UTF-8")
}

#[test]
fn tokenizers_normalize_generation_config_returns_compact_json() {
    let payload = CString::new(r#"{"temperature": 0.5}"#).unwrap();
    let mut status = -1;
    let result =
        tokenizers_normalize_generation_config(payload.as_ptr(), ptr::addr_of_mut!(status));

    assert_eq!(status, 0);
    let normalized = read_string(result);
    assert_eq!(normalized, "{\"temperature\":0.5}");
}

#[test]
fn tokenizers_plan_logits_processors_emits_bindings() {
    let payload = CString::new(r#"{"temperature": 0.7}"#).unwrap();
    let mut status = -1;
    let result = tokenizers_plan_logits_processors(payload.as_ptr(), ptr::addr_of_mut!(status));

    assert_eq!(status, 0);
    let json = read_string(result);
    let value: Value = serde_json::from_str(&json).expect("valid JSON");
    assert!(!value.as_array().unwrap().is_empty());
}

#[test]
fn tokenizers_plan_stopping_criteria_emits_entries() {
    let payload = CString::new(r#"{"max_new_tokens": 16}"#).unwrap();
    let mut status = -1;
    let result = tokenizers_plan_stopping_criteria(payload.as_ptr(), ptr::addr_of_mut!(status));

    assert_eq!(status, 0);
    let json = read_string(result);
    let value: Value = serde_json::from_str(&json).expect("valid JSON");
    assert_eq!(value.as_array().unwrap().len(), 1);
}

#[test]
fn tokenizers_plan_logits_processors_handles_null_pointer() {
    let mut status = -1;
    let result = tokenizers_plan_logits_processors(ptr::null(), ptr::addr_of_mut!(status));

    assert!(result.is_null());
    assert_ne!(status, 0);
}

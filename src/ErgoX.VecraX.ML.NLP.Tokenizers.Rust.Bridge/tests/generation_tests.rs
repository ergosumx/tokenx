use serde_json::json;
use tokenx_bridge::generation::normalize_generation_config;
use tokenx_bridge::generation::test_support::{build_logits_bindings, build_stopping_criteria};

#[test]
fn normalize_generation_config_preserves_structure() {
    let input = r#"{"temperature":0.7,"nested":{"top_p":"0.9"}}"#;
    let normalized = normalize_generation_config(input).expect("normalization should succeed");
    assert_eq!(
        normalized,
        "{\"temperature\":0.7,\"nested\":{\"top_p\":\"0.9\"}}"
    );
}

#[test]
fn build_logits_bindings_skips_neutral_entries() {
    let object = json!({
        "temperature": 1.0,
        "top_p": 1.0,
        "repetition_penalty": 1.0
    })
    .as_object()
    .cloned()
    .unwrap();

    let bindings = build_logits_bindings(&object);
    assert!(bindings.is_empty());
}

#[test]
fn build_stopping_criteria_emits_expected_entries() {
    let object = json!({
        "max_new_tokens": 256,
        "stop_sequences": ["</s>", "###"]
    })
    .as_object()
    .cloned()
    .unwrap();

    let criteria = build_stopping_criteria(&object);
    assert_eq!(criteria.len(), 2);
}

use serde_json::Value;
use tokenx_bridge::generation::{
    normalize_generation_config, plan_logits_processors, plan_stopping_criteria,
};

#[test]
fn normalize_generation_config_returns_compact_json() {
    let input = r#"{ "temperature": 0.9, "nested": { "top_p": "0.8" } }"#;
    let normalized = normalize_generation_config(input).expect("normalization should succeed");
    assert_eq!(
        normalized,
        "{\"temperature\":0.9,\"nested\":{\"top_p\":\"0.8\"}}"
    );
}

#[test]
fn plan_logits_processors_emits_expected_bindings() {
    let input = r#"{
        "temperature": 0.7,
        "top_p": 0.9,
        "repetition_penalty": 1.1
    }"#;

    let plan = plan_logits_processors(input).expect("planner should succeed");
    let parsed: Value = serde_json::from_str(&plan).expect("plan must be valid JSON");
    let bindings = parsed.as_array().expect("plan must be an array");
    assert_eq!(bindings.len(), 3);

    let categories: Vec<String> = bindings
        .iter()
        .filter_map(|entry| {
            entry
                .get("category")
                .and_then(Value::as_str)
                .map(String::from)
        })
        .collect();
    assert!(categories
        .iter()
        .any(|category| category.eq_ignore_ascii_case("warper")));
    assert!(categories
        .iter()
        .any(|category| category.eq_ignore_ascii_case("processor")));
}

#[test]
fn plan_logits_processors_omits_neutral_values() {
    let input = r#"{
        "temperature": 1.0,
        "top_p": 1.0,
        "repetition_penalty": 1.0
    }"#;

    let plan = plan_logits_processors(input).expect("planner should succeed");
    let parsed: Value = serde_json::from_str(&plan).expect("plan must be valid JSON");
    let bindings = parsed.as_array().expect("plan must be an array");
    assert!(bindings.is_empty());
}

#[test]
fn plan_stopping_criteria_emits_expected_entries() {
    let input = r#"{
        "max_new_tokens": 128,
        "stop_sequences": ["END", "STOP"]
    }"#;

    let plan = plan_stopping_criteria(input).expect("planner should succeed");
    let parsed: Value = serde_json::from_str(&plan).expect("plan must be valid JSON");
    let criteria = parsed.as_array().expect("plan must be an array");
    assert_eq!(criteria.len(), 2);

    let has_max_tokens = criteria
        .iter()
        .any(|entry| entry.get("kind") == Some(&Value::String(String::from("max_new_tokens"))));
    let has_stop_sequences = criteria
        .iter()
        .any(|entry| entry.get("kind") == Some(&Value::String(String::from("stop_sequences"))));

    assert!(has_max_tokens);
    assert!(has_stop_sequences);
}

#[test]
fn plan_stopping_criteria_handles_missing_optional_fields() {
    let input = r#"{"max_new_tokens":0}"#;
    let plan = plan_stopping_criteria(input).expect("planner should succeed");
    let parsed: Value = serde_json::from_str(&plan).expect("plan must be valid JSON");
    assert!(parsed.as_array().unwrap().is_empty());
}

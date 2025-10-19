use serde_json::json;
use tokenx_bridge::generation::test_support::{
    collect_stop_sequences,
    convert_value_to_string,
    extract_numeric,
};

#[test]
fn extract_numeric_reads_case_insensitive_property() {
    let map = json!({ "Temperature": 0.75 }).as_object().cloned().unwrap();
    assert_eq!(extract_numeric(&map, "temperature"), Some(0.75));
}

#[test]
fn collect_stop_sequences_gathers_unique_values() {
    let map = json!({
        "stop_sequences": ["END", "END", "STOP"],
    })
    .as_object()
    .cloned()
    .unwrap();

    let collected = collect_stop_sequences(&map).unwrap();
    assert_eq!(collected, vec!["END".to_string(), "STOP".to_string()]);
}

#[test]
fn convert_value_to_string_trims_whitespace() {
    assert_eq!(
        convert_value_to_string(&json!("  value  ")),
        Some("value".to_string())
    );
    assert!(convert_value_to_string(&json!("   ")).is_none());
}

use serde_json::{Map, Value};

pub(crate) fn extract_numeric(map: &Map<String, Value>, key: &str) -> Option<f64> {
    let candidate = get_property(map, key)?;
    to_f64(candidate)
}

pub(crate) fn extract_integer(map: &Map<String, Value>, key: &str) -> Option<u64> {
    let candidate = get_property(map, key)?;
    to_u64(candidate)
}

pub(crate) fn collect_stop_sequences(map: &Map<String, Value>) -> Option<Vec<String>> {
    const CANDIDATES: [&str; 6] = [
        "stop_sequences",
        "stop_strings",
        "stop_words",
        "stop",
        "stopping_sequences",
        "stopping_strings",
    ];

    for key in CANDIDATES {
        if let Some(value) = get_property(map, key) {
            let sequences = value_to_string_list(value);
            if !sequences.is_empty() {
                return Some(sequences);
            }
        }
    }

    None
}

pub(crate) fn get_property<'a>(map: &'a Map<String, Value>, key: &str) -> Option<&'a Value> {
    map.get(key).or_else(|| {
        map.iter()
            .find(|(existing, _)| existing.eq_ignore_ascii_case(key))
            .map(|(_, value)| value)
    })
}

pub(crate) fn to_f64(value: &Value) -> Option<f64> {
    match value {
        Value::Number(number) => number
            .as_f64()
            .or_else(|| number.as_i64().map(|signed| signed as f64))
            .or_else(|| number.as_u64().map(|unsigned| unsigned as f64)),
        Value::String(text) => text.trim().parse::<f64>().ok(),
        _ => None,
    }
}

pub(crate) fn to_u64(value: &Value) -> Option<u64> {
    match value {
        Value::Number(number) => number.as_u64().or_else(|| {
            number
                .as_i64()
                .and_then(|signed| (signed >= 0).then_some(signed as u64))
        }),
        Value::String(text) => text.trim().parse::<f64>().ok().and_then(|parsed| {
            if parsed.is_finite() && parsed >= 0.0 {
                Some(parsed.trunc() as u64)
            } else {
                None
            }
        }),
        _ => None,
    }
}

pub(crate) fn value_to_string_list(value: &Value) -> Vec<String> {
    match value {
        Value::Array(items) => {
            let mut unique: Vec<String> = Vec::with_capacity(items.len());
            for item in items {
                if let Some(text) = convert_value_to_string(item) {
                    if !unique.iter().any(|existing| existing == &text) {
                        unique.push(text);
                    }
                }
            }
            unique
        }
        _ => convert_value_to_string(value).map_or_else(Vec::new, |text| vec![text]),
    }
}

pub(crate) fn convert_value_to_string(value: &Value) -> Option<String> {
    match value {
        Value::String(text) => {
            let trimmed = text.trim();
            if trimmed.is_empty() {
                None
            } else {
                Some(trimmed.to_string())
            }
        }
        Value::Number(number) => Some(number.to_string()),
        Value::Bool(flag) => Some(flag.to_string()),
        _ => None,
    }
}

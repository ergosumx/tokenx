use serde_json::{Map, Value};

mod helpers;
mod models;

use helpers::{collect_stop_sequences, extract_integer, extract_numeric};
use models::{LogitsBinding, StoppingCriterion};

const EPSILON: f64 = 1e-9;

#[derive(Debug)]
pub enum GenerationConfigError {
    Parse(String),
    Serialize(String),
}

impl GenerationConfigError {
    pub fn into_message(self) -> String {
        match self {
            GenerationConfigError::Parse(reason) => {
                format!("failed to parse generation_config.json: {reason}")
            }
            GenerationConfigError::Serialize(reason) => {
                format!("failed to serialize generation configuration: {reason}")
            }
        }
    }
}

pub fn normalize_generation_config(source: &str) -> Result<String, GenerationConfigError> {
    let parsed: Value = serde_json::from_str(source)
        .map_err(|err| GenerationConfigError::Parse(err.to_string()))?;

    serde_json::to_string(&parsed).map_err(|err| GenerationConfigError::Serialize(err.to_string()))
}

pub fn plan_logits_processors(source: &str) -> Result<String, GenerationConfigError> {
    let parsed: Value = serde_json::from_str(source)
        .map_err(|err| GenerationConfigError::Parse(err.to_string()))?;

    let object = parsed.as_object().ok_or_else(|| {
        GenerationConfigError::Parse(String::from(
            "generation configuration must be a JSON object",
        ))
    })?;

    let bindings = build_logits_bindings(object);
    serde_json::to_string(&bindings)
        .map_err(|err| GenerationConfigError::Serialize(err.to_string()))
}

pub fn plan_stopping_criteria(source: &str) -> Result<String, GenerationConfigError> {
    let parsed: Value = serde_json::from_str(source)
        .map_err(|err| GenerationConfigError::Parse(err.to_string()))?;

    let object = parsed.as_object().ok_or_else(|| {
        GenerationConfigError::Parse(String::from(
            "generation configuration must be a JSON object",
        ))
    })?;

    let criteria = build_stopping_criteria(object);
    serde_json::to_string(&criteria)
        .map_err(|err| GenerationConfigError::Serialize(err.to_string()))
}

pub(crate) fn build_logits_bindings(object: &Map<String, Value>) -> Vec<LogitsBinding> {
    let mut bindings: Vec<LogitsBinding> = Vec::new();

    if let Some(temperature) = extract_numeric(object, "temperature") {
        if temperature.is_finite() && temperature > 0.0 && (temperature - 1.0).abs() > EPSILON {
            bindings.push(LogitsBinding {
                category: "warper",
                kind: "temperature",
                value: temperature,
            });
        }
    }

    if let Some(top_p) = extract_numeric(object, "top_p") {
        if top_p.is_finite() && top_p > 0.0 && top_p + EPSILON < 1.0 {
            bindings.push(LogitsBinding {
                category: "warper",
                kind: "top_p",
                value: top_p,
            });
        }
    }

    if let Some(repetition_penalty) = extract_numeric(object, "repetition_penalty") {
        if repetition_penalty.is_finite() && repetition_penalty > 1.0 + EPSILON {
            bindings.push(LogitsBinding {
                category: "processor",
                kind: "repetition_penalty",
                value: repetition_penalty,
            });
        }
    }

    bindings
}

pub(crate) fn build_stopping_criteria(object: &Map<String, Value>) -> Vec<StoppingCriterion> {
    let mut criteria: Vec<StoppingCriterion> = Vec::new();

    if let Some(max_tokens) = extract_integer(object, "max_new_tokens") {
        if max_tokens > 0 {
            criteria.push(StoppingCriterion {
                kind: "max_new_tokens",
                value: Some(max_tokens),
                sequences: None,
            });
        }
    }

    if let Some(stop_sequences) = collect_stop_sequences(object) {
        if !stop_sequences.is_empty() {
            criteria.push(StoppingCriterion {
                kind: "stop_sequences",
                value: None,
                sequences: Some(stop_sequences),
            });
        }
    }

    criteria
}

#[cfg_attr(not(test), doc(hidden))]
pub mod test_support {
    use serde_json::{Map, Value};

    use super::helpers;

    pub fn extract_numeric(map: &Map<String, Value>, key: &str) -> Option<f64> {
        helpers::extract_numeric(map, key)
    }

    pub fn extract_integer(map: &Map<String, Value>, key: &str) -> Option<u64> {
        helpers::extract_integer(map, key)
    }

    pub fn collect_stop_sequences(map: &Map<String, Value>) -> Option<Vec<String>> {
        helpers::collect_stop_sequences(map)
    }

    pub fn convert_value_to_string(value: &Value) -> Option<String> {
        helpers::convert_value_to_string(value)
    }

    pub fn get_property<'a>(map: &'a Map<String, Value>, key: &str) -> Option<&'a Value> {
        helpers::get_property(map, key)
    }

    pub fn to_f64(value: &Value) -> Option<f64> {
        helpers::to_f64(value)
    }

    pub fn to_u64(value: &Value) -> Option<u64> {
        helpers::to_u64(value)
    }

    pub fn value_to_string_list(value: &Value) -> Vec<String> {
        helpers::value_to_string_list(value)
    }

    pub fn build_logits_bindings(object: &Map<String, Value>) -> Vec<Value> {
        super::build_logits_bindings(object)
            .into_iter()
            .map(|binding| serde_json::to_value(binding).expect("binding must serialize"))
            .collect()
    }

    pub fn build_stopping_criteria(object: &Map<String, Value>) -> Vec<Value> {
        super::build_stopping_criteria(object)
            .into_iter()
            .map(|criterion| serde_json::to_value(criterion).expect("criterion must serialize"))
            .collect()
    }
}

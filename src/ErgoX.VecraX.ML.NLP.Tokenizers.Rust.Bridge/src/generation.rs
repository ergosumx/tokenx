use serde_json::Value;

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
    serde_json::to_string(&parsed)
        .map_err(|err| GenerationConfigError::Serialize(err.to_string()))
}

#[cfg(test)]
mod tests {
    use super::normalize_generation_config;

    #[test]
    fn normalize_preserves_semantics() {
        let input = r#"{"temperature":0.7,"nested":{"top_p":"0.9"}}"#;
        let normalized = normalize_generation_config(input).expect("normalization should succeed");
        assert_eq!(normalized, "{\"temperature\":0.7,\"nested\":{\"top_p\":\"0.9\"}}");
    }
}

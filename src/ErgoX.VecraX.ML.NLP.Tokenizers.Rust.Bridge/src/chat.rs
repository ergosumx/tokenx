use std::borrow::Cow;

use minijinja::value::Value as JinjaValue;
use minijinja::{Environment, Error as JinjaError, ErrorKind};
use serde_json::{Map as JsonMap, Value as JsonValue};

#[derive(Debug)]
pub enum ChatTemplateError {
    InvalidMessages(String),
    InvalidVariables(String),
    Template(String),
}

fn raise_exception(message: JinjaValue) -> Result<JinjaValue, JinjaError> {
    let rendered = message.to_string();
    Err(JinjaError::new(ErrorKind::UndefinedError, rendered))
}

fn normalize_template_source(source: &str) -> Cow<'_, str> {
    let mut owned: Option<String> = None;

    let mut apply = |pattern: &str, replacement: &str| {
        let current = owned.as_deref().unwrap_or(source);
        if current.contains(pattern) {
            owned = Some(current.replace(pattern, replacement));
        }
    };

    apply(".strip()", " | trim");
    apply(".lstrip()", " | lstrip");
    apply(".rstrip()", " | rstrip");
    apply(".replace(", " | replace(");
    apply(".title()", " | title");

    match owned {
        Some(value) => Cow::Owned(value),
        None => Cow::Borrowed(source),
    }
}

pub fn render_chat_template(
    template_source: &str,
    messages_json: &str,
    variables_json: Option<&str>,
    add_generation_prompt: bool,
) -> Result<String, ChatTemplateError> {
    let messages_value: JsonValue = serde_json::from_str(messages_json).map_err(|err| {
        ChatTemplateError::InvalidMessages(format!("failed to parse messages payload: {err}"))
    })?;

    if !messages_value.is_array() {
        return Err(ChatTemplateError::InvalidMessages(
            "messages payload must be a JSON array".to_string(),
        ));
    }

    let mut globals = JsonMap::new();
    globals.insert("messages".to_string(), messages_value);
    globals.insert(
        "add_generation_prompt".to_string(),
        JsonValue::Bool(add_generation_prompt),
    );

    if let Some(raw) = variables_json {
        if !raw.trim().is_empty() {
            let parsed: JsonValue = serde_json::from_str(raw).map_err(|err| {
                ChatTemplateError::InvalidVariables(format!(
                    "failed to parse chat template variables payload: {err}"
                ))
            })?;

            if let JsonValue::Object(map) = parsed {
                for (key, value) in map {
                    globals.insert(key, value);
                }
            } else {
                return Err(ChatTemplateError::InvalidVariables(
                    "chat template variables payload must be a JSON object".to_string(),
                ));
            }
        }
    }

    let root_value = JsonValue::Object(globals);
    let mut environment = Environment::new();
    environment.set_trim_blocks(true);
    environment.set_lstrip_blocks(true);
    environment.add_function("raise_exception", raise_exception);

    let normalized_template = normalize_template_source(template_source);

    environment
        .render_str(normalized_template.as_ref(), &root_value)
        .map_err(|err| ChatTemplateError::Template(format!(
            "failed to render chat template: {err}"
        )))
}

use serde::Serialize;

#[derive(Serialize)]
pub(crate) struct LogitsBinding {
    #[serde(rename = "category")]
    pub(crate) category: &'static str,
    #[serde(rename = "kind")]
    pub(crate) kind: &'static str,
    #[serde(rename = "value")]
    pub(crate) value: f64,
}

#[derive(Serialize)]
pub(crate) struct StoppingCriterion {
    #[serde(rename = "kind")]
    pub(crate) kind: &'static str,
    #[serde(skip_serializing_if = "Option::is_none", rename = "value")]
    pub(crate) value: Option<u64>,
    #[serde(skip_serializing_if = "Option::is_none", rename = "sequences")]
    pub(crate) sequences: Option<Vec<String>>,
}

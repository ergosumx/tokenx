use tokenx_bridge::{render_chat_template, ChatTemplateError};

fn sample_messages() -> &'static str {
    r#"[
        {"role": "system", "content": "Be helpful."},
        {"role": "user", "content": "Hello there"}
    ]"#
}

#[test]
fn render_chat_template_renders_messages_and_prompt() {
    let template = "{% for message in messages %}{{ message.role }}: {{ message.content.strip() }}\n{% endfor %}{% if add_generation_prompt %}assistant: {% endif %}";
    let rendered = render_chat_template(template, sample_messages(), None, true)
        .expect("template should render");

    assert!(rendered.contains("system: Be helpful."));
    assert!(rendered.contains("assistant:"));
}

#[test]
fn render_chat_template_errors_on_non_array_messages() {
    let template = "{{ messages | length }}";
    let error = render_chat_template(template, "{}", None, false)
        .expect_err("invalid messages payload should fail");

    match error {
        ChatTemplateError::InvalidMessages(message) => {
            assert!(message.contains("must be a JSON array"));
        }
        other => panic!("unexpected error: {other:?}"),
    }
}

#[test]
fn render_chat_template_errors_on_non_object_variables() {
    let template = "{{ custom }}";
    let error = render_chat_template(template, sample_messages(), Some("[]"), false)
        .expect_err("invalid variables payload should fail");

    match error {
        ChatTemplateError::InvalidVariables(message) => {
            assert!(message.contains("must be a JSON object"));
        }
        other => panic!("unexpected error: {other:?}"),
    }
}

#[test]
fn render_chat_template_reports_template_error() {
    let template = "{{ raise_exception('boom') }}";
    let error = render_chat_template(template, sample_messages(), None, false)
        .expect_err("template failure should be reported");

    match error {
        ChatTemplateError::Template(message) => {
            assert!(message.contains("boom"));
        }
        other => panic!("unexpected error: {other:?}"),
    }
}

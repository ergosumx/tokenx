use std::ffi::CString;
use std::ptr;
use tokenx_bridge::ffi::chat::tokenizers_apply_chat_template;
use tokenx_bridge::ffi::lifecycle::tokenizers_free_string;
use tokenx_bridge::ffi::test_helpers;
use tokenx_bridge::CTokenizer;

fn messages_json() -> CString {
    CString::new(r#"[{"role":"user","content":"Hello"}]"#).unwrap()
}

#[test]
fn tokenizers_apply_chat_template_renders_output() {
    let tokenizer = test_helpers::create_tokenizer();
    let template = CString::new("{{ messages[0].content }}").unwrap();
    let messages = messages_json();
    let mut status = -1;
    let rendered = tokenizers_apply_chat_template(
        &tokenizer as *const CTokenizer,
        template.as_ptr(),
        messages.as_ptr(),
        ptr::null(),
        false,
        ptr::addr_of_mut!(status),
    );

    assert_eq!(status, 0);
    assert!(!rendered.is_null());

    unsafe {
        tokenizers_free_string(rendered);
    }
}

#[test]
fn tokenizers_apply_chat_template_handles_invalid_messages() {
    let tokenizer = test_helpers::create_tokenizer();
    let template = CString::new("{{ messages }}").unwrap();
    let invalid_messages = CString::new("{}").unwrap();
    let mut status = -1;
    let rendered = tokenizers_apply_chat_template(
        &tokenizer as *const CTokenizer,
        template.as_ptr(),
        invalid_messages.as_ptr(),
        ptr::null(),
        false,
        ptr::addr_of_mut!(status),
    );

    assert!(rendered.is_null());
    assert_ne!(status, 0);
}

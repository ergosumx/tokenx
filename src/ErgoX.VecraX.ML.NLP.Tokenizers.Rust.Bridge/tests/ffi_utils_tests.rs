use std::ffi::CString;
use std::ptr;
use tokenx_bridge::ffi::utils_test_support::{
    copy_slice, read_optional_utf8, read_required_utf8, set_length, set_status,
};

#[test]
fn set_status_updates_value_when_pointer_is_valid() {
    let mut status = 0;
    set_status(ptr::addr_of_mut!(status), 7);
    assert_eq!(status, 7);
}

#[test]
fn set_status_ignores_null_pointer() {
    set_status(ptr::null_mut(), 42);
}

#[test]
fn set_length_updates_length_when_pointer_is_valid() {
    let mut length = 0usize;
    set_length(ptr::addr_of_mut!(length), 3);
    assert_eq!(length, 3);
}

#[test]
fn read_required_utf8_returns_error_on_null() {
    assert!(read_required_utf8(ptr::null()).is_err());
}

#[test]
fn read_optional_utf8_reads_valid_string() {
    let value = CString::new("hello").unwrap();
    let read = read_optional_utf8(value.as_ptr()).expect("read should succeed");
    assert_eq!(read.as_deref(), Some("hello"));
}

#[test]
fn copy_slice_transfers_elements() {
    let source = [1u32, 2, 3];
    let mut destination = [0u32; 3];
    copy_slice(&source, destination.as_mut_ptr(), destination.len());
    assert_eq!(destination, source);
}

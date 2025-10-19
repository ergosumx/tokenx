use std::ffi::CStr;
use std::os::raw::{c_char, c_int};

pub(crate) fn set_status(status: *mut c_int, value: c_int) {
    if !status.is_null() {
        unsafe {
            *status = value;
        }
    }
}

pub(crate) fn set_length(target: *mut usize, value: usize) {
    if !target.is_null() {
        unsafe {
            *target = value;
        }
    }
}

pub(crate) fn read_required_utf8(ptr_value: *const c_char) -> Result<String, &'static str> {
    if ptr_value.is_null() {
        return Err("received null pointer");
    }

    let source = unsafe { CStr::from_ptr(ptr_value) };
    source
        .to_str()
        .map(|s| s.to_owned())
        .map_err(|_| "invalid UTF-8 data")
}

pub(crate) fn read_optional_utf8(ptr_value: *const c_char) -> Result<Option<String>, &'static str> {
    if ptr_value.is_null() {
        Ok(None)
    } else {
        read_required_utf8(ptr_value).map(Some)
    }
}

pub(crate) fn copy_slice<T: Copy>(source: &[T], destination: *mut T, length: usize) {
    if destination.is_null() || length == 0 {
        return;
    }

    let copy_len = length.min(source.len());
    unsafe {
        std::ptr::copy_nonoverlapping(source.as_ptr(), destination, copy_len);
    }
}

#[cfg_attr(not(test), doc(hidden))]
pub mod test_support {
    use std::os::raw::{c_char, c_int};

    use super::set_status as inner_set_status;
    use super::{copy_slice as inner_copy_slice, read_optional_utf8 as inner_read_optional_utf8};
    use super::{read_required_utf8 as inner_read_required_utf8, set_length as inner_set_length};

    pub fn set_status(status: *mut c_int, value: c_int) {
        inner_set_status(status, value);
    }

    pub fn set_length(target: *mut usize, value: usize) {
        inner_set_length(target, value);
    }

    pub fn read_required_utf8(ptr_value: *const c_char) -> Result<String, &'static str> {
        inner_read_required_utf8(ptr_value)
    }

    pub fn read_optional_utf8(ptr_value: *const c_char) -> Result<Option<String>, &'static str> {
        inner_read_optional_utf8(ptr_value)
    }

    pub fn copy_slice<T: Copy>(source: &[T], destination: *mut T, length: usize) {
        inner_copy_slice(source, destination, length);
    }
}

use std::cell::RefCell;
use std::ffi::CString;
use std::os::raw::c_char;
use std::ptr;

thread_local! {
    pub(crate) static LAST_ERROR: RefCell<Option<CString>> = const { RefCell::new(None) };
}

pub(crate) fn store_error(message: &str) {
    LAST_ERROR.with(|cell| {
        let fallback = "tokenx bridge error";
        let cstring = CString::new(message)
            .unwrap_or_else(|_| CString::new(fallback).expect("fallback string is valid"));
        *cell.borrow_mut() = Some(cstring);
    });
}

pub(crate) fn clear_error() {
    LAST_ERROR.with(|cell| {
        *cell.borrow_mut() = None;
    });
}

#[no_mangle]
pub extern "C" fn tokenizers_get_last_error() -> *const c_char {
    LAST_ERROR.with(|cell| match &*cell.borrow() {
        Some(err) => err.as_ptr(),
        None => ptr::null(),
    })
}

#[cfg_attr(not(test), doc(hidden))]
pub mod test_support {
    pub fn store_error(message: &str) {
        super::store_error(message);
    }

    pub fn clear_error() {
        super::clear_error();
    }
}

use std::ffi::CString;
use std::os::raw::{c_char, c_int};
use std::ptr;

use serde_json::from_str;
use tokenizers::models::ModelWrapper;

use crate::error::{clear_error, store_error};
use crate::ffi::utils::{read_required_utf8, set_status};
use crate::tokenizer::CTokenizer;

#[repr(C)]
pub struct CModel {
    inner: ModelWrapper,
}

impl CModel {
    fn new(inner: ModelWrapper) -> Self {
        Self { inner }
    }

    fn clone_inner(&self) -> ModelWrapper {
        self.inner.clone()
    }

    fn model_type(&self) -> &'static str {
        match &self.inner {
            ModelWrapper::BPE(_) => "BPE",
            ModelWrapper::WordPiece(_) => "WordPiece",
            ModelWrapper::WordLevel(_) => "WordLevel",
            ModelWrapper::Unigram(_) => "Unigram",
        }
    }

    fn serialize(&self, pretty: bool) -> Result<String, serde_json::Error> {
        if pretty {
            serde_json::to_string_pretty(&self.inner)
        } else {
            serde_json::to_string(&self.inner)
        }
    }
}

/// # Safety
/// `json` must point to a null-terminated UTF-8 string and `status` must be a valid pointer.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_model_from_json(
    json: *const c_char,
    status: *mut c_int,
) -> *mut CModel {
    match read_required_utf8(json) {
        Ok(payload) => match from_str::<ModelWrapper>(&payload) {
            Ok(model) => {
                clear_error();
                set_status(status, 0);
                Box::into_raw(Box::new(CModel::new(model)))
            }
            Err(err) => {
                store_error(&format!("tokenizers_model_from_json failed: {err}"));
                set_status(status, 2);
                ptr::null_mut()
            }
        },
        Err(message) => {
            store_error(message);
            set_status(status, 1);
            ptr::null_mut()
        }
    }
}

/// # Safety
/// `model` must be null or a pointer previously returned by this library.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_model_free(model: *mut CModel) {
    if model.is_null() {
        return;
    }

    unsafe {
        drop(Box::from_raw(model));
    }
}

/// # Safety
/// `model` must be valid and `status` must be writable.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_model_get_type(
    model: *const CModel,
    status: *mut c_int,
) -> *mut c_char {
    if model.is_null() {
        store_error("tokenizers_model_get_type received null model");
        set_status(status, 1);
        return ptr::null_mut();
    }

    let model = &*model;
    match CString::new(model.model_type()) {
        Ok(value) => {
            clear_error();
            set_status(status, 0);
            value.into_raw()
        }
        Err(_) => {
            store_error("tokenizers_model_get_type failed to allocate CString");
            set_status(status, 2);
            ptr::null_mut()
        }
    }
}

/// # Safety
/// `model` must be valid and `status` writable; returns an owned string that must be freed with `tokenizers_free_string`.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_model_to_json(
    model: *const CModel,
    pretty: bool,
    status: *mut c_int,
) -> *mut c_char {
    if model.is_null() {
        store_error("tokenizers_model_to_json received null model");
        set_status(status, 1);
        return ptr::null_mut();
    }

    let model = &*model;
    match model.serialize(pretty) {
        Ok(json) => match CString::new(json) {
            Ok(value) => {
                clear_error();
                set_status(status, 0);
                value.into_raw()
            }
            Err(_) => {
                store_error("tokenizers_model_to_json failed to allocate CString");
                set_status(status, 2);
                ptr::null_mut()
            }
        },
        Err(err) => {
            store_error(&format!("tokenizers_model_to_json failed: {err}"));
            set_status(status, 3);
            ptr::null_mut()
        }
    }
}

/// # Safety
/// All pointers must be valid and writable where appropriate.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_tokenizer_set_model(
    tokenizer: *mut CTokenizer,
    model: *const CModel,
    status: *mut c_int,
) -> c_int {
    if tokenizer.is_null() {
        store_error("tokenizers_tokenizer_set_model received null tokenizer");
        set_status(status, 1);
        return 0;
    }

    if model.is_null() {
        store_error("tokenizers_tokenizer_set_model received null model");
        set_status(status, 2);
        return 0;
    }

    let tokenizer = &mut *tokenizer;
    let model = &*model;

    tokenizer.inner_mut().with_model(model.clone_inner());
    clear_error();
    set_status(status, 0);
    1
}

#[cfg_attr(not(test), doc(hidden))]
pub mod test_support {
    use super::CModel;

    pub fn inner_model(model: &CModel) -> &tokenizers::models::ModelWrapper {
        &model.inner
    }
}

use std::ffi::CString;
use std::os::raw::{c_char, c_int};
use std::ptr;

use serde_json::from_str;
use tokenizers::decoders::DecoderWrapper;

use crate::error::{clear_error, store_error};
use crate::ffi::utils::{read_required_utf8, set_status};
use crate::tokenizer::CTokenizer;

#[repr(C)]
pub struct CDecoder {
    inner: DecoderWrapper,
}

impl CDecoder {
    fn new(inner: DecoderWrapper) -> Self {
        Self { inner }
    }

    fn clone_inner(&self) -> DecoderWrapper {
        self.inner.clone()
    }

    fn decoder_type(&self) -> &'static str {
        match &self.inner {
            DecoderWrapper::BPE(_) => "BPE",
            DecoderWrapper::ByteLevel(_) => "ByteLevel",
            DecoderWrapper::WordPiece(_) => "WordPiece",
            DecoderWrapper::Metaspace(_) => "Metaspace",
            DecoderWrapper::CTC(_) => "CTC",
            DecoderWrapper::Sequence(_) => "Sequence",
            DecoderWrapper::Replace(_) => "Replace",
            DecoderWrapper::Strip(_) => "Strip",
            DecoderWrapper::ByteFallback(_) => "ByteFallback",
            DecoderWrapper::Fuse(_) => "Fuse",
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
/// `json` must reference a null-terminated UTF-8 string; `status` must be writable.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_decoder_from_json(
    json: *const c_char,
    status: *mut c_int,
) -> *mut CDecoder {
    match read_required_utf8(json) {
        Ok(payload) => match from_str::<DecoderWrapper>(&payload) {
            Ok(decoder) => {
                clear_error();
                set_status(status, 0);
                Box::into_raw(Box::new(CDecoder::new(decoder)))
            }
            Err(err) => {
                store_error(&format!("tokenizers_decoder_from_json failed: {err}"));
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
/// `decoder` must be null or a pointer previously obtained from this library.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_decoder_free(decoder: *mut CDecoder) {
    if decoder.is_null() {
        return;
    }

    drop(Box::from_raw(decoder));
}

/// # Safety
/// `decoder` must be valid; returns an owned string that must be freed with `tokenizers_free_string`.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_decoder_get_type(
    decoder: *const CDecoder,
    status: *mut c_int,
) -> *mut c_char {
    if decoder.is_null() {
        store_error("tokenizers_decoder_get_type received null decoder");
        set_status(status, 1);
        return ptr::null_mut();
    }

    let decoder = &*decoder;
    match CString::new(decoder.decoder_type()) {
        Ok(value) => {
            clear_error();
            set_status(status, 0);
            value.into_raw()
        }
        Err(_) => {
            store_error("tokenizers_decoder_get_type failed to allocate CString");
            set_status(status, 2);
            ptr::null_mut()
        }
    }
}

/// # Safety
/// `decoder` must be valid; result must be released via `tokenizers_free_string`.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_decoder_to_json(
    decoder: *const CDecoder,
    pretty: bool,
    status: *mut c_int,
) -> *mut c_char {
    if decoder.is_null() {
        store_error("tokenizers_decoder_to_json received null decoder");
        set_status(status, 1);
        return ptr::null_mut();
    }

    let decoder = &*decoder;
    match decoder.serialize(pretty) {
        Ok(json) => match CString::new(json) {
            Ok(value) => {
                clear_error();
                set_status(status, 0);
                value.into_raw()
            }
            Err(_) => {
                store_error("tokenizers_decoder_to_json failed to allocate CString");
                set_status(status, 2);
                ptr::null_mut()
            }
        },
        Err(err) => {
            store_error(&format!("tokenizers_decoder_to_json failed: {err}"));
            set_status(status, 3);
            ptr::null_mut()
        }
    }
}

/// # Safety
/// All pointers must be valid and writable.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_tokenizer_set_decoder(
    tokenizer: *mut CTokenizer,
    decoder: *const CDecoder,
    status: *mut c_int,
) -> c_int {
    if tokenizer.is_null() {
        store_error("tokenizers_tokenizer_set_decoder received null tokenizer");
        set_status(status, 1);
        return 0;
    }

    if decoder.is_null() {
        store_error("tokenizers_tokenizer_set_decoder received null decoder");
        set_status(status, 2);
        return 0;
    }

    let tokenizer = &mut *tokenizer;
    let decoder = &*decoder;

    tokenizer
        .inner_mut()
        .with_decoder(Some(decoder.clone_inner()));

    clear_error();
    set_status(status, 0);
    1
}

/// # Safety
/// `tokenizer` must be valid and `status` writable.
#[no_mangle]
pub unsafe extern "C" fn tokenizers_tokenizer_clear_decoder(
    tokenizer: *mut CTokenizer,
    status: *mut c_int,
) -> c_int {
    if tokenizer.is_null() {
        store_error("tokenizers_tokenizer_clear_decoder received null tokenizer");
        set_status(status, 1);
        return 0;
    }

    let tokenizer = &mut *tokenizer;
    tokenizer.inner_mut().with_decoder(None::<DecoderWrapper>);

    clear_error();
    set_status(status, 0);
    1
}

#[cfg_attr(not(test), doc(hidden))]
pub mod test_support {
    use super::CDecoder;

    pub fn inner_decoder(decoder: &CDecoder) -> &tokenizers::decoders::DecoderWrapper {
        &decoder.inner
    }
}

pub(crate) mod chat;
pub(crate) mod encoding;
pub(crate) mod error;
pub mod ffi;
pub mod generation;
pub(crate) mod tokenizer;

pub use encoding::{CEncoding, CEncodingNumericDest, CEncodingOffset};
pub use error::tokenizers_get_last_error;
pub use tokenizer::CTokenizer;

#[doc(hidden)]
pub use chat::{render_chat_template, ChatTemplateError};

#[doc(hidden)]
pub use encoding::test_support as encoding_test_support;

#[doc(hidden)]
pub use error::test_support as error_test_support;

#[doc(hidden)]
pub use ffi::models::test_support as models_test_support;

#[doc(hidden)]
pub use ffi::decoders::test_support as decoders_test_support;

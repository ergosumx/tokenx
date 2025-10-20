pub(crate) mod utils;

pub mod chat;
pub mod config;
pub mod decode;
pub mod decoders;
pub mod encoding;
pub mod generation;
pub mod lifecycle;
pub mod models;
pub mod padding;
pub mod truncation;

#[cfg_attr(not(test), doc(hidden))]
pub mod test_helpers {
    use ahash::AHashMap;
    use std::ffi::CString;

    use tokenizers::models::wordlevel::WordLevel;
    use tokenizers::pre_tokenizers::whitespace::Whitespace;
    use tokenizers::Tokenizer;

    use crate::tokenizer::CTokenizer;

    pub fn create_tokenizer() -> CTokenizer {
        let mut vocab = AHashMap::new();
        vocab.insert("[UNK]".to_string(), 0);
        vocab.insert("hello".to_string(), 1);
        vocab.insert("world".to_string(), 2);

        let model = WordLevel::builder()
            .vocab(vocab)
            .unk_token("[UNK]".into())
            .build()
            .expect("wordlevel vocab should be valid");

        let mut tokenizer = Tokenizer::new(model);
        tokenizer.with_pre_tokenizer(Some(Whitespace));
        CTokenizer::new(tokenizer)
    }

    pub fn tokenizer_config_json() -> CString {
        let tokenizer = create_tokenizer();
        let json = tokenizer
            .inner()
            .to_string(true)
            .expect("tokenizer should serialize");
        CString::new(json).expect("json must not contain null bytes")
    }
}

#[doc(hidden)]
pub use utils::test_support as utils_test_support;

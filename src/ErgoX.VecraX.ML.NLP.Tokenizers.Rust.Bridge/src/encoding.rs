use tokenizers::Encoding;

const NUMERIC_LENGTH_ERROR: &str =
    "tokenizers_encoding_copy_numeric received insufficient destination length";
const DESTINATION_NULL_ERROR: &str =
    "tokenizers_encoding_copy_numeric received null destination buffer";
const WORD_OVERFLOW_ERROR: &str = "tokenizers_encoding_copy_numeric encountered word id overflow";
const SEQUENCE_OVERFLOW_ERROR: &str =
    "tokenizers_encoding_copy_numeric encountered sequence id overflow";

#[derive(Clone)]
pub struct CEncoding {
    pub(crate) ids: Vec<u32>,
    pub(crate) tokens: Vec<String>,
    pub(crate) offsets: Vec<(u32, u32)>,
    pub(crate) type_ids: Vec<u32>,
    pub(crate) attention_mask: Vec<u32>,
    pub(crate) special_tokens_mask: Vec<u32>,
    pub(crate) word_ids: Vec<Option<u32>>,
    pub(crate) sequence_ids: Vec<Option<usize>>,
    pub(crate) overflowing: Vec<CEncoding>,
}

#[repr(C)]
#[derive(Clone, Copy, Default)]
pub struct CEncodingOffset {
    pub start: u32,
    pub end: u32,
}

#[repr(C)]
pub struct CEncodingNumericDest {
    pub ids: *mut u32,
    pub type_ids: *mut u32,
    pub attention_mask: *mut u32,
    pub special_tokens_mask: *mut u32,
    pub offsets: *mut CEncodingOffset,
    pub word_ids: *mut i32,
    pub sequence_ids: *mut i32,
}

impl CEncoding {
    pub(crate) fn from_encoding(encoding: Encoding) -> Self {
        let ids = encoding.get_ids().to_vec();
        let tokens = encoding.get_tokens().to_vec();
        let offsets = encoding
            .get_offsets()
            .iter()
            .map(|(start, end)| (*start as u32, *end as u32))
            .collect();
        let type_ids = encoding.get_type_ids().to_vec();
        let attention_mask = encoding.get_attention_mask().to_vec();
        let special_tokens_mask = encoding.get_special_tokens_mask().to_vec();
        let word_ids = encoding.get_word_ids().to_vec();
        let sequence_ids = encoding.get_sequence_ids();
        let overflowing = encoding
            .get_overflowing()
            .iter()
            .cloned()
            .map(Self::from_encoding)
            .collect();

        Self {
            ids,
            tokens,
            offsets,
            type_ids,
            attention_mask,
            special_tokens_mask,
            word_ids,
            sequence_ids,
            overflowing,
        }
    }

    pub(crate) fn len(&self) -> usize {
        self.ids.len()
    }

    pub(crate) fn numeric_len_and_validate(
        &self,
        destination_length: usize,
    ) -> Result<usize, &'static str> {
        let required = self.len();
        if required > destination_length {
            Err(NUMERIC_LENGTH_ERROR)
        } else {
            Ok(required)
        }
    }

    pub(crate) fn fill_offsets(&self, destination: *mut CEncodingOffset) {
        if destination.is_null() {
            return;
        }

        for (index, (start, end)) in self.offsets.iter().enumerate() {
            unsafe {
                destination.add(index).write(CEncodingOffset {
                    start: *start,
                    end: *end,
                });
            }
        }
    }

    pub(crate) fn fill_word_ids(&self, destination: *mut i32) -> Result<(), &'static str> {
        if destination.is_null() {
            return Err(DESTINATION_NULL_ERROR);
        }

        for (index, value) in self.word_ids.iter().enumerate() {
            let mapped = match value {
                Some(id) => {
                    if *id > i32::MAX as u32 {
                        return Err(WORD_OVERFLOW_ERROR);
                    }
                    *id as i32
                }
                None => -1,
            };

            unsafe {
                destination.add(index).write(mapped);
            }
        }

        Ok(())
    }

    pub(crate) fn fill_sequence_ids(&self, destination: *mut i32) -> Result<(), &'static str> {
        if destination.is_null() {
            return Err(DESTINATION_NULL_ERROR);
        }

        for (index, value) in self.sequence_ids.iter().enumerate() {
            let mapped = match value {
                Some(id) => {
                    if *id > i32::MAX as usize {
                        return Err(SEQUENCE_OVERFLOW_ERROR);
                    }
                    *id as i32
                }
                None => -1,
            };

            unsafe {
                destination.add(index).write(mapped);
            }
        }

        Ok(())
    }
}

impl CEncodingNumericDest {
    pub(crate) fn validate(&self) -> Result<(), &'static str> {
        if self.ids.is_null()
            || self.type_ids.is_null()
            || self.attention_mask.is_null()
            || self.special_tokens_mask.is_null()
            || self.offsets.is_null()
            || self.word_ids.is_null()
            || self.sequence_ids.is_null()
        {
            Err(DESTINATION_NULL_ERROR)
        } else {
            Ok(())
        }
    }
}

#[cfg_attr(not(test), doc(hidden))]
pub mod test_support {
    use super::*;
    use ahash::AHashMap;
    use tokenizers::models::wordlevel::WordLevel;
    use tokenizers::pre_tokenizers::whitespace::Whitespace;
    use tokenizers::Tokenizer;

    pub fn build_encoding_from_tokenizer() -> CEncoding {
        let mut vocab = AHashMap::new();
        vocab.insert("[UNK]".to_string(), 0);
        vocab.insert("Hello".to_string(), 1);
        vocab.insert("world".to_string(), 2);

        let model = WordLevel::builder()
            .vocab(vocab)
            .unk_token("[UNK]".into())
            .build()
            .expect("wordlevel vocab should be valid");

    let mut tokenizer = Tokenizer::new(model);
    tokenizer.with_pre_tokenizer(Some(Whitespace));

        let encoding = tokenizer
            .encode("Hello world", true)
            .expect("encoding should succeed");

        CEncoding::from_encoding(encoding)
    }

    pub fn from_encoding(encoding: tokenizers::Encoding) -> CEncoding {
        CEncoding::from_encoding(encoding)
    }

    pub fn len(encoding: &CEncoding) -> usize {
        encoding.len()
    }

    pub fn ids(encoding: &CEncoding) -> &[u32] {
        &encoding.ids
    }

    pub fn tokens(encoding: &CEncoding) -> &[String] {
        &encoding.tokens
    }

    pub fn numeric_len_and_validate(
        encoding: &CEncoding,
        destination_length: usize,
    ) -> Result<usize, &'static str> {
        encoding.numeric_len_and_validate(destination_length)
    }

    pub fn fill_offsets(encoding: &CEncoding, destination: *mut CEncodingOffset) {
        encoding.fill_offsets(destination);
    }

    pub fn fill_word_ids(encoding: &CEncoding, destination: *mut i32) -> Result<(), &'static str> {
        encoding.fill_word_ids(destination)
    }

    pub fn fill_sequence_ids(
        encoding: &CEncoding,
        destination: *mut i32,
    ) -> Result<(), &'static str> {
        encoding.fill_sequence_ids(destination)
    }

    pub fn validate_destination(destination: &CEncodingNumericDest) -> Result<(), &'static str> {
        destination.validate()
    }

    pub fn word_overflow_error() -> &'static str {
        super::WORD_OVERFLOW_ERROR
    }

    pub fn sequence_overflow_error() -> &'static str {
        super::SEQUENCE_OVERFLOW_ERROR
    }

    pub fn destination_null_error() -> &'static str {
        super::DESTINATION_NULL_ERROR
    }

    pub fn numeric_length_error() -> &'static str {
        super::NUMERIC_LENGTH_ERROR
    }

    pub fn set_word_ids(encoding: &mut CEncoding, values: Vec<Option<u32>>) {
        encoding.word_ids = values;
    }

    pub fn set_sequence_ids(encoding: &mut CEncoding, values: Vec<Option<usize>>) {
        encoding.sequence_ids = values;
    }
}

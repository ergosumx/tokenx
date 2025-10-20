use tokenizers::Tokenizer;

pub struct CTokenizer {
    inner: Tokenizer,
}

impl CTokenizer {
    pub(crate) fn new(tokenizer: Tokenizer) -> Self {
        Self { inner: tokenizer }
    }

    pub(crate) fn inner(&self) -> &Tokenizer {
        &self.inner
    }

    pub(crate) fn inner_mut(&mut self) -> &mut Tokenizer {
        &mut self.inner
    }
}

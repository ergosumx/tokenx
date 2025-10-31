# t5-small

This folder is intended to hold the model files for `t5-small` (text-to-text Transformer).

How to download

1) Using git (requires git-lfs):

```powershell
git lfs install
git clone https://huggingface.co/t5-small examples/.models/t5-small
```

2) Using Transformers (Python):

```python
from transformers import T5ForConditionalGeneration, T5Tokenizer
tokenizer = T5Tokenizer.from_pretrained('t5-small')
model = T5ForConditionalGeneration.from_pretrained('t5-small')

# To load from a local folder after cloning:
# tokenizer = T5Tokenizer.from_pretrained('examples/.models/t5-small')
# model = T5ForConditionalGeneration.from_pretrained('examples/.models/t5-small')
```

Notes
- If the model is private, authenticate with `huggingface-cli login` first.
- Model page: https://huggingface.co/t5-small

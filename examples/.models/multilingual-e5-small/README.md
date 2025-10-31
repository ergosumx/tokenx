# intfloat/multilingual-e5-small

This folder is intended to hold the model files for `intfloat/multilingual-e5-small` (multilingual embeddings).

How to download

1) Using git (requires git-lfs):

```powershell
git lfs install
git clone https://huggingface.co/intfloat/multilingual-e5-small examples/.models/multilingual-e5-small
```

2) Using Transformers / Hugging Face Hub (Python):

```python
from transformers import AutoTokenizer, AutoModel
tokenizer = AutoTokenizer.from_pretrained('intfloat/multilingual-e5-small')
model = AutoModel.from_pretrained('intfloat/multilingual-e5-small')
# Or, after cloning locally:
# tokenizer = AutoTokenizer.from_pretrained('examples/.models/multilingual-e5-small')
# model = AutoModel.from_pretrained('examples/.models/multilingual-e5-small')
```

Notes
- Authenticate with `huggingface-cli login` if the model requires it.
- See the model page for license details: https://huggingface.co/intfloat/multilingual-e5-small

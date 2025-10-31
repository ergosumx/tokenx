# intfloat/e5-small-v2

This folder is intended to hold the model files for `intfloat/e5-small-v2` (embedding model).

How to download

1) Using git (requires git-lfs):

```powershell
git lfs install
git clone https://huggingface.co/intfloat/e5-small-v2 examples/.models/e5-small-v2
```

2) Using Transformers / Hugging Face Hub (Python):

```python
from transformers import AutoTokenizer, AutoModel
tokenizer = AutoTokenizer.from_pretrained('intfloat/e5-small-v2')
model = AutoModel.from_pretrained('intfloat/e5-small-v2')
# Or, after cloning locally:
# tokenizer = AutoTokenizer.from_pretrained('examples/.models/e5-small-v2')
# model = AutoModel.from_pretrained('examples/.models/e5-small-v2')
```

Notes
- If the model is private, authenticate with `huggingface-cli login` before cloning or using `from_pretrained`.
- Check the model page for license and usage terms: https://huggingface.co/intfloat/e5-small-v2

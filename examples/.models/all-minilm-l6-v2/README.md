# sentence-transformers/all-MiniLM-L6-v2

This folder is intended to hold the model files for `sentence-transformers/all-MiniLM-L6-v2`.

How to download

1) Using git (recommended if you want the full repo locally — requires git-lfs):

```powershell
git lfs install
git clone https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2 examples/.models/all-minilm-l6-v2
```

2) Using the Hugging Face Python libraries (will download to HF cache):

```python
from sentence_transformers import SentenceTransformer
model = SentenceTransformer('sentence-transformers/all-MiniLM-L6-v2')
# To load from the local folder after cloning:
# model = SentenceTransformer('examples/.models/all-minilm-l6-v2')
```

Notes
- If the model is private, run `huggingface-cli login` first and provide a valid token.
- Respect the model license shown on the model page: https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2

License & attribution
- See the model page for license and citation details.

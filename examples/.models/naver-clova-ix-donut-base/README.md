# naver-clova/ix-donut-base

This folder is intended to hold the model files for `naver-clova/ix-donut-base` (Donut image-to-text / document understanding model).

How to download

1) Using git (requires git-lfs):

```powershell
git lfs install
git clone https://huggingface.co/naver-clova/ix-donut-base examples/.models/naver-clova-ix-donut-base
```

2) Using the Hugging Face Hub in Python (example using Transformers/Donut integrations):

```python
# Clone the model locally then load from the folder, or use from_pretrained directly which downloads to the HF cache
from transformers import AutoFeatureExtractor, AutoTokenizer, AutoModel
# Replace with actual loader code for Donut/vision-encoder-decoder; many Donut implementations load from a local folder:
# feature_extractor = AutoFeatureExtractor.from_pretrained('naver-clova/ix-donut-base')
# model = AutoModel.from_pretrained('naver-clova/ix-donut-base')

# Or, after cloning:
# model = AutoModel.from_pretrained('examples/.models/naver-clova-ix-donut-base')
```

Notes
- Donut-style models often require image processors and additional pre/post-processing; refer to the model card for examples and code snippets.
- If the model is private, run `huggingface-cli login` first.
- Model page: https://huggingface.co/naver-clova/ix-donut-base

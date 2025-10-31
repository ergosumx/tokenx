---
base_model: facebook/nougat-base
library_name: transformers.js
pipeline_tag: image-to-text
tags:
- vision
- nougat
---

https://huggingface.co/facebook/nougat-base with ONNX weights to be compatible with Transformers.js.


## Usage (Transformers.js)

If you haven't already, you can install the [Transformers.js](https://huggingface.co/docs/transformers.js) JavaScript library from [NPM](https://www.npmjs.com/package/@xenova/transformers) using:
```bash
npm i @xenova/transformers
```

You can then use the model to convert images of scientific PDFs into markdown like this:

```js
import { pipeline } from '@xenova/transformers';

// Create an image-to-text pipeline
const pipe = await pipeline('image-to-text', 'Xenova/nougat-base');

// Generate markdown
const url = 'https://huggingface.co/datasets/Xenova/transformers.js-docs/resolve/main/nougat_paper.png';
const output = await pipe(url, {
  min_length: 1,
  max_new_tokens: 40,
  bad_words_ids: [[pipe.tokenizer.unk_token_id]],
});
console.log(output);
// [{ generated_text: "# Nougat: Neural Optical Understanding for Academic Documents\n\n Lukas Blecher\n\nCorrespondence to: liblecher@meta.com\n\nGuillem Cucurull" }]
```


---

Note: Having a separate repo for ONNX weights is intended to be a temporary solution until WebML gains more traction. If you would like to make your models web-ready, we recommend converting to ONNX using [🤗 Optimum](https://huggingface.co/docs/optimum/index) and structuring your repo like this one (with ONNX weights located in a subfolder named `onnx`).
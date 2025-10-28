# Nougat: Neural Optical Understanding for Academic Documents

 Lukas Blecher

Correspondence to: lblecher@meta.com

Guillem Cucurull

Thomas Scialom

Robert Stojnic

Meta AI

###### Abstract

Scientific knowledge is predominantly stored in books and scientific journals, often in the form of PDFs. However, the PDF format leads to a loss of semantic information, particularly for mathematical expressions. We propose Nougat (Neural Optical Understanding for Academic Documents), a Visual Transformer model that performs an _Optical Character Recognition_ (OCR) task for processing scientific documents into a markup language, and demonstrate the effectiveness of our model on a new dataset of scientific documents. The proposed approach offers a promising solution to enhance the accessibility of scientific knowledge in the digital age, by bridging the gap between human-readable documents and machine-readable text. We release the models and code to accelerate future work on scientific text recognition.

## 1 Introduction

The majority of scientific knowledge is stored in books or published in scientific journals, most commonly in the Portable Document Format (PDF). Next to HTML, PDFs are the second most prominent data format on the internet, making up 2.4% of common crawl [1]. However, the information stored in these files is very difficult to extract into any other formats. This is especially true for highly specialized documents, such as scientific research papers, where the semantic information of mathematical expressions is lost.

Existing Optical Character Recognition (OCR) engines, such as Tesseract OCR [2], excel at detecting and classifying individual characters and words in an image, but fail to understand the relationship between them due to their line-by-line approach. This means that they treat superscripts and subscripts in the same way as the surrounding text, which is a significant drawback for mathematical expressions. In mathematical notations like fractions, exponents, and matrices, relative positions of characters are crucial.

Converting academic research papers into machine-readable text also enables accessibility and searchability of science as a whole. The information of millions of academic papers can not be fully accessed because they are locked behind an unreadable format. Existing corpora, such as the S2ORC dataset [3], capture the text of 12M2 papers using GROBID [4], but are missing meaningful representations of the mathematical equations.

Footnote 2: The paper reports 8.1M papers but the authors recently updated the numbers on the GitHub page https://github.com/allenai/s2orc        

To this end, we introduce Nougat, a transformer based model that can convert images of document pages to formatted markup text.

The primary contributions in this paper are

* Release of a pre-trained model capable of converting a PDF to a lightweight markup language. We release the code and the model on GitHub3 Footnote 3: https://github.com/facebookresearch/nougat
* We introduce a pipeline to create dataset for pairing PDFs to source code
* Our method is only dependent on the image of a page, allowing access to scanned papers and books

---

Optical Character Recognition (OCR) is an extensively researched field in computer vision for a variety applications, such as document digitalization [2; 5], handwriting recognition and scene text recognition [6; 7; 8].

More concretely, recognizing mathematical expressions is a heavily researched subtopic. Grammar based methods [9; 10; 11] for handwritten mathematical expressions were improved upon by different encoder-decoder models. The fully convolutional model [12] was succeeded by various RNN decoder models [13; 14; 15; 16; 17], both for handwritten and printed formulas. Recently, the decoder [18; 19] as well as the encoder [20] were replaced with the Transformer [21] architecture.

Visual Document Understanding (VDU) is another related topic of deep learning research and focuses on extracting relevant information of a variety of document types. Previous works depend on pre-trained models that learn to extract information by jointly modeling text and layout information using the Transformer architecture. The LayoutLM model family [22; 23; 24] uses masked layout prediction task to capture the spatial relationships between different document elements.

Open source solutions with a related goal as ours include GROBID [4], which parses digital-born scientific documents to XML with a focus on the bibliographic data and pdf2htmlEX [25], that converts digital-born PDFs to HTML while preserving the layout and appearance of the document. However, both solutions can not recover the semantic information of mathematical expressions.

## 3 Model

Previous VDU methods either rely on OCR text from a third party tool [22; 23; 26] or focus on document types such as receipts, invoices or form-like documents [27]. Recent studies [28; 29] show that an external OCR engine is not necessarily needed to achieve competitive results in VDU.      

The architecture is a encoder-decoder transformer [21] architecture, that allows for an end-to-end training procedure. We build on the Donut [28] architecture. The model does not require any OCR related inputs or modules. The text is recognized implicitly by the network. See Fig. 1 for an overview of the approach.



**Encoder** The visual encoder receives a document image \(\mathbf{x}\in\mathbb{R}^{3\times R_{0}\times W_{0}}\), crops the margins and resizes the image to fit in a fixed rectangle of size \((H,\ W)\). If the image is similar than the rectangle, additional padding is added to ensure each image has the same dimensionality. We use a Swin Transformer [30], a hierarchical vision transformer [31] that splits the image into non-overlapping windows of fixed size and applies a series of self-attention layers to aggregate information across these windows. The model output a sequence of the embedded patches \(\mathbf{z}\in\mathbb{R}^{d\times N}\) where \(d\) is the latent dimension and \(N\) is the number of patches.

**Decoder** The encoded image \(\mathbf{z}\) is decoded into a sequence of tokens using a transformer decoder architecture with cross-attention. The tokens are generated in an auto-regressive manner, using self-attention and cross-attention to attend to different parts of the input sequence and encoder output respectively. Finally, the output is projected to the size of the vocabulary \(v\), yielding the logits \(\boldsymbol{\ell}\in\mathbb{R}^{v}\).

Following Kim et al. [28], we use the implementation of the mBART [32] decoder. We use the same tokenizer as Taylor et al. [33] because their model is also specialized in the scientific text domain.

Figure 1: Our simple end-to-end architecture followin Donut [28]. The Swin Transformer encoder takes a document image and converts it into latent embeddings, which are subsequently converted to a sequence of tokens in a auto-regressive manner

--- 

### Setup

We render the document images at a resolution of 96 DPI. Due to the restrictive possible input dimensions of the Swin Transformer we choose the input size \((H,\,W)=(896,\,672)\). The aspect ratio is in between the US letter and Din A4 format \(\frac{22}{17}<\frac{4}{3}<\sqrt{2}\). The document images are resized and then padded to achieve the desired input size. This input size allows us to use the Swin base model architecture [30]. We initialize the model with the pre-trained weights.

The Transformer decoder has a maximal sequence length of \(S=4096\). This relatively large sizing is due to the fact that the text of academic research papers can be dense and the syntax for tables in particular is token intensive. The BART decoder is a decoder-only transformer with 10 layers. The entire architecture has a total of 350M parameters. We also test experiment with a smaller model (250M parameters) with a slightly smaller sequence length of \(S=3584\) and only 4 decoder layers, where we start from the pre-trained base model.

During inference the text is generated using greedy decoding.

**Training** We use an AdamW optimizer [34] to train for 3 epochs with an effective batch size of 192. Due to training instabilities, we choose a learning rate of \(\mathrm{lr}_{\mathrm{init}}=5\cdot 10^{-5}\) which is reduced by a factor of \(0.9996\) every 15 updates until it reaches \(\mathrm{lr}_{\mathrm{end}}=7.5\cdot 10^{-6}\).

### Data Augmentation

In image recognition tasks, it is often beneficial to use data augmentation to improve generalization. Since we are only using digital-born academic research papers, we need to employ a number of transformations to simulate the imperfections and variability of scanned documents. These transformations include erosion, dilation, gaussian noise, gaussian blur, bitmap conversion, image compression, grid distortion and elastic transform [35]. Each has a fixed probability of being applied to a given image. The transformations are implemented in the _Albumentations_[36] library. For an overview of the effect of each transformation, see Fig. 2.

During training time, we also add perturbations to the ground truth text by randomly replacing tokens. We found this to reduce the collapse into a repeating loop significantly. For more details, see Section 5.4.
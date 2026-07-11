from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import numpy as np
import torch
from typing import Optional

app = FastAPI()

_models = {}


def _load_bge_m3():
    from FlagEmbedding import BGEM3FlagModel
    return BGEM3FlagModel("BAAI/bge-m3", use_fp16=False)


def _load_multilingual_e5():
    from sentence_transformers import SentenceTransformer
    return SentenceTransformer("intfloat/multilingual-e5-base")


def _load_phobert():
    from transformers import AutoTokenizer, AutoModel
    tokenizer = AutoTokenizer.from_pretrained("vinai/phobert-base")
    model = AutoModel.from_pretrained("vinai/phobert-base")
    model.eval()
    return tokenizer, model


def _get_model(name: str):
    if name not in _models:
        if name == "bge-m3":
            _models[name] = _load_bge_m3()
        elif name == "multilingual-e5-base":
            _models[name] = _load_multilingual_e5()
        elif name == "phobert-base":
            _models[name] = _load_phobert()
        else:
            raise ValueError(f"Model không được hỗ trợ: {name}")
    return _models[name]


def _mean_pool(token_embeddings, attention_mask):
    mask = attention_mask.unsqueeze(-1).expand(token_embeddings.size()).float()
    return torch.sum(token_embeddings * mask, 1) / torch.clamp(mask.sum(1), min=1e-9)


class EmbedRequest(BaseModel):
    model: str
    text: str


class EmbedResponse(BaseModel):
    model: str
    dimensions: int
    embedding: list[float]


@app.get("/health")
def health():
    return {
        "status": "ok",
        "supported_models": ["bge-m3", "multilingual-e5-base", "phobert-base"]
    }


@app.post("/embed", response_model=EmbedResponse)
def embed(request: EmbedRequest):
    model_name = request.model.lower().strip()

    if not request.text or request.text.strip() == "":
        return EmbedResponse(model=model_name, dimensions=0, embedding=[])

    if model_name == "bge-m3":
        model = _get_model("bge-m3")
        result = model.encode(
            [request.text],
            batch_size=1,
            max_length=8192,
            return_dense=True,
            return_sparse=False,
            return_colbert_vecs=False
        )
        vector = np.asarray(result["dense_vecs"][0], dtype=np.float32)

    elif model_name == "multilingual-e5-base":
        model = _get_model("multilingual-e5-base")
        text_with_prefix = "query: " + request.text
        vector = model.encode(
            text_with_prefix,
            normalize_embeddings=True,
            convert_to_numpy=True
        ).astype(np.float32)

    elif model_name == "phobert-base":
        tokenizer, model = _get_model("phobert-base")
        inputs = tokenizer(
            request.text,
            return_tensors="pt",
            truncation=True,
            max_length=256,
            padding=True
        )
        with torch.no_grad():
            outputs = model(**inputs)
        vector = _mean_pool(
            outputs.last_hidden_state,
            inputs["attention_mask"]
        )[0].numpy().astype(np.float32)

    else:
        raise HTTPException(
            status_code=400,
            detail=f"Model không được hỗ trợ: {request.model}. Hỗ trợ: bge-m3, multilingual-e5-base, phobert-base"
        )

    norm = np.linalg.norm(vector)
    if norm > 0:
        vector = vector / norm

    embedding = vector.tolist()

    return EmbedResponse(
        model=model_name,
        dimensions=len(embedding),
        embedding=embedding
    )

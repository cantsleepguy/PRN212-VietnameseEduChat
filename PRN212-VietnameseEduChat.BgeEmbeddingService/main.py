from fastapi import FastAPI
from pydantic import BaseModel
from FlagEmbedding import BGEM3FlagModel
import numpy as np

app = FastAPI()

model = BGEM3FlagModel(
    "BAAI/bge-m3",
    use_fp16=False
)


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
        "model": "bge-m3"
    }


@app.post("/embed", response_model=EmbedResponse)
def embed(request: EmbedRequest):
    if request.model.lower() != "bge-m3":
        raise ValueError("This local service only supports bge-m3.")

    if request.text is None or request.text.strip() == "":
        return EmbedResponse(
            model="bge-m3",
            dimensions=0,
            embedding=[]
        )

    result = model.encode(
        [request.text],
        batch_size=1,
        max_length=8192,
        return_dense=True,
        return_sparse=False,
        return_colbert_vecs=False
    )

    dense_vecs = result["dense_vecs"]
    vector = np.asarray(dense_vecs[0], dtype=np.float32)

    norm = np.linalg.norm(vector)

    if norm > 0:
        vector = vector / norm

    embedding = vector.tolist()

    return EmbedResponse(
        model="bge-m3",
        dimensions=len(embedding),
        embedding=embedding
    )
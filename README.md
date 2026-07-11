# VietnameseEduChat

Ứng dụng chatbot giáo dục AI dành cho sinh viên Việt Nam, sử dụng RAG (Retrieval-Augmented Generation). Sinh viên có thể chat với tài liệu học tập (PDF, DOCX), hỏi đáp theo môn học/chương, và thực nghiệm so sánh các chiến lược chunking và embedding model.

---

## Tính năng chính

### 1. Quản lý tài liệu
- Upload PDF, DOCX, PPTX (slide bài giảng)
- Tự động trích xuất văn bản + OCR (Tesseract) cho tài liệu scan
- Tự động chunk & embed tài liệu khi được duyệt
- Quản lý theo môn học và chương
- Xem danh sách tài liệu đã index với trạng thái xử lý

### 2. Chat & Hỏi đáp (RAG)
- Chat tự nhiên theo ngữ cảnh hội thoại
- Trích dẫn nguồn tài liệu gốc trong câu trả lời
- Giới hạn trả lời trong phạm vi tài liệu đã upload
- Lịch sử hội thoại theo phiên (session)
- Bộ lọc theo môn học khi chat

### 3. Module nghiên cứu / RBL Benchmark
- So sánh RAG vs Fine-tuned vs Base model
- Benchmark nhiều **chunking strategy**:
  - Fixed-size (baseline 1200/200)
  - Paragraph-based
  - Sentence-based
  - Semantic chunking (MVP)
- Benchmark nhiều **embedding model**:
  - `text-embedding-3-small` (OpenAI, 1536 chiều)
  - `text-embedding-3-large` (OpenAI, 3072 chiều)
  - `multilingual-e5-base` (miễn phí, local, 768 chiều)
  - `phobert-base` (tiếng Việt, local, 768 chiều)
  - `bge-m3` (BAAI, local, 1024 chiều)
- Dashboard kết quả với 4 metrics: Answer Similarity, Context Relevance, Groundedness, Keyword Hit
- **Export kết quả benchmark ra CSV**
- Quản lý test set 50 câu hỏi + ground truth (import CSV)

---

## Tech Stack

| Layer | Công nghệ |
|-------|-----------|
| Backend/Frontend | ASP.NET Core 8.0 Razor Pages |
| Database | SQL Server (LocalDB / SQL Server Express) |
| ORM | Entity Framework Core 8.0 |
| AI - Chat | OpenAI GPT-4o-mini |
| AI - Embedding | OpenAI API + Local Python FastAPI |
| OCR | Tesseract OCR + Magick.NET + PdfPig |
| Auth | Cookie-based, Role-based (System Admin, Academic Admin, Lecturer, Student) |
| Local Embedding | Python FastAPI (bge-m3, multilingual-e5-base, phobert-base) |

---

## Cấu trúc dự án

```
PRN212-VietnameseEduChat/           # Web UI (Razor Pages) - port 5000
PRN212-VietnameseEduChat.BusinessObjects/  # Entities & DTOs
PRN212-VietnameseEduChat.DataAccess/       # EF Core DbContext & migrations
PRN212-VietnameseEduChat.Repositories/    # Repository pattern
PRN212-VietnameseEduChat.Services/        # Business logic (OCR, Chat, Embedding, Research)
PRN212-VietnameseEduChat.BgeEmbeddingService/  # Python FastAPI - local embedding
```

---

## Yêu cầu hệ thống

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [SQL Server](https://www.microsoft.com/sql-server) hoặc SQL Server LocalDB (đi kèm Visual Studio)
- [Python 3.10+](https://www.python.org/) (chỉ cần nếu dùng local embedding models)
- OpenAI API Key

---

## Cài đặt và chạy

### Bước 1 — Clone repository

```bash
git clone https://github.com/cantsleepguy/PRN212-VietnameseEduChat.git
cd PRN212-VietnameseEduChat
```

### Bước 2 — Cấu hình

Mở file `PRN212-VietnameseEduChat/appsettings.json` và điền:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=VietnameseEduChatDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "OpenAI": {
    "ApiKey": "sk-...",
    "EmbeddingModel": "text-embedding-3-small",
    "ChatModel": "gpt-4o-mini"
  }
}
```

> **SQL Server Express:** Thay connection string thành:  
> `Server=.\SQLEXPRESS;Database=VietnameseEduChatDb;Trusted_Connection=True;TrustServerCertificate=True`

### Bước 3 — Chạy ứng dụng web

```bash
dotnet run --project PRN212-VietnameseEduChat/PRN212-VietnameseEduChat.csproj
```

Lần đầu chạy, app sẽ tự động:
1. Tạo database và migrate schema
2. Seed tài khoản mặc định

Truy cập tại: **http://localhost:5000**

### Bước 4 — (Tùy chọn) Chạy Python embedding service

Chỉ cần nếu muốn dùng `multilingual-e5-base`, `phobert-base` hoặc `bge-m3`:

```bash
cd PRN212-VietnameseEduChat.BgeEmbeddingService
pip install -r requirements.txt
uvicorn main:app --host 127.0.0.1 --port 8001
```

---

## Tài khoản mặc định

| Email | Mật khẩu | Vai trò |
|-------|---------|---------|
| systemadmin@gmail.com | 123456 | System Admin |
| academicadmin@gmail.com | 123456 | Academic Admin |
| lecturer@gmail.com | 123456 | Lecturer |
| student@gmail.com | 123456 | Student |

---

## Test set 50 câu hỏi

File CSV mẫu để import vào module Research có tại:

```
PRN212-VietnameseEduChat/wwwroot/downloads/sample-testset-50.csv
```

Import tại: **Academic Admin → Research Questions → Import CSV**

Format CSV:
```
SubjectName,ChapterName,SourceDocumentName,Question,GroundTruthAnswer,ExpectedKeywords,ExpectedSource
```

---

## Hướng dẫn sử dụng nhanh

### Workflow cơ bản (Student/Lecturer)
1. Đăng nhập → **Documents** → Upload tài liệu PDF
2. Chờ Academic Admin duyệt tài liệu
3. Vào **Chat** → Chọn môn học → Bắt đầu hỏi đáp

### Workflow Research (Academic Admin)
1. **Research Questions** → Import 50 câu hỏi từ CSV mẫu
2. **Research Experiments** → Tạo experiment (chọn embedding model + chunking strategy)
3. Bấm **Chạy** → Chờ benchmark hoàn thành
4. Bấm **CSV** để xuất kết quả
5. So sánh kết quả giữa các experiment

---

## Metrics đánh giá

| Metric | Mô tả | Trọng số |
|--------|-------|---------|
| Answer Similarity | Cosine similarity giữa generated answer và ground truth | 40% |
| Context Relevance | Độ liên quan của chunks được retrieve | 20% |
| Groundedness | Câu trả lời có dựa trên context không | 20% |
| Keyword Hit | Tỷ lệ keyword kỳ vọng xuất hiện trong câu trả lời | 20% |

---

## Ghi chú

- Ghostscript (Windows): nếu cài đặt tại `C:\Program Files\gs\gs10.07.1\bin`, app tự động sử dụng để xử lý PDF phức tạp
- Python embedding service khởi động lần đầu sẽ tự download model (~1-2GB) từ HuggingFace
- Mọi file upload được lưu tại `PRN212-VietnameseEduChat/wwwroot/uploads/`

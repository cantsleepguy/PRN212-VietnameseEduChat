# VietnameseEduChat

An AI-powered educational platform for Vietnamese students using RAG (Retrieval-Augmented Generation). Students can chat with educational documents (PDFs), ask questions by subject/chapter, and benchmark different chunking strategies and embedding models.

## Tech Stack

- **Backend/Frontend**: ASP.NET Core 8.0 Razor Pages
- **Database**: PostgreSQL (via Npgsql EF Core provider)
- **AI**: OpenAI API (embeddings + chat completion), optional local BGE-M3 embedding service
- **OCR**: Tesseract OCR + ImageMagick/Magick.NET + PdfPig
- **Auth**: Cookie-based authentication

## Project Structure

- `PRN212-VietnameseEduChat/` — Web UI (Razor Pages), port 5000
- `PRN212-VietnameseEduChat.BusinessObjects/` — Entities and DTOs
- `PRN212-VietnameseEduChat.DataAccess/` — EF Core DbContext and migrations
- `PRN212-VietnameseEduChat.Repositories/` — Repository pattern
- `PRN212-VietnameseEduChat.Services/` — Business logic (OCR, Chat, Embedding)
- `PRN212-VietnameseEduChat.BgeEmbeddingService/` — Optional Python FastAPI for local BGE-M3

## Running the App

The main workflow starts the web app:
```
dotnet run --project PRN212-VietnameseEduChat/PRN212-VietnameseEduChat.csproj
```

## Configuration

- Database connection is built from `PGHOST`, `PGPORT`, `PGUSER`, `PGPASSWORD`, `PGDATABASE` env vars (auto-set by Replit)
- OpenAI API key needed for AI features: set `OpenAI:ApiKey` in appsettings.json or as environment variable
- App runs on `http://0.0.0.0:5000`
- Default seed accounts: `systemadmin@gmail.com`, `academicadmin@gmail.com`, `lecturer@gmail.com`, `student@gmail.com` (all password: `123456`)

## Notes

- SQL Server migrations were replaced with PostgreSQL (Npgsql) migrations
- `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)` is set to handle `DateTime.Kind=Local` timestamps
- Ghostscript integration only activates on Windows (path `C:\Program Files\gs\...`)
- The BGE-M3 local embedding service is optional; the app defaults to OpenAI embeddings

## User Preferences

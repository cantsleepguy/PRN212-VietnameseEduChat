# VietnameseEduChat

An AI-powered educational platform for Vietnamese students using RAG (Retrieval-Augmented Generation). Students can chat with educational documents (PDFs), ask questions by subject/chapter, and benchmark different chunking strategies and embedding models.

## Tech Stack

- **Backend/Frontend**: ASP.NET Core 8.0 Razor Pages
- **Database**: SQL Server (via Microsoft.EntityFrameworkCore.SqlServer)
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

- Database connection string in `appsettings.json`: `Server=(localdb)\\mssqllocaldb;Database=VietnameseEduChatDb;Trusted_Connection=True;...`
- OpenAI API key needed for AI features: set `OpenAI:ApiKey` in appsettings.json or as environment variable
- App runs on `http://0.0.0.0:5000`
- Default seed accounts: `systemadmin@gmail.com`, `academicadmin@gmail.com`, `lecturer@gmail.com`, `student@gmail.com` (all password: `123456`)

## Key Features (July 2026 update)

- **Admin-configurable chunking**: 3 strategies (Paragraph / Character / FixedSize) stored in DB (`ChunkingConfigurations` table), managed at `/Admin/ChunkingConfig`. Only one active at a time; new uploads use the active config. Old documents can be re-indexed from that page (deletes old chunks + re-chunks + re-embeds, keeps page numbers).
- **SignalR streaming chat**: `/hubs/chat` hub with `AskStream` server-to-client stream (Session → Sources → Token* → Done/Error events). Chat UI renders markdown (marked.js) and clickable citations (document name + page + chunk index → `/Documents/Details?id=X`). Non-streaming POST fallback retained.
- **Packages & mock payments**: `Packages`, `UserSubscriptions`, `Payments` tables. Users buy packages at `/Packages/Index` → mock checkout at `/Packages/MockCheckout` (behind `IPaymentProvider` abstraction — `MockPaymentProvider` can be swapped for VNPay later). Limits enforced in services: daily question limit, max upload size, max documents.
- **Admin dashboard**: `/Admin/Dashboard` with Chart.js charts (revenue by day/package, package distribution, questions by day), plus `/Admin/Packages` (edit packages) and `/Admin/Payments` (all transactions).
- Frontend libs served locally from `wwwroot/lib/`: signalr, marked, chartjs.

## Notes

- Database: SQL Server (LocalDB by default). Change connection string in `appsettings.json` for SQL Server Express or full SQL Server.
- Apply migrations with `dotnet ef database update` (or run the app — it migrates/seeds on startup). Latest migration: `AddPackagesPaymentsChunkingConfig`.
- Ghostscript integration only activates on Windows (path `C:\Program Files\gs\...`)
- The BGE-M3 local embedding service is optional; the app defaults to OpenAI embeddings
- This Replit environment has no SQL Server — the app cannot run here; verify changes with `dotnet build` only. The user runs it locally on Windows.

## User Preferences

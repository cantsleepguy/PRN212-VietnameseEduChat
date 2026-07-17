# VietnameseEduChat

VietnameseEduChat là đồ án chatbot giáo dục dành cho sinh viên Việt Nam, xây dựng bằng ASP.NET Core 8 Razor Pages. Ứng dụng dùng RAG (Retrieval-Augmented Generation) để trả lời câu hỏi dựa trên tài liệu học tập và hiển thị nguồn tham khảo.

## Tính năng

- Đăng nhập cookie và phân quyền Student, Lecturer, Academic Admin, System Admin.
- Quản lý môn học, chương và phân công giảng viên.
- Tải lên PDF, DOCX, PPTX; trích xuất văn bản, OCR, chunk và embedding.
- Xử lý tài liệu bằng hàng đợi nền, không giữ request upload trong lúc gọi AI.
- Chat RAG theo môn học và lưu lịch sử hội thoại.
- Benchmark chiến lược chunking và embedding model.
- Quản lý gói dịch vụ và thanh toán VNPay sandbox.
- Dashboard riêng theo từng vai trò.
- Health checks và bộ test tự động.

## Công nghệ

| Thành phần | Công nghệ |
| --- | --- |
| Web | ASP.NET Core 8 Razor Pages, SignalR |
| Database | SQL Server, Entity Framework Core 8 |
| AI | OpenAI Chat Completion và Embedding |
| Xử lý tài liệu | PdfPig, Open XML, Tesseract, Magick.NET |
| Local embedding | Python FastAPI |
| Test | xUnit, ASP.NET Core Test Host, SQLite |

## Yêu cầu

- .NET 8 SDK.
- SQL Server LocalDB hoặc SQL Server Express.
- Python 3.10+ chỉ khi dùng local embedding.
- OpenAI API key cho chat và embedding.
- Tài khoản VNPay sandbox nếu muốn demo thanh toán.

## Cấu hình an toàn

Sao chép file mẫu:

```powershell
Copy-Item PRN212-VietnameseEduChat/appsettings.example.json PRN212-VietnameseEduChat/appsettings.json
```

Không commit `appsettings.json` hoặc API key. Lưu secret bằng .NET User Secrets:

```powershell
dotnet user-secrets --project PRN212-VietnameseEduChat set "OpenAI:ApiKey" "YOUR_OPENAI_KEY"
dotnet user-secrets --project PRN212-VietnameseEduChat set "VnPay:TmnCode" "YOUR_TMN_CODE"
dotnet user-secrets --project PRN212-VietnameseEduChat set "VnPay:HashSecret" "YOUR_HASH_SECRET"
```

Các cấu hình quan trọng:

```json
{
  "Database": {
    "AutoMigrate": true
  },
  "DemoData": {
    "Enabled": true
  },
  "DocumentStorage": {
    "RootPath": "App_Data/documents"
  }
}
```

- `Database:AutoMigrate=true`: tự chạy EF Core migrations khi khởi động.
- `DemoData:Enabled=true`: tạo dữ liệu và tài khoản demo.
- Khi triển khai môi trường thật, đặt cả hai giá trị thành `false` và quản lý migration/tài khoản riêng.
- `DocumentStorage:RootPath` nằm ngoài `wwwroot`; tài liệu chỉ được đọc qua endpoint có kiểm tra quyền.
- `Ghostscript:Directory` là tùy chọn. Bỏ trống nếu máy không cài Ghostscript.

## Chạy ứng dụng

```powershell
dotnet restore PRN212-VietnameseEduChat.sln
dotnet run --project PRN212-VietnameseEduChat/PRN212-VietnameseEduChat.csproj --launch-profile http
```

Mặc định launch profile HTTP dùng `http://localhost:5233`. Có thể đổi cổng:

```powershell
dotnet run --project PRN212-VietnameseEduChat/PRN212-VietnameseEduChat.csproj --urls http://localhost:5234
```

## Tài khoản demo

Các tài khoản sau chỉ được tạo khi `DemoData:Enabled=true`:

| Email | Mật khẩu | Vai trò |
| --- | --- | --- |
| `systemadmin@gmail.com` | `123456` | System Admin |
| `academicadmin@gmail.com` | `123456` | Academic Admin |
| `lecturer@gmail.com` | `123456` | Lecturer |
| `student@gmail.com` | `123456` | Student |

Không bật seed demo trên hệ thống công khai.

## Luồng xử lý tài liệu

1. Lecturer chọn môn, chương và tải file lên.
2. Hệ thống kiểm tra dung lượng, đuôi file, MIME type và chữ ký nội dung.
3. File được lưu riêng tư với tên GUID; bản ghi chuyển sang `Queued`.
4. `DocumentProcessingWorker` chuyển trạng thái sang `Processing`, thực hiện trích xuất, chunk và embedding.
5. Thành công chuyển sang `PendingApproval`; lỗi chuyển sang `Failed` với thông báo an toàn.
6. Khi ứng dụng khởi động lại, tài liệu `Queued` hoặc đang `Processing` được đưa lại vào hàng đợi.

## Chạy test

```powershell
dotnet test PRN212-VietnameseEduChat.sln
pwsh -File scripts/verify-role-ui.ps1
pwsh -File scripts/verify-repository-hygiene.ps1
```

Test không gọi OpenAI, VNPay, OCR executable hoặc local embedding service. GitHub Actions tự động restore, build, test và kiểm tra repository trên mỗi push và Pull Request.

## Health checks

- `GET /health/live`: tiến trình web đang hoạt động.
- `GET /health/ready`: ứng dụng kết nối được database.

Health check không gọi API AI nên không tiêu tốn quota và không phụ thuộc mạng ngoài.

## Local embedding tùy chọn

```powershell
Set-Location PRN212-VietnameseEduChat.BgeEmbeddingService
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
uvicorn main:app --host 127.0.0.1 --port 8001
```

## Research benchmark

File mẫu 50 câu hỏi nằm tại:

`PRN212-VietnameseEduChat/wwwroot/downloads/sample-testset-50.csv`

Academic Admin có thể import file này trong Research Questions, tạo experiment, chạy benchmark và xuất kết quả CSV.

## Nguyên tắc repository

- Không commit secret, `appsettings.json`, database cục bộ hoặc file người dùng tải lên.
- Chỉ file dữ liệu mẫu có chủ đích mới được đặt trong `wwwroot/downloads`.
- Mỗi thay đổi phải build và chạy test trước khi tạo Pull Request.

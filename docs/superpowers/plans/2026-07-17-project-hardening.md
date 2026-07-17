# Project Hardening Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Harden the classroom project with meaningful automated tests, private validated uploads, recoverable background indexing, idempotent VNPay fulfillment, explicit startup behavior, health checks, CI, and safe documentation.

**Architecture:** Keep the existing Razor Pages and service/repository layers. Add focused storage, validation, queue, processor, startup, and access-policy units with interfaces that can be tested without external AI or payment calls. Use an in-memory `Channel<int>` queue with database-backed recovery because the target is a single-instance course demo.

**Tech Stack:** .NET 8, ASP.NET Core Razor Pages, EF Core 8/SQL Server, xUnit, `Microsoft.AspNetCore.Mvc.Testing`, EF Core SQLite test database, GitHub Actions.

## Global Constraints

- Do not add Redis, RabbitMQ, Hangfire, Docker, or paid services.
- Do not call OpenAI, VNPay, OCR executables, or the local embedding server from automated tests.
- Accept only PDF, DOCX, and PPTX files no larger than 25 MB.
- Store runtime uploads outside `wwwroot`; never expose filesystem paths.
- Keep the four existing roles and business workflows unchanged.
- New behavior must follow red-green-refactor and each task ends with a focused commit.
- User-visible errors are Vietnamese and must not contain exception messages, secrets, or physical paths.

---

### Task 1: Automated test foundation

**Files:**
- Create: `PRN212-VietnameseEduChat.Tests/PRN212-VietnameseEduChat.Tests.csproj`
- Create: `PRN212-VietnameseEduChat.Tests/GlobalUsings.cs`
- Create: `PRN212-VietnameseEduChat.Tests/Architecture/AuthorizationMetadataTests.cs`
- Modify: `PRN212-VietnameseEduChat.sln`

**Interfaces:**
- Consumes: existing Razor Page model types and `AppRoles` constants.
- Produces: a test project that later tasks extend and the command `dotnet test PRN212-VietnameseEduChat.Tests/PRN212-VietnameseEduChat.Tests.csproj`.

- [ ] **Step 1: Scaffold the test project and add it to the solution**

Run:

```powershell
dotnet new xunit -n PRN212-VietnameseEduChat.Tests -f net8.0
dotnet sln PRN212-VietnameseEduChat.sln add PRN212-VietnameseEduChat.Tests/PRN212-VietnameseEduChat.Tests.csproj
dotnet add PRN212-VietnameseEduChat.Tests/PRN212-VietnameseEduChat.Tests.csproj reference PRN212-VietnameseEduChat/PRN212-VietnameseEduChat.csproj PRN212-VietnameseEduChat.Services/PRN212-VietnameseEduChat.Services.csproj PRN212-VietnameseEduChat.DataAccess/PRN212-VietnameseEduChat.DataAccess.csproj
dotnet add PRN212-VietnameseEduChat.Tests/PRN212-VietnameseEduChat.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing --version 8.0.28
dotnet add PRN212-VietnameseEduChat.Tests/PRN212-VietnameseEduChat.Tests.csproj package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.28
```

Delete generated `UnitTest1.cs`. Keep `GlobalUsings.cs` with `global using Xunit;`.

- [ ] **Step 2: Write the baseline authorization metadata test**

Create a theory that reflects over page model attributes and expects Lecturer-only upload, AcademicAdmin-only research, SystemAdmin-only user management, and authenticated dashboards. Include cases such as:

```csharp
[Theory]
[InlineData(typeof(Pages.Documents.UploadModel), AppRoles.Lecturer)]
[InlineData(typeof(Pages.ResearchQuestions.IndexModel), AppRoles.AcademicAdmin)]
[InlineData(typeof(Pages.Admin.Users.IndexModel), AppRoles.SystemAdmin)]
public void Sensitive_page_requires_expected_role(Type pageType, string role)
{
    var attribute = pageType.GetCustomAttribute<AuthorizeAttribute>();
    Assert.NotNull(attribute);
    Assert.Contains(role, attribute!.Roles!.Split(','));
}
```

Keep this test limited to existing attribute-enforced page contracts. New SystemAdmin document-access behavior is introduced test-first in Task 3.

- [ ] **Step 3: Run the focused baseline test**

Run:

```powershell
dotnet test PRN212-VietnameseEduChat.Tests/PRN212-VietnameseEduChat.Tests.csproj --filter AuthorizationMetadataTests
```

Expected: PASS. This task is test infrastructure and records existing behavior; production behavior begins with failing tests in Task 2.

- [ ] **Step 4: Run the complete baseline**

Run:

```powershell
dotnet test PRN212-VietnameseEduChat.sln
dotnet build PRN212-VietnameseEduChat.sln --no-restore --nologo
```

Expected: all foundation tests pass; build exits 0.

- [ ] **Step 5: Commit**

```powershell
git add PRN212-VietnameseEduChat.sln PRN212-VietnameseEduChat.Tests
git commit -m "test: add automated test foundation"
```

---

### Task 2: Private storage and file-content validation

**Files:**
- Create: `PRN212-VietnameseEduChat.Services/Options/DocumentStorageOptions.cs`
- Create: `PRN212-VietnameseEduChat.Services/Interfaces/IDocumentStorage.cs`
- Create: `PRN212-VietnameseEduChat.Services/Interfaces/IDocumentFileValidator.cs`
- Create: `PRN212-VietnameseEduChat.Services/Implementations/LocalDocumentStorage.cs`
- Create: `PRN212-VietnameseEduChat.Services/Implementations/DocumentFileValidator.cs`
- Create: `PRN212-VietnameseEduChat.Tests/Documents/DocumentFileValidatorTests.cs`
- Create: `PRN212-VietnameseEduChat.Tests/Documents/LocalDocumentStorageTests.cs`
- Modify: `PRN212-VietnameseEduChat/Program.cs`

**Interfaces:**
- Produces: `Task<ValidatedDocumentFile> ValidateAsync(IFormFile file, CancellationToken)`; `Task<StoredDocumentFile> SaveAsync(Stream content, string extension, CancellationToken)`; `Task<Stream?> OpenReadAsync(string storedFileName, CancellationToken)`; `Task DeleteIfExistsAsync(string storedFileName, CancellationToken)`.
- `ValidatedDocumentFile` contains normalized extension, trusted content type, original safe display name, and length.

- [ ] **Step 1: Write failing validator tests**

Cover an empty upload, more than 25 MB, unsupported extension, PDF extension with non-PDF bytes, a valid `%PDF-` header, valid DOCX/PPTX ZIP header `50 4B 03 04`, and filename `../lesson.pdf`. Assert a traversal name is reduced to `lesson.pdf`.

- [ ] **Step 2: Verify validator RED**

Run:

```powershell
dotnet test PRN212-VietnameseEduChat.Tests/PRN212-VietnameseEduChat.Tests.csproj --filter DocumentFileValidatorTests
```

Expected: compilation fails because `IDocumentFileValidator` does not exist.

- [ ] **Step 3: Implement minimal validation**

Use an immutable allowlist mapping:

```csharp
private static readonly IReadOnlyDictionary<string, string[]> AllowedTypes =
    new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = ["application/pdf"],
        [".docx"] = ["application/vnd.openxmlformats-officedocument.wordprocessingml.document", "application/zip"],
        [".pptx"] = ["application/vnd.openxmlformats-officedocument.presentationml.presentation", "application/zip"]
    };
```

Read only enough leading bytes to validate `%PDF-` or ZIP signatures, rewind seekable streams, and return trusted content type based on extension rather than the browser value.

- [ ] **Step 4: Run validator tests GREEN**

Run the same focused command. Expected: all validator cases pass.

- [ ] **Step 5: Write failing local-storage tests**

Use a temporary root. Assert GUID-based names, writes remain inside the resolved root, read returns exact bytes, deletion is idempotent, and a stored name containing separators is rejected.

- [ ] **Step 6: Implement local storage and register options**

Resolve `DocumentStorage:RootPath` relative to `IHostEnvironment.ContentRootPath`; default to `App_Data/documents`. Validate every final full path begins with the normalized storage root. Register options and services in `Program.cs`.

- [ ] **Step 7: Run focused and full tests, then commit**

```powershell
dotnet test PRN212-VietnameseEduChat.Tests/PRN212-VietnameseEduChat.Tests.csproj --filter "DocumentFileValidatorTests|LocalDocumentStorageTests"
dotnet test PRN212-VietnameseEduChat.sln
git add PRN212-VietnameseEduChat.Services PRN212-VietnameseEduChat.Tests PRN212-VietnameseEduChat/Program.cs
git commit -m "feat: add private validated document storage"
```

---

### Task 3: Authorized document download and upload migration

**Files:**
- Create: `PRN212-VietnameseEduChat.Services/Interfaces/IDocumentAccessPolicy.cs`
- Create: `PRN212-VietnameseEduChat.Services/Implementations/DocumentAccessPolicy.cs`
- Create: `PRN212-VietnameseEduChat.Tests/Documents/DocumentAccessPolicyTests.cs`
- Modify: `PRN212-VietnameseEduChat.Services/Implementations/DocumentService.cs`
- Modify: `PRN212-VietnameseEduChat.Services/Interfaces/IDocumentService.cs`
- Modify: `PRN212-VietnameseEduChat/Pages/Documents/Details.cshtml.cs`
- Modify: `PRN212-VietnameseEduChat/Program.cs`

**Interfaces:**
- Produces: `Task<bool> CanReadAsync(Document document, ClaimsPrincipal user, CancellationToken)` and `Task<DocumentDownload?> OpenDownloadAsync(int documentId, ClaimsPrincipal user, CancellationToken)`.
- `DocumentDownload` contains a stream, trusted content type, original filename, and range-processing capability.

- [ ] **Step 1: Write failing access-policy tests**

Assert admins can read any document; lecturer can read their own or an assigned-subject document; unrelated lecturer cannot; student can read only approved documents; unauthenticated user cannot. Use lightweight fake assignment behavior rather than a live server.

- [ ] **Step 2: Verify access-policy RED**

Run with `--filter DocumentAccessPolicyTests`; expect missing type compilation failure.

- [ ] **Step 3: Implement the access policy and storage-backed download**

Move role checks out of `DetailsModel`. Change preview/download handlers to call the service and return `File(stream, contentType, enableRangeProcessing: true)`. Do not use `IWebHostEnvironment.WebRootPath` or `PhysicalFileResult`.

- [ ] **Step 4: Add a failing upload compensation test**

Arrange storage save success followed by repository failure; assert the stored file is deleted. Arrange validation failure; assert repository and storage are not called.

- [ ] **Step 5: Refactor upload to validator/storage**

Replace direct `FileStream` creation under `wwwroot`. Store only the opaque stored filename in `FilePath`/`StoredFileName`, persist after storage succeeds, and delete the file if persistence fails. Keep existing subject, chapter, and package checks.

- [ ] **Step 6: Run tests and commit**

```powershell
dotnet test PRN212-VietnameseEduChat.Tests/PRN212-VietnameseEduChat.Tests.csproj --filter "DocumentAccessPolicyTests|DocumentService"
dotnet test PRN212-VietnameseEduChat.sln
git add PRN212-VietnameseEduChat.Services PRN212-VietnameseEduChat.Tests PRN212-VietnameseEduChat/Pages/Documents PRN212-VietnameseEduChat/Program.cs
git commit -m "feat: authorize private document downloads"
```

---

### Task 4: Recoverable background document processing

**Files:**
- Create: `PRN212-VietnameseEduChat.Services/Interfaces/IDocumentProcessingQueue.cs`
- Create: `PRN212-VietnameseEduChat.Services/Interfaces/IDocumentProcessor.cs`
- Create: `PRN212-VietnameseEduChat.Services/Implementations/DocumentProcessingQueue.cs`
- Create: `PRN212-VietnameseEduChat.Services/Implementations/DocumentProcessor.cs`
- Create: `PRN212-VietnameseEduChat/HostedServices/DocumentProcessingWorker.cs`
- Create: `PRN212-VietnameseEduChat/HostedServices/DocumentQueueRecoveryService.cs`
- Create: `PRN212-VietnameseEduChat.Tests/Documents/DocumentProcessingQueueTests.cs`
- Create: `PRN212-VietnameseEduChat.Tests/Documents/DocumentProcessorTests.cs`
- Create: `PRN212-VietnameseEduChat.Tests/Documents/DocumentQueueRecoveryTests.cs`
- Modify: `PRN212-VietnameseEduChat.Services/Implementations/DocumentService.cs`
- Modify: `PRN212-VietnameseEduChat.Repositories/Interfaces/IDocumentRepository.cs`
- Modify: `PRN212-VietnameseEduChat.Repositories/Implementations/DocumentRepository.cs`
- Modify: `PRN212-VietnameseEduChat/Program.cs`

**Interfaces:**
- Produces: `ValueTask EnqueueAsync(int documentId, CancellationToken)`; `ValueTask<int> DequeueAsync(CancellationToken)`; `Task ProcessAsync(int documentId, CancellationToken)`; repository query `Task<List<Document>> GetPendingProcessingAsync(CancellationToken)`.

- [ ] **Step 1: Write and fail bounded queue tests**

Assert FIFO delivery, cancellation while empty, and duplicate IDs are coalesced until dequeued. Run `--filter DocumentProcessingQueueTests` and verify missing implementation failure.

- [ ] **Step 2: Implement the channel queue**

Use `Channel.CreateBounded<int>(new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.Wait, SingleReader = true })` plus a locked `HashSet<int>` for queued IDs.

- [ ] **Step 3: Write failing processor transition tests**

Assert `Queued → Processing → PendingApproval`, ineligible final state is ignored, extraction failure becomes `Failed`, exception detail is logged but `ErrorMessage` equals a stable safe message, and original source remains stored.

- [ ] **Step 4: Extract current indexing logic into `DocumentProcessor`**

Move extraction, chunk building, embedding, and status handling out of `UploadAsync`. Upload must now set `Queued`, persist, enqueue, and return. `ReindexAsync` resets to `Queued` and enqueues rather than processing synchronously.

- [ ] **Step 5: Write failing recovery tests**

Given documents in `Queued`, `Processing`, `Approved`, and `Failed`, assert recovery enqueues only the first two and changes interrupted `Processing` to `Queued`.

- [ ] **Step 6: Implement worker/recovery registration**

The singleton queue is shared by scoped upload services and hosted services. The worker creates a DI scope for each ID. Recovery runs after startup, queries pending documents, resets interrupted states, and enqueues each ID.

- [ ] **Step 7: Run all tests/build and commit**

```powershell
dotnet test PRN212-VietnameseEduChat.sln
dotnet build PRN212-VietnameseEduChat.sln --no-restore --nologo
git add PRN212-VietnameseEduChat.Services PRN212-VietnameseEduChat.Repositories PRN212-VietnameseEduChat.Tests PRN212-VietnameseEduChat/HostedServices PRN212-VietnameseEduChat/Program.cs
git commit -m "feat: process documents in recoverable background queue"
```

---

### Task 5: VNPay fulfillment concurrency

**Files:**
- Create: `PRN212-VietnameseEduChat.Tests/Payments/VnPayPaymentServiceTests.cs`
- Modify: `PRN212-VietnameseEduChat.Services/Implementations/PaymentService.cs`
- Modify: `PRN212-VietnameseEduChat.Repositories/Interfaces/IPaymentRepository.cs`
- Modify: `PRN212-VietnameseEduChat.Repositories/Implementations/PaymentRepository.cs`

**Interfaces:**
- Consumes: the existing SQL Server `Payment.RowVersion` concurrency token and transaction boundary.
- Produces: callback processing that applies a payment at most once and returns response code `02` for a duplicate/conflicting callback.

- [ ] **Step 1: Write failing callback behavior tests**

Cover invalid signature (`97`), missing payment (`01`), amount mismatch (`04`), provider failure (`00` without subscription), existing success (`02`), and successful fulfillment (`00`).

- [ ] **Step 2: Add a failing concurrency regression test**

Use two separate EF Core contexts over the same SQLite connection (or SQL Server LocalDB when rowversion semantics are required). Load the same pending payment twice, process both, and assert exactly one subscription mutation. Confirm the test fails with the current read-before-transaction flow.

- [ ] **Step 3: Claim the payment inside the transaction**

Begin the transaction before the final status check, reload the payment, keep the existing `RowVersion` concurrency token, save `Success`, apply the subscription, and commit. Catch `DbUpdateConcurrencyException`, clear tracked state, and return `02` without fulfillment.

- [ ] **Step 4: Run focused/full tests and commit**

```powershell
dotnet test PRN212-VietnameseEduChat.Tests/PRN212-VietnameseEduChat.Tests.csproj --filter VnPayPaymentServiceTests
dotnet test PRN212-VietnameseEduChat.sln
git add PRN212-VietnameseEduChat.Services PRN212-VietnameseEduChat.Repositories PRN212-VietnameseEduChat.Tests
git commit -m "fix: make VNPay fulfillment idempotent"
```

---

### Task 6: Explicit startup behavior and health checks

**Files:**
- Create: `PRN212-VietnameseEduChat/Options/DatabaseStartupOptions.cs`
- Create: `PRN212-VietnameseEduChat/Options/DemoDataOptions.cs`
- Create: `PRN212-VietnameseEduChat/Options/GhostscriptOptions.cs`
- Create: `PRN212-VietnameseEduChat/Health/ConfigurationHealthCheck.cs`
- Create: `PRN212-VietnameseEduChat.Tests/Startup/StartupPolicyTests.cs`
- Create: `PRN212-VietnameseEduChat.Tests/Startup/HealthEndpointTests.cs`
- Modify: `PRN212-VietnameseEduChat/Program.cs`
- Create: `PRN212-VietnameseEduChat/appsettings.example.json`

**Interfaces:**
- Produces configuration keys `Database:AutoMigrate`, `DemoData:Enabled`, `Ghostscript:Directory`, and endpoints `/health/live`, `/health/ready`.

- [ ] **Step 1: Make `Program` test-host compatible and write failing health tests**

Add `public partial class Program { }` only after a test requiring `WebApplicationFactory<Program>` fails. The test host replaces SQL Server with SQLite, disables migrate/seed/background processing, and supplies placeholder VNPay/OpenAI configuration.

- [ ] **Step 2: Add health endpoints**

Register ASP.NET Core health checks. `/health/live` uses a predicate that excludes dependency checks. `/health/ready` includes `AddDbContextCheck<ApplicationDbContext>()` and a configuration check that reports unhealthy when required keys are missing without exposing their values.

- [ ] **Step 3: Write failing startup policy tests**

Extract a small `StartupPolicy` whose methods decide whether migration and seed run from environment plus options. Assert Development defaults may be enabled by example configuration while Production defaults are false unless explicitly enabled.

- [ ] **Step 4: Gate migration/seed and configure Ghostscript**

Replace unconditional `Migrate()`/`SeedAsync()` with option checks. Use configured Ghostscript directory when valid; otherwise probe known platform locations without failing startup. Do not include a machine-specific path in code.

- [ ] **Step 5: Add safe example configuration**

`appsettings.example.json` contains placeholders only and includes all required sections. It must not contain `sk-`, a real VNPay secret, database password, or live URL.

- [ ] **Step 6: Run tests/build and commit**

```powershell
dotnet test PRN212-VietnameseEduChat.Tests/PRN212-VietnameseEduChat.Tests.csproj --filter "StartupPolicyTests|HealthEndpointTests"
dotnet test PRN212-VietnameseEduChat.sln
dotnet build PRN212-VietnameseEduChat.sln --no-restore --nologo
git add PRN212-VietnameseEduChat PRN212-VietnameseEduChat.Tests
git commit -m "feat: add explicit startup policy and health checks"
```

---

### Task 7: Repository hygiene, CI, and documentation

**Files:**
- Create: `.github/workflows/dotnet-ci.yml`
- Modify: `.gitignore`
- Modify: `README.md`
- Create: `scripts/verify-repository-hygiene.ps1`
- Delete: `PRN212-VietnameseEduChat/wwwroot/uploads/documents/*.pdf`
- Delete: `PRN212-VietnameseEduChat/wwwroot/uploads/documents/*.docx`
- Modify: `PRN212-VietnameseEduChat/Pages/Documents/Details.cshtml`

**Interfaces:**
- Produces: repeatable CI and complete classroom setup/run/test documentation.

- [ ] **Step 1: Add a failing repository-hygiene test script/check**

Add assertions to `scripts/verify-role-ui.ps1` or a new `scripts/verify-repository-hygiene.ps1` that fail when tracked runtime uploads exist, real-secret patterns are tracked, or `wwwroot/uploads` is not ignored. Run it and observe failure from current tracked uploads.

- [ ] **Step 2: Remove tracked runtime uploads safely**

Delete only the six tracked files under the exact `PRN212-VietnameseEduChat/wwwroot/uploads/documents` directory. Add `PRN212-VietnameseEduChat/App_Data/documents/` and `PRN212-VietnameseEduChat/wwwroot/uploads/` to `.gitignore`, and add `!PRN212-VietnameseEduChat/appsettings.example.json` so the safe example overrides the existing `appsettings.*.json` ignore rule. Preserve `wwwroot/downloads/sample-testset-50.csv`.

- [ ] **Step 3: Add GitHub Actions CI**

Create a Windows-compatible or Ubuntu-compatible workflow:

```yaml
name: .NET CI
on:
  push:
  pull_request:
jobs:
  build-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 8.0.x
      - run: dotnet restore PRN212-VietnameseEduChat.sln
      - run: dotnet build PRN212-VietnameseEduChat.sln --no-restore --nologo
      - run: dotnet test PRN212-VietnameseEduChat.sln --no-build --nologo
```

Ensure tests do not require LocalDB, external AI, VNPay, OCR, or secrets on CI.

- [ ] **Step 4: Rewrite README as UTF-8 Vietnamese**

Document prerequisites, `dotnet user-secrets`, example configuration, development migrate/seed switches, demo accounts only when seed is enabled, private storage, background status flow, health URLs, test command, and CI. State that secrets must never be committed.

- [ ] **Step 5: Run final repository checks**

```powershell
pwsh -File scripts/verify-role-ui.ps1
pwsh -File scripts/verify-repository-hygiene.ps1
git ls-files | Select-String 'wwwroot/uploads/documents'
git grep -n -E 'sk-proj-|HashSecret.*[A-Za-z0-9]{16,}|ApiKey.*sk-'
git diff --check
```

Expected: verification scripts pass; tracked-upload and secret searches return no matches; diff check is clean.

- [ ] **Step 6: Perform full technical and browser verification**

```powershell
dotnet restore PRN212-VietnameseEduChat.sln
dotnet build PRN212-VietnameseEduChat.sln --no-restore --nologo
dotnet test PRN212-VietnameseEduChat.sln --no-build --nologo
```

Run the app with development migrate/seed enabled. Smoke-test login and a representative page for Student, Lecturer, Academic Admin, and System Admin. Upload a small valid PDF, confirm immediate `Queued` response and eventual final state, confirm a static `/uploads/documents/...` URL returns 404, and verify `/health/live` plus `/health/ready` return healthy.

- [ ] **Step 7: Commit**

```powershell
git add .github .gitignore README.md scripts PRN212-VietnameseEduChat
git commit -m "chore: add CI and repository hardening docs"
```

---

## Final review gate

- [ ] Re-read `docs/superpowers/specs/2026-07-17-project-hardening-design.md` and map every goal to a completed task.
- [ ] Run `git diff codex/full-ui-ux-redesign...HEAD --check`.
- [ ] Review `git diff --stat codex/full-ui-ux-redesign...HEAD` and confirm no unrelated UI/domain changes.
- [ ] Run the complete build, test, static verification, secret scan, and browser smoke suite again with fresh output.
- [ ] Push `codex/project-hardening` only after the user explicitly requests the external action.

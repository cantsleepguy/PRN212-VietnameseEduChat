# Project Hardening Design

## Context

VietnameseEduChat is a .NET 8 Razor Pages course project with cookie authentication, role-based pages, document ingestion, RAG chat, and VNPay sandbox payments. The goal is to add meaningful engineering contributions without introducing infrastructure that makes the classroom demo difficult to run.

This work builds on `codex/full-ui-ux-redesign` and lives on the stacked branch `codex/project-hardening`. It will remain a separate review unit from the UI redesign.

## Goals

- Add automated coverage for critical authentication, authorization, upload, payment, health, and background-processing behavior.
- Keep uploaded documents outside the public static-file tree and serve them through an authorized endpoint.
- Validate file size, extension, MIME type, and file signature before accepting a document.
- Move OCR, chunking, and embedding out of the upload request into a recoverable in-process queue.
- Ensure concurrent VNPay callbacks activate a subscription at most once.
- Make migrations and demo data explicit configuration choices.
- Add liveness/readiness endpoints, CI, safe configuration documentation, and repository hygiene.

## Non-goals

- Redis, RabbitMQ, Hangfire, cloud queues, or multi-instance job coordination.
- Docker or a production deployment pipeline.
- Replacing SQL Server, VNPay, OpenAI, OCR, or the existing repository/service architecture.
- Redesigning the user interface or changing business roles.

## Architecture

### Test foundation

Create an xUnit test project referenced by the solution. Unit tests exercise isolated validation and payment rules. Integration tests use a test host with test configuration and a disposable database provider appropriate to each boundary. Tests must not call OpenAI, VNPay, or the local embedding server.

The first coverage targets are authentication and role restrictions, upload validation, document queue transitions, duplicate VNPay callbacks, health endpoints, and startup configuration. New behavior follows red-green-refactor.

### Private document storage

Introduce document-storage options with a default development location under the application's content root, outside `wwwroot`. A storage service owns safe path creation, writes, opens, and deletion. Stored names remain GUID-based and callers never combine user-controlled path segments.

Validation accepts PDF, DOCX, and PPTX up to 25 MB. It checks the normalized extension, an allowlisted MIME type, and the format's leading signature. Office Open XML files share the ZIP signature and retain extension-specific extraction checks. Validation failures occur before a database row or file is created.

An authenticated download handler resolves the document by ID, evaluates role and subject ownership/assignment rules, and returns a stream with the original download name. Static middleware never serves uploaded documents.

Tracked sample uploads are removed and the runtime upload directory is ignored. The existing downloadable research CSV remains tracked because it is an intentional application asset.

### Background document processing

`IDocumentProcessingQueue` exposes enqueue and dequeue operations backed by a bounded `Channel<int>`. Upload performs validation, saves the file and document metadata, sets status to `Queued`, and enqueues the document ID. It then returns immediately.

`DocumentProcessingWorker`, implemented as a `BackgroundService`, consumes IDs and invokes a focused processor that performs text extraction, chunking, embedding, and state updates. Success moves `Processing` to `PendingApproval`; failure moves it to `Failed`, stores a user-safe message, and logs technical details server-side.

At startup, a recovery service finds `Queued` and interrupted `Processing` documents, resets interrupted items to `Queued`, and enqueues them. This provides restart recovery for a single-instance classroom deployment without persistent queue infrastructure. Duplicate queue entries are harmless because the processor claims only eligible states.

### VNPay idempotency

Add an EF Core concurrency token to `Payment`. Callback handling reloads and claims a pending payment inside a database transaction. Only the request that successfully transitions the row may apply the subscription. A concurrency conflict or an already-final state returns VNPay's already-confirmed response without applying benefits again.

The successful payment path, payment state transition, and subscription mutation remain in the same transaction. Signature and amount checks still happen before fulfillment.

### Startup and health

Add startup options:

- `Database:AutoMigrate`, default `true` only in Development configuration.
- `DemoData:Enabled`, default `true` only in Development configuration.
- `DocumentStorage:RootPath`, optional; a safe content-root location is used when absent.

Production does not silently migrate or create known demo credentials. Development retains a one-command classroom setup when explicitly configured.

Replace the hard-coded Ghostscript path with optional configuration plus platform-safe discovery. Missing Ghostscript must not prevent application startup; OCR-dependent operations report a controlled failure.

Expose `/health/live` for process liveness and `/health/ready` for database readiness. External AI services are represented by configuration diagnostics rather than network calls so the demo remains deterministic and health checks do not spend API quota.

### CI and documentation

Add a GitHub Actions workflow for pushes and pull requests that installs .NET 8, restores, builds with warnings treated normally, and runs all tests. No secrets are required.

Rewrite affected README content as valid UTF-8 Vietnamese. Document local configuration through user-secrets or environment variables, migration/seed switches, test commands, health endpoints, private upload storage, and background processing. Never include real credentials.

## Authorization rules for downloads

- System Admin and Academic Admin may download any document they can administer.
- A Lecturer may download documents they uploaded or documents belonging to an assigned subject.
- A Student may download only approved documents in subjects available to the student under the existing application rules.
- Unauthenticated requests are redirected to login; authenticated but unauthorized requests receive forbidden/not-found behavior without exposing storage paths.

If the current domain model lacks an explicit student-subject enrollment relationship, student access is limited to approved documents already visible through the existing document query rules rather than inventing a new enrollment feature.

## Error handling and observability

- User-facing errors are stable Vietnamese messages and do not include exception text, filesystem paths, API responses, keys, or document contents.
- Logs use structured identifiers such as document ID, payment ID, and transaction ID.
- File writes clean up partial files when validation or persistence fails.
- Database failure after a file write triggers compensating file deletion.
- Background failures preserve the source file so a lecturer can retry indexing.

## Verification

Completion requires:

- Solution restore, build, and test succeed from a clean checkout.
- Tests cover the new validation, queue recovery, authorization, health, startup-option, and VNPay concurrency behavior.
- Desktop role smoke tests still pass for Student, Lecturer, Academic Admin, and System Admin.
- A valid document upload returns promptly, advances through `Queued` and `Processing`, and ends in `PendingApproval` or a controlled `Failed` state.
- Uploaded files cannot be retrieved as static URLs.
- Concurrent successful VNPay callbacks result in one successful payment and one subscription application.
- Git tracks no runtime uploads, local settings, API keys, payment secrets, or generated databases.

## Delivery sequence

1. Test foundation and reusable test fixtures.
2. Private storage and validated upload/download.
3. In-process processing queue, worker, and recovery.
4. VNPay concurrency protection.
5. Startup options, Ghostscript configuration, and health endpoints.
6. Repository cleanup, GitHub Actions, README, and full verification.


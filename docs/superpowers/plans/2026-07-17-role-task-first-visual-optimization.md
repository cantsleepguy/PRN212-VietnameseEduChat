# Role Task-First Visual Optimization Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Tối ưu hình ảnh và bố cục VietnameseEduChat theo hướng task-first cho Student, Lecturer, AcademicAdmin và SystemAdmin mà không thay đổi nghiệp vụ hoặc quyền truy cập.

**Architecture:** Giữ ASP.NET Core Razor Pages và design system CSS hiện có. Mở rộng semantic tokens/component primitives, sau đó áp dụng cùng một page hierarchy cho app shell và từng module; dashboard thay đổi thứ tự ưu tiên theo role bằng Razor authorization hiện có. Verification kết hợp static UI audit, `dotnet build` và browser smoke test theo role/breakpoint.

**Tech Stack:** ASP.NET Core 8 Razor Pages, Bootstrap 5, CSS custom properties, vanilla JavaScript, SVG sprite, PowerShell verification, browser automation.

## Global Constraints

- Chỉ thay đổi hình ảnh, bố cục và accessibility presentation; không thay đổi nghiệp vụ.
- Không thay đổi service, repository, entity, database schema, endpoint, authorization policy hoặc role mapping.
- Không thay đổi luồng RAG, streaming, embedding hoặc thanh toán.
- Không thêm framework frontend hoặc thư viện nghiệp vụ mới.
- Không tạo metric, nội dung hoặc dữ liệu giả để lấp giao diện.
- Giữ bảng màu học thuật: nền sáng, sidebar navy, teal cho CTA chính.
- Dùng icon SVG; không dùng emoji làm icon giao diện.
- Kiểm tra ở 320px, 768px, 1024px và 1440px.
- Contrast, focus, heading và accessible name phải đáp ứng WCAG 2.1 AA.

---

## File Responsibility Map

- `PRN212-VietnameseEduChat/wwwroot/css/site.css`: tokens, app shell, shared primitives, focus, responsive navigation.
- `PRN212-VietnameseEduChat/wwwroot/css/pages.css`: page-specific task-first layouts, tables, forms, chat and role dashboards.
- `PRN212-VietnameseEduChat/wwwroot/js/site.js`: mobile drawer and presentation-only progressive enhancement.
- `PRN212-VietnameseEduChat/Pages/Shared/_Layout.cshtml`: role-aware navigation, landmarks and shared shell.
- `PRN212-VietnameseEduChat/Pages/Shared/_Icon.cshtml`: shared SVG icon rendering.
- `PRN212-VietnameseEduChat/Pages/Dashboard.cshtml`: task-first dashboard composition for all four roles.
- `PRN212-VietnameseEduChat/Pages/Chat/Index.cshtml`: conversation workspace hierarchy.
- `PRN212-VietnameseEduChat/Pages/Documents/*.cshtml`: document list, upload and details presentation.
- `PRN212-VietnameseEduChat/Pages/Subjects/Index.cshtml`: subject/chapter presentation.
- `PRN212-VietnameseEduChat/Pages/ResearchQuestions/Index.cshtml`: research question toolbar and data presentation.
- `PRN212-VietnameseEduChat/Pages/ResearchExperiments/Index.cshtml`: experiment status, metrics and results presentation.
- `PRN212-VietnameseEduChat/Pages/Packages/*.cshtml`: pricing, checkout, history and payment result presentation.
- `PRN212-VietnameseEduChat/Pages/Admin/**/*.cshtml`: administration dashboard, tables, forms and import presentation.
- `scripts/verify-role-ui.ps1`: repeatable static checks for role UI structure and forbidden visual patterns.

---

### Task 1: Add Repeatable UI Structure Verification

**Files:**
- Create: `scripts/verify-role-ui.ps1`
- Test: `scripts/verify-role-ui.ps1`

**Interfaces:**
- Consumes: Razor pages under `PRN212-VietnameseEduChat/Pages` and styles under `PRN212-VietnameseEduChat/wwwroot/css`.
- Produces: PowerShell command that exits `0` when required UI markers exist and exits `1` with actionable failures otherwise.

- [ ] **Step 1: Write the failing static verification script**

```powershell
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$layout = Get-Content -Raw "$root/PRN212-VietnameseEduChat/Pages/Shared/_Layout.cshtml"
$dashboard = Get-Content -Raw "$root/PRN212-VietnameseEduChat/Pages/Dashboard.cshtml"
$siteCss = Get-Content -Raw "$root/PRN212-VietnameseEduChat/wwwroot/css/site.css"
$pagesCss = Get-Content -Raw "$root/PRN212-VietnameseEduChat/wwwroot/css/pages.css"

$checks = [ordered]@{
    'Skip link exists' = $layout.Contains('class="skip-link"')
    'Navigation has accessible label' = $layout.Contains('aria-label="Điều hướng chính"')
    'Student task region exists' = $dashboard.Contains('data-role-dashboard="student"')
    'Lecturer task region exists' = $dashboard.Contains('data-role-dashboard="lecturer"')
    'Academic admin task region exists' = $dashboard.Contains('data-role-dashboard="academic-admin"')
    'System admin task region exists' = $dashboard.Contains('data-role-dashboard="system-admin"')
    'Task grid styles exist' = $pagesCss.Contains('.task-action-grid')
    'Reduced motion is respected' = $siteCss.Contains('prefers-reduced-motion')
    'No emoji icon markers remain' = -not (($layout + $dashboard) -match '[😀-🙏]')
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value })
if ($failed.Count -gt 0) {
    $failed | ForEach-Object { Write-Error "UI check failed: $($_.Key)" }
    exit 1
}

Write-Host "Role UI verification passed: $($checks.Count) checks."
```

- [ ] **Step 2: Run the script and verify it fails before role markers are added**

Run: `powershell -ExecutionPolicy Bypass -File scripts/verify-role-ui.ps1`

Expected: exit `1` with at least `Student task region exists` or `Task grid styles exist`.

- [ ] **Step 3: Commit the verification harness**

```powershell
git add scripts/verify-role-ui.ps1
git commit -m "test: add role UI structure verification"
```

---

### Task 2: Refine Design Tokens and Shared Visual Primitives

**Files:**
- Modify: `PRN212-VietnameseEduChat/wwwroot/css/site.css`
- Modify: `PRN212-VietnameseEduChat/wwwroot/css/pages.css`
- Modify: `PRN212-VietnameseEduChat/Pages/Shared/_Icon.cshtml`
- Test: `scripts/verify-role-ui.ps1`

**Interfaces:**
- Consumes: existing semantic CSS variables and local SVG sprite.
- Produces: `.content-section`, `.task-action-grid`, `.task-action`, `.page-toolbar`, `.data-surface`, `.empty-state` and consistent focus/motion behavior for later tasks.

- [ ] **Step 1: Add semantic density and hierarchy tokens to `site.css`**

```css
:root {
    --space-1: .25rem;
    --space-2: .5rem;
    --space-3: .75rem;
    --space-4: 1rem;
    --space-6: 1.5rem;
    --space-8: 2rem;
    --content-reading: 72ch;
    --content-wide: 90rem;
    --radius-control: .5rem;
    --radius-surface: .75rem;
    --shadow-raised: 0 .5rem 1.5rem rgba(16, 42, 67, .08);
}

:focus-visible {
    outline: 3px solid var(--color-focus);
    outline-offset: 3px;
}

@media (prefers-reduced-motion: reduce) {
    *, *::before, *::after {
        scroll-behavior: auto !important;
        transition-duration: .01ms !important;
        animation-duration: .01ms !important;
        animation-iteration-count: 1 !important;
    }
}
```

- [ ] **Step 2: Add shared task-first primitives to `pages.css`**

```css
.content-section {
    padding-block: var(--space-6);
    border-top: 1px solid var(--color-border);
}

.task-action-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(min(100%, 15rem), 1fr));
    gap: var(--space-3);
}

.task-action {
    display: grid;
    grid-template-columns: auto 1fr auto;
    align-items: center;
    gap: var(--space-3);
    min-height: 4.5rem;
    padding: var(--space-4);
    color: var(--color-text);
    background: var(--color-surface);
    border: 1px solid var(--color-border);
    border-radius: var(--radius-surface);
    text-decoration: none;
}

.task-action:hover {
    border-color: var(--color-primary);
    box-shadow: var(--shadow-raised);
    transform: translateY(-1px);
}

.page-toolbar {
    display: flex;
    flex-wrap: wrap;
    align-items: end;
    justify-content: space-between;
    gap: var(--space-3);
}
```

- [ ] **Step 3: Ensure `_Icon.cshtml` exposes decorative and labelled modes**

```cshtml
<svg class="app-icon @Model.CssClass"
     aria-hidden="@(string.IsNullOrWhiteSpace(Model.Label) ? "true" : "false")"
     aria-label="@(string.IsNullOrWhiteSpace(Model.Label) ? null : Model.Label)"
     role="@(string.IsNullOrWhiteSpace(Model.Label) ? null : "img")"
     focusable="false">
    <use href="/icons/app-icons.svg#@Model.Name"></use>
</svg>
```

- [ ] **Step 4: Run static verification and build**

Run:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/verify-role-ui.ps1
dotnet build PRN212-VietnameseEduChat.sln --no-restore --nologo
```

Expected: static verification may still fail only on dashboard role markers; build succeeds with `0 Error(s)`.

- [ ] **Step 5: Commit shared primitives**

```powershell
git add PRN212-VietnameseEduChat/wwwroot/css/site.css PRN212-VietnameseEduChat/wwwroot/css/pages.css PRN212-VietnameseEduChat/Pages/Shared/_Icon.cshtml
git commit -m "feat: refine task-first UI primitives"
```

---

### Task 3: Optimize the Role-Aware App Shell

**Files:**
- Modify: `PRN212-VietnameseEduChat/Pages/Shared/_Layout.cshtml`
- Modify: `PRN212-VietnameseEduChat/wwwroot/css/site.css`
- Modify: `PRN212-VietnameseEduChat/wwwroot/js/site.js`
- Test: `scripts/verify-role-ui.ps1`

**Interfaces:**
- Consumes: existing `AppRoles`, `Active()` page helper and Bootstrap assets.
- Produces: accessible role-grouped navigation, skip link and mobile drawer behavior used by every authenticated page.

- [ ] **Step 1: Add skip navigation and label the primary navigation**

```cshtml
<a class="skip-link" href="#main-content">Bỏ qua điều hướng</a>
<nav class="sidebar-nav" aria-label="Điều hướng chính">
    @* Preserve every existing role check and asp-page target. *@
</nav>
<main id="main-content" class="app-main" tabindex="-1">
    @RenderBody()
</main>
```

- [ ] **Step 2: Group existing links without changing authorization conditions**

```cshtml
<div class="nav-group" aria-labelledby="nav-learning-title">
    <p class="nav-group-title" id="nav-learning-title">Học tập</p>
    <a class="sidebar-link @Active("/Dashboard")" asp-page="/Dashboard">
        <partial name="_Icon" model='@(ValueTuple.Create<string, string?, string?>("home", null, null))' />
        <span>Tổng quan</span>
    </a>
    <a class="sidebar-link @Active("/Chat")" asp-page="/Chat/Index">
        <partial name="_Icon" model='@(ValueTuple.Create<string, string?, string?>("chat", null, null))' />
        <span>Hỏi đáp với AI</span>
    </a>
</div>
```

Use `User.IsInRole(PRN212_VietnameseEduChat.Services.Security.AppRoles.AcademicAdmin)` for the research group and the existing AcademicAdmin/SystemAdmin OR condition for the administration group. Copy every existing `asp-page` target without renaming it.

- [ ] **Step 3: Make mobile drawer state accessible in `site.js`**

```javascript
const menuButton = document.querySelector('[data-sidebar-toggle]');
const sidebar = document.querySelector('[data-sidebar]');
const backdrop = document.querySelector('[data-sidebar-backdrop]');

function setSidebar(open) {
  document.body.classList.toggle('sidebar-open', open);
  menuButton?.setAttribute('aria-expanded', String(open));
  sidebar?.setAttribute('aria-hidden', String(!open));
  if (open) sidebar?.querySelector('a, button')?.focus();
  else menuButton?.focus();
}

menuButton?.addEventListener('click', () => {
  setSidebar(menuButton.getAttribute('aria-expanded') !== 'true');
});
backdrop?.addEventListener('click', () => setSidebar(false));
document.addEventListener('keydown', event => {
  if (event.key === 'Escape' && document.body.classList.contains('sidebar-open')) setSidebar(false);
});
```

- [ ] **Step 4: Verify shell structure and build**

Run:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/verify-role-ui.ps1
dotnet build PRN212-VietnameseEduChat.sln --no-restore --nologo
```

Expected: skip-link and navigation checks pass; build succeeds with `0 Error(s)`.

- [ ] **Step 5: Commit the app shell**

```powershell
git add PRN212-VietnameseEduChat/Pages/Shared/_Layout.cshtml PRN212-VietnameseEduChat/wwwroot/css/site.css PRN212-VietnameseEduChat/wwwroot/js/site.js
git commit -m "feat: optimize role-aware app shell"
```

---

### Task 4: Build Task-First Dashboards for All Roles

**Files:**
- Modify: `PRN212-VietnameseEduChat/Pages/Dashboard.cshtml`
- Modify: `PRN212-VietnameseEduChat/wwwroot/css/pages.css`
- Test: `scripts/verify-role-ui.ps1`

**Interfaces:**
- Consumes: `User.IsInRole(AppRoles.*)`, `Model.ManagedSubjects` and existing page routes.
- Produces: four role-specific regions identified by `data-role-dashboard` and shared `.task-action-grid` markup.

- [ ] **Step 1: Add the Student task-first region**

```cshtml
@if (User.IsInRole(AppRoles.Student))
{
    <section data-role-dashboard="student" aria-labelledby="student-actions-title">
        <div class="section-heading">
            <div><p class="eyebrow">Bắt đầu nhanh</p><h2 id="student-actions-title">Bạn muốn học gì hôm nay?</h2></div>
        </div>
        <div class="task-action-grid">
            <a class="task-action task-action-primary" asp-page="/Chat/Index"><span>Hỏi đáp với AI</span><span aria-hidden="true">→</span></a>
            <a class="task-action" asp-page="/Packages/Index"><span>Xem gói dịch vụ</span><span aria-hidden="true">→</span></a>
            <a class="task-action" asp-page="/Packages/History"><span>Lịch sử thanh toán</span><span aria-hidden="true">→</span></a>
        </div>
    </section>
}
```

- [ ] **Step 2: Add the Lecturer task-first region before managed subjects**

```cshtml
@if (User.IsInRole(AppRoles.Lecturer))
{
    <section data-role-dashboard="lecturer" aria-labelledby="lecturer-actions-title">
        <h2 id="lecturer-actions-title">Tác vụ giảng dạy</h2>
        <div class="task-action-grid">
            <a class="task-action task-action-primary" asp-page="/Documents/Upload"><span>Tải lên tài liệu</span><span aria-hidden="true">→</span></a>
            <a class="task-action" asp-page="/Documents/Index"><span>Quản lý tài liệu</span><span aria-hidden="true">→</span></a>
            <a class="task-action" asp-page="/Chat/Index"><span>Mở hỏi đáp AI</span><span aria-hidden="true">→</span></a>
        </div>
    </section>
}
```

Preserve the existing `Model.ManagedSubjects` loop and move it into a lower `content-section` without altering its values or links.

- [ ] **Step 3: Add AcademicAdmin and SystemAdmin regions using existing routes**

```cshtml
@if (User.IsInRole(AppRoles.AcademicAdmin))
{
    <section data-role-dashboard="academic-admin" aria-labelledby="academic-actions-title">
        <h2 id="academic-actions-title">Tác vụ học thuật</h2>
        <div class="task-action-grid">
            <a class="task-action task-action-primary" asp-page="/Subjects/Index"><span>Quản lý môn học</span><span aria-hidden="true">→</span></a>
            <a class="task-action" asp-page="/ResearchQuestions/Index"><span>Ngân hàng câu hỏi</span><span aria-hidden="true">→</span></a>
            <a class="task-action" asp-page="/ResearchExperiments/Index"><span>Thí nghiệm RBL</span><span aria-hidden="true">→</span></a>
            <a class="task-action" asp-page="/Admin/ChunkingConfig"><span>Cấu hình chunking</span><span aria-hidden="true">→</span></a>
        </div>
    </section>
}
@if (User.IsInRole(AppRoles.SystemAdmin))
{
    <section data-role-dashboard="system-admin" aria-labelledby="system-actions-title">
        <h2 id="system-actions-title">Tác vụ hệ thống</h2>
        <div class="task-action-grid">
            <a class="task-action task-action-primary" asp-page="/Admin/Users/Index"><span>Quản lý tài khoản</span><span aria-hidden="true">→</span></a>
            <a class="task-action" asp-page="/Admin/Packages"><span>Quản lý gói</span><span aria-hidden="true">→</span></a>
            <a class="task-action" asp-page="/Admin/Payments"><span>Theo dõi thanh toán</span><span aria-hidden="true">→</span></a>
            <a class="task-action" asp-page="/Admin/Dashboard"><span>Dashboard quản trị</span><span aria-hidden="true">→</span></a>
        </div>
    </section>
}
```

Academic actions must link to `/Subjects/Index`, `/ResearchQuestions/Index`, `/ResearchExperiments/Index` and `/Admin/ChunkingConfig`. System actions must link to `/Admin/Users/Index`, `/Admin/Packages`, `/Admin/Payments` and `/Admin/Dashboard`.

- [ ] **Step 4: Run the verification harness and build**

Run:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/verify-role-ui.ps1
dotnet build PRN212-VietnameseEduChat.sln --no-restore --nologo
```

Expected: `Role UI verification passed` and build succeeds with `0 Error(s)`.

- [ ] **Step 5: Commit role dashboards**

```powershell
git add PRN212-VietnameseEduChat/Pages/Dashboard.cshtml PRN212-VietnameseEduChat/wwwroot/css/pages.css
git commit -m "feat: prioritize dashboard tasks by role"
```

---

### Task 5: Optimize Learning Workspaces

**Files:**
- Modify: `PRN212-VietnameseEduChat/Pages/Chat/Index.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Documents/Index.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Documents/Upload.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Documents/Details.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Subjects/Index.cshtml`
- Modify: `PRN212-VietnameseEduChat/wwwroot/css/pages.css`

**Interfaces:**
- Consumes: all existing form names, tag helpers, element IDs used by Chat JavaScript and Razor model properties.
- Produces: consistent page headers, compact toolbars, content sections and mobile-safe learning layouts without changing form submission or SignalR behavior.

- [ ] **Step 1: Restructure Chat visually while preserving IDs and event targets**

```cshtml
<section class="chat-workspace" aria-label="Không gian hỏi đáp AI">
    <aside class="chat-history-panel" aria-label="Lịch sử hội thoại">
        <h2 class="chat-panel-title">Lịch sử hội thoại</h2>
        <button id="newConversationButton" type="button" class="btn btn-primary">Cuộc trò chuyện mới</button>
        <div id="conversationList" class="conversation-list"></div>
    </aside>
    <div class="chat-stage">
        <header class="chat-stage-header"><h1>Hỏi đáp với AI</h1></header>
        <div id="messages" class="chat-message-list" aria-live="polite"></div>
        <form id="chatForm" class="chat-composer">
            <label class="visually-hidden" for="messageInput">Câu hỏi của bạn</label>
            <textarea id="messageInput" name="message" rows="1" placeholder="Nhập câu hỏi"></textarea>
            <button id="sendButton" type="submit" class="btn btn-primary">Gửi câu hỏi</button>
        </form>
    </div>
</section>
```

Copy all existing IDs, `data-*` attributes, form fields and script blocks unchanged into the new regions.

- [ ] **Step 2: Normalize Documents list toolbar and data surface**

```cshtml
<header class="page-header">
    <div class="page-header__copy"><p class="page-eyebrow">Kho học liệu</p><h1>Danh sách tài liệu</h1></div>
    <a class="btn btn-primary" asp-page="/Documents/Upload">Tải lên tài liệu</a>
</header>
<section class="page-toolbar" aria-label="Công cụ tài liệu">
    <label for="documentSearch">Tìm tài liệu</label>
    <input id="documentSearch" class="form-control" type="search" placeholder="Nhập tên tài liệu" />
</section>
<section class="data-surface" aria-label="Danh sách tài liệu">
    <div class="table-scroll"><table class="table document-table"><thead><tr><th>Tài liệu</th><th>Trạng thái</th><th>Thao tác</th></tr></thead><tbody></tbody></table></div>
</section>
```

Keep every existing action URL, status value and authorization condition unchanged.

- [ ] **Step 3: Group Upload and Details content into logical sections**

```cshtml
<form method="post" enctype="multipart/form-data" class="form-stack">
    <section class="form-section" aria-labelledby="document-file-title">
        <h2 id="document-file-title">Chọn tài liệu</h2>
        <input asp-for="UploadFile" class="form-control" type="file" accept=".pdf,.docx,.pptx" />
        <span asp-validation-for="UploadFile" class="text-danger"></span>
    </section>
    <section class="form-section" aria-labelledby="document-context-title">
        <h2 id="document-context-title">Thông tin phân loại</h2>
        <label asp-for="SubjectId" class="form-label"></label>
        <select asp-for="SubjectId" class="form-select">
            <option value="">-- Chọn môn học --</option>
            @foreach (var subject in Model.Subjects) { <option value="@subject.SubjectId">@subject.SubjectName</option> }
        </select>
    </section>
    <div class="form-actions"><button type="submit" class="btn btn-primary">Tải lên</button><a asp-page="/Documents/Index" class="btn btn-outline-secondary">Hủy</a></div>
</form>
```

Reuse existing `asp-for`, `asp-validation-for` and submit values exactly.

- [ ] **Step 4: Separate subject actions, chapters and assigned lecturers visually**

```cshtml
<article class="subject-workspace">
    <header class="subject-summary"><p class="page-eyebrow">Môn học</p><h2>@subject.SubjectName</h2><p>@subject.Description</p></header>
    <section class="content-section" aria-labelledby="chapters-title"><h3 id="chapters-title">Chương học</h3><div class="chapter-list"></div></section>
    <section class="content-section" aria-labelledby="lecturers-title"><h3 id="lecturers-title">Giảng viên phụ trách</h3><div class="lecturer-list"></div></section>
</article>
```

- [ ] **Step 5: Build and smoke-test the learning pages**

Run:

```powershell
dotnet build PRN212-VietnameseEduChat.sln --no-restore --nologo
powershell -ExecutionPolicy Bypass -File scripts/verify-role-ui.ps1
```

Expected: build succeeds with `0 Error(s)` and role UI verification passes.

- [ ] **Step 6: Commit learning workspace changes**

```powershell
git add PRN212-VietnameseEduChat/Pages/Chat PRN212-VietnameseEduChat/Pages/Documents PRN212-VietnameseEduChat/Pages/Subjects/Index.cshtml PRN212-VietnameseEduChat/wwwroot/css/pages.css
git commit -m "feat: optimize learning workspace layouts"
```

---

### Task 6: Optimize Research, Packages and Administration

**Files:**
- Modify: `PRN212-VietnameseEduChat/Pages/ResearchQuestions/Index.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/ResearchExperiments/Index.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Packages/Index.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Packages/Checkout.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Packages/History.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Packages/VnPayReturn.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Admin/Dashboard.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Admin/ChunkingConfig.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Admin/Packages.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Admin/Payments.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Admin/Users/Index.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Admin/Users/Import.cshtml`
- Modify: `PRN212-VietnameseEduChat/wwwroot/css/pages.css`

**Interfaces:**
- Consumes: existing Razor handlers, Chart.js canvas IDs, form values, table data and authorization.
- Produces: consistent data-dense page hierarchy using page header, toolbar, status badge, data surface and form section patterns.

- [ ] **Step 1: Apply a consistent research toolbar and data hierarchy**

```cshtml
<header class="page-header"><div class="page-header__copy"><p class="page-eyebrow">RBL Benchmark</p><h1>Câu hỏi nghiên cứu</h1></div><button type="button" class="btn btn-primary">Thêm câu hỏi</button></header>
<section class="page-toolbar" aria-label="Công cụ nghiên cứu"><label for="researchSearch">Tìm câu hỏi</label><input id="researchSearch" type="search" class="form-control" /></section>
<section class="data-surface research-data"><div class="table-scroll"><table class="table"><thead><tr><th>Câu hỏi</th><th>Môn học</th><th>Trạng thái</th><th>Thao tác</th></tr></thead><tbody></tbody></table></div></section>
```

Preserve experiment buttons, question import/export handlers, canvas IDs and Chart.js initialization code.

- [ ] **Step 2: Clarify package selection and checkout hierarchy**

```cshtml
<article class="pricing-option">
    <header class="pricing-option-header"><p class="page-eyebrow">Gói học tập</p><h2>@package.PackageName</h2></header>
    <div class="pricing-value"><strong>@package.Price.ToString("N0")</strong><span> VNĐ</span></div>
    <ul class="pricing-features" role="list"><li>@package.DurationDays ngày sử dụng</li><li>Tối đa @package.MaxDocuments tài liệu</li><li>@package.DailyQuestionLimit câu hỏi mỗi ngày</li></ul>
    <div class="pricing-action"><a asp-page="/Packages/Checkout" asp-route-packageId="@package.PackageId" class="btn btn-primary">Chọn gói</a></div>
</article>
```

Use one CTA per package and keep the existing package ID and payment form fields unchanged.

- [ ] **Step 3: Normalize administration pages**

```cshtml
<header class="page-header"><div class="page-header__copy"><p class="page-eyebrow">Quản trị hệ thống</p><h1>Quản lý tài khoản</h1></div><a asp-page="/Admin/Users/Import" class="btn btn-primary">Nhập tài khoản</a></header>
<section class="page-toolbar" aria-label="Công cụ quản trị"><label for="adminSearch">Tìm kiếm</label><input id="adminSearch" name="q" type="search" class="form-control" /></section>
<section class="data-surface"><div class="table-scroll"><table class="table"><thead><tr><th>Tài khoản</th><th>Vai trò</th><th>Trạng thái</th><th>Thao tác</th></tr></thead><tbody></tbody></table></div></section>
```

Use the same placement for search/filter, primary create/import action, status and row actions across users, packages and payments.

- [ ] **Step 4: Make destructive actions visually explicit without changing handlers**

```cshtml
<button type="submit" class="btn btn-danger btn-sm">
    <partial name="_Icon" model='new IconModel("trash", null)' />
    <span>Xóa</span>
</button>
```

Retain existing `onclick`, modal confirmation, handler name and anti-forgery behavior.

- [ ] **Step 5: Build and run static verification**

Run:

```powershell
dotnet build PRN212-VietnameseEduChat.sln --no-restore --nologo
powershell -ExecutionPolicy Bypass -File scripts/verify-role-ui.ps1
```

Expected: build succeeds with `0 Error(s)` and static verification passes.

- [ ] **Step 6: Commit data-dense modules**

```powershell
git add PRN212-VietnameseEduChat/Pages/ResearchQuestions PRN212-VietnameseEduChat/Pages/ResearchExperiments PRN212-VietnameseEduChat/Pages/Packages PRN212-VietnameseEduChat/Pages/Admin PRN212-VietnameseEduChat/wwwroot/css/pages.css
git commit -m "feat: optimize research commerce and admin layouts"
```

---

### Task 7: Responsive, Accessibility and Browser Verification

**Files:**
- Modify: `PRN212-VietnameseEduChat/wwwroot/css/site.css`
- Modify: `PRN212-VietnameseEduChat/wwwroot/css/pages.css`
- Modify: Razor files identified by browser findings only when the fix is presentation-only.
- Test: `scripts/verify-role-ui.ps1`

**Interfaces:**
- Consumes: completed UI from Tasks 2–6 and the four seeded role accounts already present in the development database.
- Produces: verified presentation at four breakpoints with clean build, no horizontal overflow, no asset failures and accessible primary navigation/actions.

- [ ] **Step 1: Add mobile-first layout constraints**

```css
@media (max-width: 767.98px) {
    .page-header,
    .page-toolbar,
    .form-actions {
        align-items: stretch;
        flex-direction: column;
    }

    .task-action-grid,
    .dashboard-grid,
    .pricing-grid {
        grid-template-columns: 1fr;
    }

    .table-scroll {
        overflow-x: auto;
        overscroll-behavior-inline: contain;
    }
}
```

- [ ] **Step 2: Run final static verification and clean build**

Run:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/verify-role-ui.ps1
dotnet build PRN212-VietnameseEduChat.sln --no-restore --nologo
git diff --check
```

Expected: verification passes, build reports `0 Warning(s)` and `0 Error(s)`, and `git diff --check` prints nothing.

- [ ] **Step 3: Start the feature worktree server**

Run:

```powershell
dotnet run --project PRN212-VietnameseEduChat/PRN212-VietnameseEduChat.csproj --no-build --urls http://localhost:5234
```

Expected: log contains `Now listening on: http://localhost:5234`.

- [ ] **Step 4: Verify each role at 1440px and 320px**

For each seeded Student, Lecturer, AcademicAdmin and SystemAdmin account:

1. Open `/Login` and authenticate.
2. Confirm the expected `data-role-dashboard` region is visible.
3. Confirm sidebar links match existing authorization.
4. Confirm the first task region appears before summary content.
5. At widths 1440 and 320, evaluate:

```javascript
({
  path: location.pathname,
  width: innerWidth,
  horizontalOverflow: document.documentElement.scrollWidth > innerWidth,
  headings: [...document.querySelectorAll('h1')].map(x => x.textContent.trim()),
  visibleTaskRegions: [...document.querySelectorAll('[data-role-dashboard]')]
    .filter(x => x.getClientRects().length > 0)
    .map(x => x.dataset.roleDashboard)
})
```

Expected: `horizontalOverflow` is `false`, exactly one `h1` is present and exactly one role region is visible.

- [ ] **Step 5: Verify representative module pages at 768px, 1024px and 1440px**

Check `/Chat`, `/Documents`, `/Subjects`, `/ResearchQuestions`, `/ResearchExperiments`, `/Packages`, `/Admin/Users`, `/Admin/Packages` and `/Admin/Payments` when authorized. Confirm:

- No console errors or failed CSS/JS/SVG responses.
- Primary action is visible without overlapping the title.
- Table overflow stays inside `.table-scroll`.
- Mobile drawer opens, updates `aria-expanded`, closes with Escape and restores focus.
- Tab navigation reaches every visible button/link with a visible focus indicator.

- [ ] **Step 6: Review the final diff for scope**

Run:

```powershell
git status --short
git diff --stat HEAD~6..HEAD
git diff --name-only HEAD~6..HEAD
```

Expected: only Razor, CSS, presentation JavaScript, SVG/shared UI files, verification script and documentation appear; no service, repository, entity or database files appear.

- [ ] **Step 7: Commit responsive and accessibility polish**

```powershell
git add PRN212-VietnameseEduChat/Pages PRN212-VietnameseEduChat/wwwroot/css PRN212-VietnameseEduChat/wwwroot/js/site.js scripts/verify-role-ui.ps1
git commit -m "fix: polish responsive role experiences"
```

---

## Final Acceptance Checklist

- [ ] Static role UI verification passes.
- [ ] Solution builds with `0 Warning(s)` and `0 Error(s)`.
- [ ] Student, Lecturer, AcademicAdmin and SystemAdmin each see one correct task-first dashboard.
- [ ] No business, service, repository, entity, database or authorization files changed.
- [ ] No console errors, asset 404s or unintended horizontal overflow.
- [ ] Login, Dashboard, Chat and representative module pages work at 320px, 768px, 1024px and 1440px.
- [ ] Keyboard focus, navigation drawer and icon accessible names are verified.
- [ ] Git worktree is clean after the final commit.

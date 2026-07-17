# VietnameseEduChat Full UI/UX Redesign Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Redesign every Razor Page in VietnameseEduChat with the approved light academic navy–teal design system while preserving all existing business behavior and role permissions.

**Architecture:** Introduce a token-driven CSS design system and a responsive Razor app shell, then migrate pages by domain so each increment remains buildable and reviewable. Reuse Bootstrap behavior, local SVG icons, existing Razor handlers, SignalR, Marked and Chart.js; visual changes stay in markup, CSS and small progressive-enhancement JavaScript.

**Tech Stack:** ASP.NET Core 8 Razor Pages, Bootstrap 5, CSS custom properties, vanilla JavaScript, jQuery validation, SignalR, Marked, Chart.js, local SVG sprite.

## Global Constraints

- Keep ASP.NET Core Razor Pages, Bootstrap, jQuery, SignalR, Marked and Chart.js.
- Do not add React, Vue, Tailwind or another frontend framework.
- Do not change database schema, repositories, services, authentication, authorization, RAG, embedding, streaming or VNPay behavior.
- Use Inter with system fallbacks and semantic CSS variables; do not scatter color literals through Razor markup.
- Replace UI emoji with local SVG icons and accessible labels.
- Meet WCAG 2.1 AA contrast, focus and keyboard requirements.
- Verify at 320 px, 768 px, 1024 px and 1440 px.
- Preserve every existing form field, handler, route value and role condition.

---

## File Map

- `wwwroot/css/site.css`: tokens, reset, primitives, app shell and shared responsive rules.
- `wwwroot/css/pages.css`: domain-specific Login, Dashboard, Chat, Documents, Subjects, Packages, Research and Admin rules.
- `wwwroot/icons/app-icons.svg`: local SVG symbols referenced by `<svg><use>`.
- `wwwroot/js/site.js`: mobile navigation, password reveal and generic progressive enhancements.
- `Pages/Shared/_Layout.cshtml`: authenticated app shell, role-aware navigation, mobile header and footer.
- `Pages/Shared/_Icon.cshtml`: accessible shared SVG partial.
- Razor pages under `Pages/`: semantic markup using the shared components and CSS classes.

---

### Task 1: Design tokens, shared primitives and local icons

**Files:**
- Modify: `PRN212-VietnameseEduChat/wwwroot/css/site.css`
- Create: `PRN212-VietnameseEduChat/wwwroot/css/pages.css`
- Create: `PRN212-VietnameseEduChat/wwwroot/icons/app-icons.svg`
- Create: `PRN212-VietnameseEduChat/Pages/Shared/_Icon.cshtml`

**Interfaces:**
- Produces: semantic tokens such as `--color-primary`, primitives `.app-button`, `.app-card`, `.page-header`, `.status-badge`, `.empty-state`, and `_Icon` parameters `Name`, `Label`, `CssClass`.
- Consumes: Bootstrap CSS already bundled under `wwwroot/lib/bootstrap`.

- [ ] **Step 1: Record the failing baseline checks**

Run:

```powershell
rg -n --glob '*.cshtml' 'ðŸ|âœ|âš|style="' PRN212-VietnameseEduChat/Pages
rg -n --glob '*.css' '#[0-9a-fA-F]{6}' PRN212-VietnameseEduChat/wwwroot/css
```

Expected: multiple emoji/mojibake icon matches, inline-style matches and raw color literals.

- [ ] **Step 2: Replace `site.css` with the token and primitive foundation**

Define the approved tokens and core contracts at the beginning of `site.css`:

```css
:root {
  --color-canvas: #f4f7f9;
  --color-surface: #ffffff;
  --color-surface-subtle: #edf3f5;
  --color-sidebar: #0e2d3d;
  --color-sidebar-hover: #173f50;
  --color-primary: #0f766e;
  --color-primary-hover: #0b5f59;
  --color-primary-soft: #e6f5f2;
  --color-text: #172033;
  --color-text-muted: #627188;
  --color-border: #dce4ea;
  --color-success: #16794b;
  --color-warning: #9a5b08;
  --color-danger: #b42318;
  --color-info: #176b87;
  --shadow-sm: 0 1px 2px rgba(18, 38, 58, .06);
  --radius-sm: .375rem;
  --radius-md: .625rem;
  --radius-lg: .875rem;
  --sidebar-width: 16rem;
  --transition-fast: 160ms ease;
}

:focus-visible {
  outline: 3px solid rgba(15, 118, 110, .32);
  outline-offset: 2px;
}

.app-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  box-shadow: var(--shadow-sm);
}
```

Keep Bootstrap compatibility selectors for `.btn`, `.form-control`, `.form-select`, `.table`, `.alert`, `.badge`, `.modal` and `.dropdown-menu`, mapped to semantic variables.

- [ ] **Step 3: Add page-specific stylesheet and icon sprite**

Create `pages.css` with named sections only (`auth`, `dashboard`, `chat`, `documents`, `subjects`, `packages`, `research`, `admin`) and no raw color values outside `var(...)` usage. Create `app-icons.svg` with symbols named `home`, `chat`, `document`, `upload`, `book`, `chart`, `settings`, `users`, `package`, `payment`, `logout`, `menu`, `close`, `chevron-down`, `search`, `plus`, `send`, `trash`, `check`, `warning`, `info`, `eye`, `eye-off`.

- [ ] **Step 4: Add the shared icon partial**

Use this contract in `_Icon.cshtml`:

```razor
@model (string Name, string? Label, string? CssClass)
<svg class="app-icon @Model.CssClass"
     aria-hidden="@(string.IsNullOrWhiteSpace(Model.Label) ? "true" : "false")"
     role="@(string.IsNullOrWhiteSpace(Model.Label) ? null : "img")">
    @if (!string.IsNullOrWhiteSpace(Model.Label))
    {
        <title>@Model.Label</title>
    }
    <use href="/icons/app-icons.svg#@Model.Name"></use>
</svg>
```

- [ ] **Step 5: Build and validate asset output**

Run:

```powershell
dotnet build PRN212-VietnameseEduChat/PRN212-VietnameseEduChat.csproj --nologo
Test-Path PRN212-VietnameseEduChat/wwwroot/icons/app-icons.svg
```

Expected: build succeeds with 0 errors; `Test-Path` returns `True`.

- [ ] **Step 6: Commit the foundation**

```powershell
git add PRN212-VietnameseEduChat/wwwroot/css PRN212-VietnameseEduChat/wwwroot/icons PRN212-VietnameseEduChat/Pages/Shared/_Icon.cshtml
git commit -m "feat: add academic UI design system"
```

---

### Task 2: Responsive app shell and navigation

**Files:**
- Modify: `PRN212-VietnameseEduChat/Pages/Shared/_Layout.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Shared/_Layout.cshtml.css`
- Modify: `PRN212-VietnameseEduChat/wwwroot/js/site.js`
- Modify: `PRN212-VietnameseEduChat/wwwroot/css/site.css`

**Interfaces:**
- Consumes: `_Icon.cshtml` and design tokens from Task 1.
- Produces: `.app-shell`, `.app-sidebar`, `.app-main`, `.mobile-header`, `[data-sidebar-toggle]`, `[data-sidebar-dismiss]` used by all authenticated pages.

- [ ] **Step 1: Capture the current navigation behavior**

Run the app and verify `/Login` returns 200:

```powershell
dotnet run --project PRN212-VietnameseEduChat/PRN212-VietnameseEduChat.csproj --no-build --launch-profile http
```

In a second terminal:

```powershell
(Invoke-WebRequest http://localhost:5233/Login -UseBasicParsing).StatusCode
```

Expected: `200`.

- [ ] **Step 2: Convert `_Layout.cshtml` to a role-aware shell**

Use one `nav` with grouped links and preserve all current `User.IsInRole(...)` conditions. Render unauthenticated pages inside `.auth-shell`; authenticated pages inside:

```razor
<div class="app-shell" data-app-shell>
    <aside class="app-sidebar" id="appSidebar" aria-label="Điều hướng chính">
        <a class="app-brand" asp-page="/Dashboard">VietnameseEduChat</a>
        <nav class="sidebar-nav">...</nav>
        <div class="sidebar-account">...</div>
    </aside>
    <div class="sidebar-backdrop" data-sidebar-dismiss hidden></div>
    <div class="app-main">
        <header class="mobile-header">
            <button type="button" class="icon-button" data-sidebar-toggle
                    aria-controls="appSidebar" aria-expanded="false"
                    aria-label="Mở menu điều hướng">...</button>
        </header>
        <main id="main-content" class="app-content">@RenderBody()</main>
    </div>
</div>
```

Add a “Bỏ qua đến nội dung” skip link and remove `data-bs-theme="dark"`, the dark-theme stylesheet link, inline user-name color and emoji icons.

- [ ] **Step 3: Add keyboard-safe drawer behavior to `site.js`**

Implement `setSidebarOpen(open)` so it toggles `body.sidebar-open`, `aria-expanded`, `hidden` on the backdrop and restores focus to the opener when closing. Close on backdrop click, dismiss click and `Escape`; do nothing when shell elements are absent.

- [ ] **Step 4: Add responsive shell rules**

Desktop uses a fixed-width sidebar and flexible content. Below 1024 px the sidebar becomes fixed off-canvas with a backdrop. Under `prefers-reduced-motion: reduce`, disable transitions and smooth scrolling.

- [ ] **Step 5: Verify shell and commit**

Run `dotnet build`, load Login unauthenticated, then sign in as Student and System Admin to verify each sees only permitted navigation. Tab through the skip link, mobile toggle and logout.

```powershell
git add PRN212-VietnameseEduChat/Pages/Shared/_Layout.cshtml PRN212-VietnameseEduChat/Pages/Shared/_Layout.cshtml.css PRN212-VietnameseEduChat/wwwroot/js/site.js PRN212-VietnameseEduChat/wwwroot/css/site.css
git commit -m "feat: add responsive role-aware app shell"
```

---

### Task 3: Login, Dashboard, Error and Privacy

**Files:**
- Modify: `PRN212-VietnameseEduChat/Pages/Login.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Dashboard.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Error.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Privacy.cshtml`
- Modify: `PRN212-VietnameseEduChat/wwwroot/css/pages.css`
- Modify: `PRN212-VietnameseEduChat/wwwroot/js/site.js`

**Interfaces:**
- Consumes: auth/app layouts, `.app-card`, `.page-header`, `.app-button`, `_Icon`.
- Produces: `[data-password-toggle]` behavior and the reference patterns for later content pages.

- [ ] **Step 1: Add password visibility behavior**

Add a button next to `Password` with `data-password-toggle`, `aria-controls="Password"` and label “Hiện mật khẩu”. In `site.js`, toggle the target between `password` and `text`, swap the accessible label and icon state.

- [ ] **Step 2: Rebuild Login markup**

Use a two-column `.auth-layout`: a short product-value panel and an `.auth-card` containing the existing validation summary, `Email`, `Password`, submit button and demo account note. Preserve `asp-for`, validation spans, `autocomplete` values and POST behavior.

- [ ] **Step 3: Rebuild Dashboard markup by role**

Use `.dashboard-hero`, `.quick-actions`, `.metric-grid` and `.subject-grid`. Preserve the lecturer SignalR script and all role conditions. Do not invent counts not present in `DashboardModel`; quick-action cards link only to existing routes.

- [ ] **Step 4: Normalize auxiliary pages**

Wrap Error in `.message-state message-state--danger` with a safe Dashboard/Login action depending on authentication. Wrap Privacy in `.reading-layout`; keep its existing copy. Ensure Logout remains behavior-only and inherits the auth layout correctly.

- [ ] **Step 5: Verify and commit**

Build, sign in with all four seeded roles, verify Dashboard renders without exceptions and lecturer SignalR reconnects without console errors.

```powershell
git add PRN212-VietnameseEduChat/Pages/Login.cshtml PRN212-VietnameseEduChat/Pages/Dashboard.cshtml PRN212-VietnameseEduChat/Pages/Error.cshtml PRN212-VietnameseEduChat/Pages/Privacy.cshtml PRN212-VietnameseEduChat/wwwroot/css/pages.css PRN212-VietnameseEduChat/wwwroot/js/site.js
git commit -m "feat: redesign authentication and dashboards"
```

---

### Task 4: Chat workspace

**Files:**
- Modify: `PRN212-VietnameseEduChat/Pages/Chat/Index.cshtml`
- Modify: `PRN212-VietnameseEduChat/wwwroot/css/pages.css`

**Interfaces:**
- Consumes: app shell, `_Icon`, form and status primitives.
- Preserves: `chatForm`, `questionInput`, `sendBtn`, `subjectSelect`, `CurrentSessionId`, `chatTitle`, `chatError`, `chatMessages` IDs and all SignalR event handling.

- [ ] **Step 1: Capture current chat selectors**

Run:

```powershell
rg -n 'id="(chatForm|questionInput|sendBtn|subjectSelect|CurrentSessionId|chatTitle|chatError|chatMessages)"' PRN212-VietnameseEduChat/Pages/Chat/Index.cshtml
```

Expected: every listed ID exists before modification.

- [ ] **Step 2: Refactor chat markup without changing behavior**

Use semantic `<aside class="chat-history">`, `<section class="chat-workspace">`, `<header class="chat-header">`, `<div class="chat-thread" role="log" aria-live="polite">` and `<footer class="chat-composer">`. Replace inline styles and emoji with classes/icons. Preserve every form handler, hidden input, confirmation and JavaScript selector.

- [ ] **Step 3: Improve source and streaming UX**

Render sources as `<details class="source-card">` with document metadata in `<summary>` and excerpt inside. Add `aria-label="Gửi câu hỏi"` to send, `aria-busy` updates in `setStreaming`, and a textual “Đang tạo câu trả lời” state. Continue to sanitize user/source strings through the existing `escapeHtml` function.

- [ ] **Step 4: Implement mobile chat history drawer**

Under 768 px, hide history behind a “Cuộc trò chuyện” button while keeping the main composer visible. Reuse CSS classes and minimal local JavaScript in the existing IIFE; close history on session navigation and `Escape`.

- [ ] **Step 5: Verify and commit**

Verify new conversation, session navigation/deletion, subject selection, Enter/Shift+Enter, streaming tokens, sources and history URL replacement.

```powershell
git add PRN212-VietnameseEduChat/Pages/Chat/Index.cshtml PRN212-VietnameseEduChat/wwwroot/css/pages.css
git commit -m "feat: redesign the AI chat workspace"
```

---

### Task 5: Documents and Subjects

**Files:**
- Modify: `PRN212-VietnameseEduChat/Pages/Documents/Index.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Documents/Upload.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Documents/Details.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Subjects/Index.cshtml`
- Modify: `PRN212-VietnameseEduChat/wwwroot/css/pages.css`

**Interfaces:**
- Consumes: page header, toolbar, table, status badge, empty state, form and modal primitives.
- Preserves: all existing page handlers, upload limits, document status values and subject/chapter actions.

- [ ] **Step 1: Migrate document list**

Add semantic page header and a `.content-toolbar`; place the existing table in `.table-surface > .table-responsive`. Map statuses to `.status-badge--success|warning|danger|info|neutral` while keeping visible status text. Replace action clusters with labeled buttons/dropdowns.

- [ ] **Step 2: Migrate upload and details**

Use `.upload-panel`, `.file-dropzone`, `.form-section`, `.metadata-list` and `.document-status-panel`. Keep file input, accepted formats, size validation, all `asp-for` and POST handler values. Ensure rejected/failed states show icon, label, reason and a safe next action.

- [ ] **Step 3: Migrate subject/chapter management**

Use `.subject-list`, `.subject-card`, `.chapter-list` and standardized modal/form actions. Preserve lecturer assignment, chapter ordering and destructive confirmations. Every icon-only edit/delete control must gain an `aria-label` containing the entity name.

- [ ] **Step 4: Verify and commit**

Test empty/data states, upload validation, details navigation, subject creation/edit, chapter actions and lecturer assignment using Lecturer and Academic Admin accounts.

```powershell
git add PRN212-VietnameseEduChat/Pages/Documents PRN212-VietnameseEduChat/Pages/Subjects/Index.cshtml PRN212-VietnameseEduChat/wwwroot/css/pages.css
git commit -m "feat: redesign documents and subjects"
```

---

### Task 6: Packages, checkout and payment history

**Files:**
- Modify: `PRN212-VietnameseEduChat/Pages/Packages/Index.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Packages/Checkout.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Packages/History.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Packages/VnPayReturn.cshtml`
- Modify: `PRN212-VietnameseEduChat/wwwroot/css/pages.css`

**Interfaces:**
- Consumes: pricing card, summary list, status badge, alert and table primitives.
- Preserves: all package selection, quote, checkout, VNPay redirect/return and history logic.

- [ ] **Step 1: Redesign pricing selection**

Use `.pricing-grid` and `.pricing-card`; show package name, price, duration, limits, AI access and one CTA. Current/recommended states include visible text labels and `aria-label`, never color alone.

- [ ] **Step 2: Redesign checkout**

Group existing checkout controls into `.checkout-layout` with `.checkout-form` and sticky `.order-summary` on desktop. Do not rename any input or alter the current JavaScript calculations and handler submission.

- [ ] **Step 3: Redesign history and VNPay result**

Use shared table/status patterns for history. Render VNPay result as `.payment-result` with success/failure icon, text, transaction metadata and actions back to packages/history.

- [ ] **Step 4: Verify and commit**

Test package selection, validation, quote changes, sandbox redirect initiation, return states and history rendering without completing a paid external transaction.

```powershell
git add PRN212-VietnameseEduChat/Pages/Packages PRN212-VietnameseEduChat/wwwroot/css/pages.css
git commit -m "feat: redesign packages and payment flow"
```

---

### Task 7: Research workspace

**Files:**
- Modify: `PRN212-VietnameseEduChat/Pages/ResearchQuestions/Index.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/ResearchExperiments/Index.cshtml`
- Modify: `PRN212-VietnameseEduChat/wwwroot/css/pages.css`

**Interfaces:**
- Consumes: page header, toolbar, table, status, modal, form and metric primitives.
- Preserves: CSV import/export, experiment creation/run, Chart.js datasets and all handler names.

- [ ] **Step 1: Redesign research questions**

Separate import/create controls from data using `.research-toolbar` and existing Bootstrap modal behavior. Put questions in a responsive data surface with subject/chapter/source metadata, keywords and clear actions. Preserve CSV sample/download/import routes.

- [ ] **Step 2: Redesign experiments**

Use `.experiment-grid`, `.experiment-card`, `.configuration-list` and `.metric-strip`. Keep every model, embedding, chunking, TopK, run and export field exactly wired to the current model/handlers.

- [ ] **Step 3: Normalize Chart.js palette**

Read CSS variables with `getComputedStyle(document.documentElement)` and map chart colors from `--color-primary`, `--color-info`, `--color-warning`, `--color-success`; retain labels and add point/border differences so series do not rely only on color.

- [ ] **Step 4: Verify and commit**

Test modal keyboard focus, CSV import errors, filters, experiment creation, running-state UI, results, export and charts at desktop/mobile widths.

```powershell
git add PRN212-VietnameseEduChat/Pages/ResearchQuestions/Index.cshtml PRN212-VietnameseEduChat/Pages/ResearchExperiments/Index.cshtml PRN212-VietnameseEduChat/wwwroot/css/pages.css
git commit -m "feat: redesign research workspaces"
```

---

### Task 8: Admin modules

**Files:**
- Modify: `PRN212-VietnameseEduChat/Pages/Admin/Dashboard.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Admin/ChunkingConfig.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Admin/Packages.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Admin/Payments.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Admin/Users/Index.cshtml`
- Modify: `PRN212-VietnameseEduChat/Pages/Admin/Users/Import.cshtml`
- Modify: `PRN212-VietnameseEduChat/wwwroot/css/pages.css`

**Interfaces:**
- Consumes: all shared design primitives.
- Preserves: role checks, account locking, user import, package CRUD, payment data and chunking configuration behavior.

- [ ] **Step 1: Migrate admin Dashboard and configuration**

Use `.admin-metric-grid`, `.admin-action-grid` and `.settings-panel`. Keep every dashboard value and chunking input; add helper text and group dependent fields semantically.

- [ ] **Step 2: Migrate Packages and Payments**

Use the shared page-header/toolbar/table/status patterns. Destructive actions use `.app-button--danger` plus confirmation text naming the affected item.

- [ ] **Step 3: Migrate Users and Import**

Keep filters, paging, lock/unlock and import behavior. Organize user actions in a dropdown on narrow screens; show role and lock status with text badges. Present import requirements, chosen file, validation summary and result counts as distinct regions.

- [ ] **Step 4: Verify and commit**

Test Academic Admin and System Admin separately, including forbidden navigation, account actions, package forms, payment tables, chunking save and CSV import validation.

```powershell
git add PRN212-VietnameseEduChat/Pages/Admin PRN212-VietnameseEduChat/wwwroot/css/pages.css
git commit -m "feat: redesign administration modules"
```

---

### Task 9: Remove legacy styling and complete verification

**Files:**
- Delete: `PRN212-VietnameseEduChat/wwwroot/css/dark-theme.css`
- Modify: `PRN212-VietnameseEduChat/Pages/Shared/_Layout.cshtml`
- Modify: `PRN212-VietnameseEduChat/wwwroot/css/site.css`
- Modify: `PRN212-VietnameseEduChat/wwwroot/css/pages.css`
- Modify: any Razor page found by the explicit audit commands below.

**Interfaces:**
- Consumes: completed UI from Tasks 1–8.
- Produces: build-clean, responsive and accessible full-site redesign.

- [ ] **Step 1: Run static UI audits**

```powershell
rg -n --glob '*.cshtml' 'ðŸ|âœ|âš' PRN212-VietnameseEduChat/Pages
rg -n --glob '*.cshtml' 'style="' PRN212-VietnameseEduChat/Pages
rg -n 'dark-theme\.css|data-bs-theme="dark"' PRN212-VietnameseEduChat
rg -n --glob '*.css' '#[0-9a-fA-F]{6}' PRN212-VietnameseEduChat/wwwroot/css/pages.css
```

Expected: no legacy emoji/mojibake icon, dark-theme reference or raw page color matches. Any remaining inline style must be a documented dynamic value (for example a runtime chart dimension); otherwise replace it with a class before proceeding.

- [ ] **Step 2: Remove legacy dark stylesheet**

Delete `dark-theme.css` only after confirming `_Layout.cshtml` no longer references it and required styles are represented in `site.css`/`pages.css`.

- [ ] **Step 3: Build and run HTTP smoke tests**

```powershell
dotnet build PRN212-VietnameseEduChat.sln --nologo
$pages = @('/Login', '/Privacy')
foreach ($page in $pages) {
  $response = Invoke-WebRequest "http://localhost:5233$page" -UseBasicParsing
  if ($response.StatusCode -ne 200) { throw "$page returned $($response.StatusCode)" }
}
```

Expected: build succeeds with 0 errors and both public pages return 200.

- [ ] **Step 4: Run role and responsive browser verification**

For each seeded account—Student, Lecturer, Academic Admin and System Admin—verify navigation, Dashboard and permitted modules. Capture Login, Dashboard, Chat, Documents/Subjects, Packages, Research and Admin at 320, 768, 1024 and 1440 px. Check console errors, asset 404s, overflow, focus order, Escape handling and keyboard activation.

- [ ] **Step 5: Review the complete diff**

```powershell
git status --short
git diff --check
git diff --stat HEAD~8..HEAD
git diff HEAD~8..HEAD -- PRN212-VietnameseEduChat/Pages PRN212-VietnameseEduChat/wwwroot
```

Confirm no `.cs`, migration, service, repository or configuration-secret file was changed.

- [ ] **Step 6: Commit final polish**

```powershell
git add PRN212-VietnameseEduChat/Pages PRN212-VietnameseEduChat/wwwroot/css PRN212-VietnameseEduChat/wwwroot/js PRN212-VietnameseEduChat/wwwroot/icons
git commit -m "fix: complete responsive and accessible UI polish"
```

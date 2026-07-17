$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent $PSScriptRoot
$layoutPath = Join-Path $projectRoot 'PRN212-VietnameseEduChat/Pages/Shared/_Layout.cshtml'
$dashboardPath = Join-Path $projectRoot 'PRN212-VietnameseEduChat/Pages/Dashboard.cshtml'
$chatPath = Join-Path $projectRoot 'PRN212-VietnameseEduChat/Pages/Chat/Index.cshtml'
$siteCssPath = Join-Path $projectRoot 'PRN212-VietnameseEduChat/wwwroot/css/site.css'
$pagesCssPath = Join-Path $projectRoot 'PRN212-VietnameseEduChat/wwwroot/css/pages.css'

$layout = Get-Content -Raw -LiteralPath $layoutPath
$dashboard = Get-Content -Raw -LiteralPath $dashboardPath
$chat = Get-Content -Raw -LiteralPath $chatPath
$siteCss = Get-Content -Raw -LiteralPath $siteCssPath
$pagesCss = Get-Content -Raw -LiteralPath $pagesCssPath

$checks = [ordered]@{
    'Skip link exists' = $layout.Contains('class="skip-link"')
    'Navigation has accessible label' = $layout.Contains('aria-label="Điều hướng chính"')
    'Student task region exists' = $dashboard.Contains('data-role-dashboard="student"')
    'Lecturer task region exists' = $dashboard.Contains('data-role-dashboard="lecturer"')
    'Academic admin task region exists' = $dashboard.Contains('data-role-dashboard="academic-admin"')
    'System admin task region exists' = $dashboard.Contains('data-role-dashboard="system-admin"')
    'Task grid styles exist' = $pagesCss.Contains('.task-action-grid')
    'Chat has a semantic H1' = $chat.Contains('<h1 class="chat-main-title"')
    'Reduced motion is respected' = $siteCss.Contains('prefers-reduced-motion')
    'Shared icon sprite is used' = $layout.Contains('/icons/app-icons.svg') -or (Test-Path (Join-Path $projectRoot 'PRN212-VietnameseEduChat/wwwroot/icons/app-icons.svg'))
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value })
if ($failed.Count -gt 0) {
    foreach ($check in $failed) {
        [Console]::Error.WriteLine("UI check failed: $($check.Key)")
    }

    exit 1
}

Write-Host "Role UI verification passed: $($checks.Count) checks."

$ErrorActionPreference = 'Stop'

$trackedUploads = @(
    git ls-files 'PRN212-VietnameseEduChat/wwwroot/uploads/**'
)

if ($trackedUploads.Count -gt 0) {
    throw "Runtime uploads are tracked: $($trackedUploads -join ', ')"
}

$ignoredStorage = git check-ignore `
    'PRN212-VietnameseEduChat/App_Data/documents/probe.pdf' `
    'PRN212-VietnameseEduChat/wwwroot/uploads/probe.pdf'

if (@($ignoredStorage).Count -ne 2) {
    throw 'Runtime document storage paths are not fully ignored.'
}

$secretPatterns = 'sk-proj-|sk-[A-Za-z0-9_-]{20,}|HashSecret["'']?\s*[:=]\s*["''][A-Za-z0-9]{16,}'
$secretMatches = git grep -n -I -E $secretPatterns -- `
    ':!docs/superpowers/**' `
    ':!scripts/verify-repository-hygiene.ps1'

if ($LASTEXITCODE -eq 0 -and $secretMatches) {
    throw "Potential tracked secret found:`n$secretMatches"
}

Write-Output 'Repository hygiene verification passed.'

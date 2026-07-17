using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Services.Interfaces;
using PRN212_VietnameseEduChat.Services.Security;

namespace PRN212_VietnameseEduChat.Pages.Documents
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IDocumentService _documentService;
        private readonly ISubjectLecturerService _subjectLecturerService;
        private readonly IWebHostEnvironment _environment;

        public IndexModel(
            IDocumentService documentService,
            ISubjectLecturerService subjectLecturerService,
            IWebHostEnvironment environment)
        {
            _documentService = documentService;
            _subjectLecturerService = subjectLecturerService;
            _environment = environment;
        }

        public List<Document> Documents { get; set; } = new();

        public HashSet<int> AssignedSubjectIds { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadAssignedSubjectIdsAsync();

            var allDocuments = await _documentService.GetAllAsync();

            Documents = allDocuments
                .Where(CanViewDocument)
                .ToList();
        }

        public async Task<IActionResult> OnGetDownloadAsync(int id)
        {
            await LoadAssignedSubjectIdsAsync();

            var document = await _documentService.GetByIdAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            if (!CanViewDocument(document))
            {
                return Forbid();
            }

            var physicalPath = GetPhysicalPath(document);

            if (!System.IO.File.Exists(physicalPath))
            {
                TempData["ErrorMessage"] =
                    "Không tìm thấy file gốc trên server.";

                return RedirectToPage();
            }

            var contentType = string.IsNullOrWhiteSpace(document.ContentType)
                ? "application/octet-stream"
                : document.ContentType;

            return PhysicalFile(
                physicalPath,
                contentType,
                document.OriginalFileName);
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!User.IsInRole(AppRoles.AcademicAdmin))
            {
                return Forbid();
            }

            var document = await _documentService.GetByIdAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            if (document.Status == "PendingApproval")
            {
                TempData["ErrorMessage"] =
                    "Không nên xóa tài liệu đang chờ duyệt. Hãy duyệt hoặc từ chối tài liệu trước.";

                return RedirectToPage();
            }

            await _documentService.DeleteAsync(id);

            TempData["SuccessMessage"] =
                "Đã xóa tài liệu thành công.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            if (!User.IsInRole(AppRoles.Lecturer))
            {
                return Forbid();
            }

            var document = await _documentService.GetByIdAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            if (!await CanReviewDocumentAsync(document))
            {
                return Forbid();
            }

            var reviewerId = GetCurrentUserId();

            try
            {
                await _documentService.ApproveAsync(id, reviewerId);

                TempData["SuccessMessage"] =
                    "Đã duyệt tài liệu thành công.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(
            int id,
            string reason)
        {
            if (!User.IsInRole(AppRoles.Lecturer))
            {
                return Forbid();
            }

            var document = await _documentService.GetByIdAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            if (!await CanReviewDocumentAsync(document))
            {
                return Forbid();
            }

            var reviewerId = GetCurrentUserId();

            try
            {
                await _documentService.RejectAsync(
                    id,
                    reviewerId,
                    reason);

                TempData["SuccessMessage"] =
                    "Đã từ chối tài liệu.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }

        public bool CanReviewDocument(Document document)
        {
            return User.IsInRole(AppRoles.Lecturer) &&
                   document.SubjectId.HasValue &&
                   AssignedSubjectIds.Contains(document.SubjectId.Value);
        }

        private bool CanViewDocument(Document document)
        {
            if (User.IsInRole(AppRoles.AcademicAdmin))
            {
                return true;
            }

            if (!IsDocumentSubjectActive(document))
            {
                return false;
            }

            if (User.IsInRole(AppRoles.Student))
            {
                return document.Status == "Approved";
            }

            if (User.IsInRole(AppRoles.Lecturer))
            {
                return CanReviewDocument(document);
            }

            return false;
        }

        private static bool IsDocumentSubjectActive(Document document)
        {
            return !document.SubjectId.HasValue ||
                   document.Subject?.IsActive == true;
        }

        private async Task<bool> CanReviewDocumentAsync(Document document)
        {
            if (!User.IsInRole(AppRoles.Lecturer) ||
                !document.SubjectId.HasValue)
            {
                return false;
            }

            return await _subjectLecturerService
                .IsLecturerAssignedAsync(
                    document.SubjectId.Value,
                    GetCurrentUserId());
        }

        private async Task LoadAssignedSubjectIdsAsync()
        {
            AssignedSubjectIds.Clear();

            if (!User.IsInRole(AppRoles.Lecturer))
            {
                return;
            }

            var currentUserId = GetCurrentUserId();

            if (currentUserId == 0)
            {
                return;
            }

            var subjects = await _subjectLecturerService
                .GetAssignedSubjectsAsync(currentUserId);

            AssignedSubjectIds = subjects
                .Select(x => x.SubjectId)
                .ToHashSet();
        }

        private int GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            return int.TryParse(userIdValue, out var userId)
                ? userId
                : 0;
        }

        private string GetPhysicalPath(Document document)
        {
            return Path.Combine(
                _environment.WebRootPath,
                document.FilePath.Replace(
                    "/",
                    Path.DirectorySeparatorChar.ToString()));
        }
    }
}

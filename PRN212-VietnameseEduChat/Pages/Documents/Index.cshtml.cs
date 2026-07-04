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
        private readonly IWebHostEnvironment _environment;

        public IndexModel(
            IDocumentService documentService,
            IWebHostEnvironment environment)
        {
            _documentService = documentService;
            _environment = environment;
        }

        public List<Document> Documents { get; set; } = new();

        public async Task OnGetAsync()
        {
            var allDocuments = await _documentService.GetAllAsync();

            Documents = allDocuments
                .Where(CanViewDocument)
                .ToList();
        }

        public async Task<IActionResult> OnGetDownloadAsync(int id)
        {
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
            if (!User.IsInRole(AppRoles.AcademicAdmin))
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
            if (!User.IsInRole(AppRoles.AcademicAdmin))
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

        private bool CanViewDocument(Document document)
        {
            if (User.IsInRole(AppRoles.AcademicAdmin))
            {
                return true;
            }

            if (User.IsInRole(AppRoles.Student))
            {
                return document.Status == "Approved";
            }

            if (User.IsInRole(AppRoles.Lecturer))
            {
                var currentUserId = GetCurrentUserId();

                return document.Status == "Approved"
                    || document.UploadedBy == currentUserId;
            }

            return false;
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
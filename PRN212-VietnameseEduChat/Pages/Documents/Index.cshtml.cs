using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Services.Interfaces;

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
            Documents = await _documentService.GetAllAsync();
        }

        public async Task<IActionResult> OnGetDownloadAsync(int id)
        {
            var document = await _documentService.GetByIdAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            var physicalPath = Path.Combine(
                _environment.WebRootPath,
                document.FilePath.Replace(
                    "/",
                    Path.DirectorySeparatorChar.ToString()));

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
            await _documentService.DeleteAsync(id);

            TempData["SuccessMessage"] =
                "Đã xóa tài liệu thành công.";

            return RedirectToPage();
        }
    }
}

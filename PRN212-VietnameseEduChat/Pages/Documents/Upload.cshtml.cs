using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN212_VietnameseEduChat.Services.Interfaces;
using System.Security.Claims;

namespace PRN212_VietnameseEduChat.Pages.Documents
{
    [Authorize]
    public class UploadModel : PageModel
    {
        private readonly IDocumentService _documentService;

        public UploadModel(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [BindProperty]
        public IFormFile? UploadFile { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (UploadFile == null || UploadFile.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(UploadFile),
                    "Vui lòng chọn tài liệu cần tải lên.");

                return Page();
            }

            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return RedirectToPage("/Login");
            }

            try
            {
                await _documentService.UploadAsync(
                    UploadFile,
                    userId);

                TempData["SuccessMessage"] =
                    "Tải lên và index tài liệu thành công.";

                return RedirectToPage("Index");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                return Page();
            }
        }
    }
}

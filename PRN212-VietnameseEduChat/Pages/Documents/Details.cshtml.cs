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
    public class DetailsModel : PageModel
    {
        private readonly IDocumentService _documentService;
        private readonly IWebHostEnvironment _environment;

        public DetailsModel(
            IDocumentService documentService,
            IWebHostEnvironment environment)
        {
            _documentService = documentService;
            _environment = environment;
        }

        public Document? Document { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Document = await _documentService.GetByIdAsync(id);

            if (Document == null)
            {
                return NotFound();
            }

            if (!CanViewDocument(Document))
            {
                return Forbid();
            }

            return Page();
        }

        public async Task<IActionResult> OnGetPreviewAsync(int id)
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

            if (document.ContentType != "application/pdf")
            {
                return BadRequest();
            }

            var physicalPath = Path.Combine(
                _environment.WebRootPath,
                document.FilePath.Replace(
                    "/",
                    Path.DirectorySeparatorChar.ToString()));

            if (!System.IO.File.Exists(physicalPath))
            {
                return NotFound();
            }

            return new PhysicalFileResult(
                physicalPath,
                document.ContentType)
            {
                EnableRangeProcessing = true
            };
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
    }
}
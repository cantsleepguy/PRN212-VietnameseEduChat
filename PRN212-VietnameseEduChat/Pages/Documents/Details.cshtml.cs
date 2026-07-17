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
        private readonly ISubjectLecturerService _subjectLecturerService;
        private readonly IWebHostEnvironment _environment;

        public DetailsModel(
            IDocumentService documentService,
            ISubjectLecturerService subjectLecturerService,
            IWebHostEnvironment environment)
        {
            _documentService = documentService;
            _subjectLecturerService = subjectLecturerService;
            _environment = environment;
        }

        public Document? Document { get; set; }

        public bool CanViewChunks { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Document = await _documentService.GetByIdWithChunksAsync(id);

            if (Document == null)
            {
                return NotFound();
            }

            if (!await CanViewDocumentAsync(Document))
            {
                return Forbid();
            }

            CanViewChunks = await CanViewChunksAsync(Document);

            return Page();
        }

        public async Task<IActionResult> OnGetPreviewAsync(int id)
        {
            var document = await _documentService.GetByIdAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            if (!await CanViewDocumentAsync(document))
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

        private async Task<bool> CanViewDocumentAsync(Document document)
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
                if (!document.SubjectId.HasValue)
                {
                    return false;
                }

                return await _subjectLecturerService
                    .IsLecturerAssignedAsync(
                        document.SubjectId.Value,
                        GetCurrentUserId());
            }

            return false;
        }

        private static bool IsDocumentSubjectActive(Document document)
        {
            return !document.SubjectId.HasValue ||
                   document.Subject?.IsActive == true;
        }

        private async Task<bool> CanViewChunksAsync(Document document)
        {
            if (User.IsInRole(AppRoles.AcademicAdmin))
            {
                return true;
            }

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

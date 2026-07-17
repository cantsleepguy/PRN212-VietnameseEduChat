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
        private readonly IDocumentAccessPolicy _accessPolicy;

        public DetailsModel(
            IDocumentService documentService,
            ISubjectLecturerService subjectLecturerService,
            IDocumentAccessPolicy accessPolicy)
        {
            _documentService = documentService;
            _subjectLecturerService = subjectLecturerService;
            _accessPolicy = accessPolicy;
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
            var download = await _documentService.OpenDownloadAsync(id, User);
            if (download == null)
            {
                return NotFound();
            }

            if (download.ContentType != "application/pdf")
            {
                await download.Content.DisposeAsync();
                return BadRequest();
            }

            return new FileStreamResult(download.Content, download.ContentType)
            {
                EnableRangeProcessing = true
            };
        }

        public async Task<IActionResult> OnGetDownloadAsync(int id)
        {
            var download = await _documentService.OpenDownloadAsync(id, User);
            if (download == null)
            {
                return NotFound();
            }

            return new FileStreamResult(download.Content, download.ContentType)
            {
                FileDownloadName = download.OriginalFileName,
                EnableRangeProcessing = true
            };
        }

        private async Task<bool> CanViewDocumentAsync(Document document)
        {
            return await _accessPolicy.CanReadAsync(document, User);
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

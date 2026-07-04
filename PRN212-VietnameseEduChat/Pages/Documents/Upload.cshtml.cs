using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Services.Interfaces;
using PRN212_VietnameseEduChat.Services.Security;
using System.Security.Claims;

namespace PRN212_VietnameseEduChat.Pages.Documents
{
    [Authorize(Roles = AppRoles.LecturerOrAcademicAdmin)]
    public class UploadModel : PageModel
    {
        private readonly IDocumentService _documentService;
        private readonly ISubjectService _subjectService;
        private readonly IChapterService _chapterService;

        public UploadModel(
            IDocumentService documentService,
            ISubjectService subjectService,
            IChapterService chapterService)
        {
            _documentService = documentService;
            _subjectService = subjectService;
            _chapterService = chapterService;
        }

        [BindProperty]
        public IFormFile? UploadFile { get; set; }

        [BindProperty]
        public int? SubjectId { get; set; }

        [BindProperty]
        public int? ChapterId { get; set; }

        public List<Subject> Subjects { get; set; } = new();

        public List<Chapter> Chapters { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadLookupsAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadLookupsAsync();

            if (SubjectId == null)
            {
                ModelState.AddModelError(
                    nameof(SubjectId),
                    "Vui lòng chọn môn học.");
            }

            if (ChapterId == null)
            {
                ModelState.AddModelError(
                    nameof(ChapterId),
                    "Vui lòng chọn chương.");
            }

            if (UploadFile == null || UploadFile.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(UploadFile),
                    "Vui lòng chọn tài liệu cần tải lên.");
            }

            if (!ModelState.IsValid)
            {
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
                    UploadFile!,
                    userId,
                    SubjectId!.Value,
                    ChapterId!.Value);

                TempData["SuccessMessage"] =
                    "Tải lên và index tài liệu thành công. Tài liệu đang chờ duyệt.";

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

        private async Task LoadLookupsAsync()
        {
            Subjects = await _subjectService.GetAllAsync();
            Chapters = await _chapterService.GetAllAsync();
        }
    }
}
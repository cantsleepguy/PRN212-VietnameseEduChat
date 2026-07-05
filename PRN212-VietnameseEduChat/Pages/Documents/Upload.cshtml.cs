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
        private readonly ISubjectLecturerService _subjectLecturerService;

        public UploadModel(
            IDocumentService documentService,
            ISubjectService subjectService,
            IChapterService chapterService,
            ISubjectLecturerService subjectLecturerService)
        {
            _documentService = documentService;
            _subjectService = subjectService;
            _chapterService = chapterService;
            _subjectLecturerService = subjectLecturerService;
        }

        [BindProperty]
        public IFormFile? UploadFile { get; set; }

        [BindProperty]
        public int? SubjectId { get; set; }

        [BindProperty]
        public int? ChapterId { get; set; }

        public List<Subject> Subjects { get; set; } = new();

        public List<Chapter> Chapters { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = GetCurrentUserId();

            if (userId == 0)
            {
                return RedirectToPage("/Login");
            }

            await LoadLookupsAsync(userId);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = GetCurrentUserId();

            if (userId == 0)
            {
                return RedirectToPage("/Login");
            }

            await LoadLookupsAsync(userId);

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

            try
            {
                var canUploadAnySubject = User.IsInRole(AppRoles.AcademicAdmin);

                await _documentService.UploadAsync(
                    UploadFile!,
                    userId,
                    SubjectId!.Value,
                    ChapterId!.Value,
                    canUploadAnySubject);

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

        private async Task LoadLookupsAsync(int userId)
        {
            if (User.IsInRole(AppRoles.AcademicAdmin))
            {
                Subjects = await _subjectService.GetAllAsync();
            }
            else
            {
                Subjects = await _subjectLecturerService
                    .GetAssignedSubjectsAsync(userId);
            }

            var allowedSubjectIds = Subjects
                .Select(x => x.SubjectId)
                .ToHashSet();

            var allChapters = await _chapterService.GetAllAsync();

            Chapters = allChapters
                .Where(x => allowedSubjectIds.Contains(x.SubjectId))
                .ToList();
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
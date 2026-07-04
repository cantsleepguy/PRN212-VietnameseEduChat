using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Services.Interfaces;
using PRN212_VietnameseEduChat.Services.Security;

namespace PRN212_VietnameseEduChat.Pages.Subjects
{
    [Authorize(Roles = AppRoles.AcademicAdmin)]
    public class IndexModel : PageModel
    {
        private readonly ISubjectService _subjectService;
        private readonly IChapterService _chapterService;

        public IndexModel(
            ISubjectService subjectService,
            IChapterService chapterService)
        {
            _subjectService = subjectService;
            _chapterService = chapterService;
        }

        public List<Subject> Subjects { get; set; } = new();

        [BindProperty]
        public string NewSubjectName { get; set; } = string.Empty;

        [BindProperty]
        public string? NewSubjectDescription { get; set; }

        [BindProperty]
        public int ChapterSubjectId { get; set; }

        [BindProperty]
        public string NewChapterName { get; set; } = string.Empty;

        [BindProperty]
        public int NewChapterOrderIndex { get; set; } = 1;

        public async Task OnGetAsync()
        {
            await LoadSubjectsAsync();
        }

        public async Task<IActionResult> OnPostCreateSubjectAsync()
        {
            try
            {
                await _subjectService.CreateAsync(
                    NewSubjectName,
                    NewSubjectDescription);

                TempData["SuccessMessage"] =
                    "Đã tạo môn học thành công.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCreateChapterAsync()
        {
            try
            {
                await _chapterService.CreateAsync(
                    ChapterSubjectId,
                    NewChapterName,
                    NewChapterOrderIndex);

                TempData["SuccessMessage"] =
                    "Đã tạo chương thành công.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteSubjectAsync(int id)
        {
            try
            {
                await _subjectService.DeleteAsync(id);

                TempData["SuccessMessage"] =
                    "Đã xóa môn học.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteChapterAsync(int id)
        {
            try
            {
                await _chapterService.DeleteAsync(id);

                TempData["SuccessMessage"] =
                    "Đã xóa chương.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }

        private async Task LoadSubjectsAsync()
        {
            Subjects = await _subjectService.GetAllAsync();

            foreach (var subject in Subjects)
            {
                subject.Chapters = subject.Chapters
                    .OrderBy(x => x.OrderIndex)
                    .ToList();
            }
        }
    }
}

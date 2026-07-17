using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Hubs;
using PRN212_VietnameseEduChat.Services.Interfaces;
using PRN212_VietnameseEduChat.Services.Security;
using System.Security.Claims;

namespace PRN212_VietnameseEduChat.Pages.Subjects
{
    [Authorize(Roles = AppRoles.AcademicAdmin)]
    public class IndexModel : PageModel
    {
        private readonly ISubjectService _subjectService;
        private readonly IChapterService _chapterService;

        private readonly ISubjectLecturerService _subjectLecturerService;
        private readonly IHubContext<SubjectHub> _subjectHubContext;

        public List<User> Lecturers { get; set; } = new();

        [BindProperty]
        public int AssignSubjectId { get; set; }

        [BindProperty]
        public int AssignLecturerId { get; set; }

        public IndexModel(
            ISubjectService subjectService,
            IChapterService chapterService,
            ISubjectLecturerService subjectLecturerService,
            IHubContext<SubjectHub> subjectHubContext)
        {
            _subjectService = subjectService;
            _chapterService = chapterService;
            _subjectLecturerService = subjectLecturerService;
            _subjectHubContext = subjectHubContext;
        }

        public List<Subject> Subjects { get; set; } = new();

        [BindProperty]
        public string NewSubjectName { get; set; } = string.Empty;

        [BindProperty]
        public string? NewSubjectDescription { get; set; }

        [BindProperty]
        public int EditSubjectId { get; set; }

        [BindProperty]
        public string EditSubjectName { get; set; } = string.Empty;

        [BindProperty]
        public string? EditSubjectDescription { get; set; }

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

                await NotifySubjectsChangedAsync("SubjectCreated");

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

                await NotifySubjectsChangedAsync(
                    "ChapterCreated",
                    ChapterSubjectId);

                TempData["SuccessMessage"] =
                    "Đã tạo chương thành công.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateSubjectAsync()
        {
            try
            {
                await _subjectService.UpdateAsync(
                    EditSubjectId,
                    EditSubjectName,
                    EditSubjectDescription);

                await NotifySubjectsChangedAsync(
                    "SubjectUpdated",
                    EditSubjectId);

                TempData["SuccessMessage"] =
                    "Đã cập nhật môn học.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostHideSubjectAsync(int id)
        {
            try
            {
                await _subjectService.HideAsync(id);

                await NotifySubjectsChangedAsync("SubjectHidden", id);

                TempData["SuccessMessage"] =
                    "Đã ẩn môn học.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRestoreSubjectAsync(int id)
        {
            try
            {
                await _subjectService.RestoreAsync(id);

                await NotifySubjectsChangedAsync("SubjectRestored", id);

                TempData["SuccessMessage"] =
                    "Đã mở lại môn học.";
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

                await NotifySubjectsChangedAsync("ChapterDeleted");

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

                subject.SubjectLecturers = subject.SubjectLecturers
                    .OrderBy(x => x.Lecturer!.FullName)
                    .ToList();
            }

            Lecturers = await _subjectLecturerService.GetLecturersAsync();
        }

        public async Task<IActionResult> OnPostAssignLecturerAsync()
        {
            var assignedBy = GetCurrentUserId();

            try
            {
                await _subjectLecturerService.AssignAsync(
                    AssignSubjectId,
                    AssignLecturerId,
                    assignedBy);

                await NotifySubjectsChangedAsync(
                    "LecturerAssigned",
                    AssignSubjectId,
                    AssignLecturerId);

                TempData["SuccessMessage"] =
                    "Đã phân công giảng viên vào môn học.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUnassignLecturerAsync(int subjectId, int lecturerId)
        {
            try
            {
                await _subjectLecturerService.UnassignAsync(
                    subjectId,
                    lecturerId);

                await NotifySubjectsChangedAsync(
                    "LecturerUnassigned",
                    subjectId,
                    lecturerId);

                TempData["SuccessMessage"] =
                    "Đã hủy phân công giảng viên.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }

        private int GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            return int.TryParse(userIdValue, out var userId)
                ? userId
                : 0;
        }

        private async Task NotifySubjectsChangedAsync(
            string action,
            int? subjectId = null,
            int? lecturerId = null)
        {
            await _subjectHubContext.Clients.All.SendAsync(
                "SubjectsChanged",
                new
                {
                    action,
                    subjectId,
                    lecturerId,
                    changedBy = GetCurrentUserId()
                });
        }
    }
}

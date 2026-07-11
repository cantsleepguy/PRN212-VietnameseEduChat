using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Chats;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Services.Interfaces;

namespace PRN212_VietnameseEduChat.Pages.Chat
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IChatService _chatService;
        private readonly ISubjectService _subjectService;

        public IndexModel(
            IChatService chatService,
            ISubjectService subjectService)
        {
            _chatService = chatService;
            _subjectService = subjectService;
        }

        public List<ChatSessionDto> Sessions { get; set; } = new();

        public List<Subject> Subjects { get; set; } = new();

        public ChatSessionDetailDto? CurrentSession { get; set; }

        [BindProperty]
        public int? CurrentSessionId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SubjectId { get; set; }

        [BindProperty]
        public string Question { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int? sessionId)
        {
            var userId = GetCurrentUserId();

            await LoadPageDataAsync(userId);

            if (sessionId.HasValue)
            {
                CurrentSession = await _chatService.GetSessionDetailAsync(
                    sessionId.Value,
                    userId);

                if (CurrentSession == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy cuộc trò chuyện.";
                    return RedirectToPage("/Chat/Index");
                }

                CurrentSessionId = CurrentSession.ChatSessionId;
                SubjectId = CurrentSession.SubjectId;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAskAsync()
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrWhiteSpace(Question))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Vui lòng nhập câu hỏi.");

                await ReloadCurrentPageAsync(userId);
                return Page();
            }

            try
            {
                var request = new ChatAskRequestDto
                {
                    ChatSessionId = CurrentSessionId,
                    SubjectId = SubjectId,
                    Question = Question
                };

                var response = await _chatService.AskAsync(
                    request,
                    userId);

                CurrentSessionId = response.ChatSessionId;

                CurrentSession = await _chatService.GetSessionDetailAsync(
                    response.ChatSessionId,
                    userId);

                if (CurrentSession != null)
                {
                    SubjectId = CurrentSession.SubjectId;
                }

                Question = string.Empty;

                await LoadPageDataAsync(userId);

                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                await ReloadCurrentPageAsync(userId);

                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int chatSessionId)
        {
            var userId = GetCurrentUserId();

            try
            {
                await _chatService.DeleteSessionAsync(
                    chatSessionId,
                    userId);

                TempData["SuccessMessage"] = "Đã xóa cuộc trò chuyện.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage("/Chat/Index");
        }

        private async Task ReloadCurrentPageAsync(int userId)
        {
            await LoadPageDataAsync(userId);

            if (CurrentSessionId.HasValue)
            {
                CurrentSession = await _chatService.GetSessionDetailAsync(
                    CurrentSessionId.Value,
                    userId);

                if (CurrentSession != null)
                {
                    SubjectId = CurrentSession.SubjectId;
                }
            }
        }

        private async Task LoadPageDataAsync(int userId)
        {
            Sessions = await _chatService.GetUserSessionsAsync(userId);
            Subjects = await _subjectService.GetAllAsync();
        }

        private int GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                throw new InvalidOperationException(
                    "Không xác định được người dùng hiện tại.");
            }

            return userId;
        }
    }
}

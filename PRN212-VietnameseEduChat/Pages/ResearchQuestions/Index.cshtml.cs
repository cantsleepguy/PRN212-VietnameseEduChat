using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Research;
using PRN212_VietnameseEduChat.Services.Interfaces;
using PRN212_VietnameseEduChat.Services.Security;
using Microsoft.AspNetCore.Http;

namespace PRN212_VietnameseEduChat.Pages.ResearchQuestions
{
    [Authorize(Roles = AppRoles.AcademicAdmin)]
    public class IndexModel : PageModel
    {
        private readonly IResearchQuestionService _researchQuestionService;

        [BindProperty]
        public IFormFile? ImportFile { get; set; }

        public IndexModel(IResearchQuestionService researchQuestionService)
        {
            _researchQuestionService = researchQuestionService;
        }

        public List<ResearchQuestionDto> Questions { get; set; } = new();

        public ResearchQuestionFormOptionsDto Options { get; set; } = new();

        public int CurrentQuestionCount { get; set; }

        public bool IsEditMode => ResearchQuestionId.HasValue;

        [BindProperty]
        public int? ResearchQuestionId { get; set; }

        [BindProperty]
        public ResearchQuestionInputDto Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? editId)
        {
            await LoadPageDataAsync();

            if (editId.HasValue)
            {
                var question = await _researchQuestionService
                    .GetByIdAsync(editId.Value);

                if (question == null)
                {
                    TempData["ErrorMessage"] =
                        "Không tìm thấy câu hỏi test set.";

                    return RedirectToPage("/ResearchQuestions/Index");
                }

                ResearchQuestionId = question.ResearchQuestionId;

                Input = new ResearchQuestionInputDto
                {
                    SubjectId = question.SubjectId,
                    ChapterId = question.ChapterId,
                    SourceDocumentId = question.SourceDocumentId,
                    Question = question.Question,
                    GroundTruthAnswer = question.GroundTruthAnswer,
                    ExpectedKeywords = question.ExpectedKeywords,
                    ExpectedSource = question.ExpectedSource
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            try
            {
                if (ResearchQuestionId.HasValue)
                {
                    await _researchQuestionService.UpdateAsync(
                        ResearchQuestionId.Value,
                        Input);

                    TempData["SuccessMessage"] =
                        "Đã cập nhật câu hỏi test set.";
                }
                else
                {
                    await _researchQuestionService.CreateAsync(Input);

                    TempData["SuccessMessage"] =
                        "Đã thêm câu hỏi vào test set.";
                }

                return RedirectToPage("/ResearchQuestions/Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                await LoadPageDataAsync();

                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                await _researchQuestionService.DeleteAsync(id);

                TempData["SuccessMessage"] =
                    "Đã xóa câu hỏi test set.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage("/ResearchQuestions/Index");
        }

        private async Task LoadPageDataAsync()
        {
            Questions = await _researchQuestionService.GetAllAsync();
            Options = await _researchQuestionService.GetFormOptionsAsync();
            CurrentQuestionCount = await _researchQuestionService.CountAsync();
        }

        public async Task<IActionResult> OnPostImportAsync()
        {
            try
            {
                if (ImportFile == null)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Vui lòng chọn file CSV.");

                    await LoadPageDataAsync();

                    return Page();
                }

                var result = await _researchQuestionService
                    .ImportCsvAsync(ImportFile);

                TempData["SuccessMessage"] =
                    $"Import hoàn tất. Thành công: {result.SuccessCount}, lỗi: {result.FailedCount}.";

                if (result.Errors.Count > 0)
                {
                    TempData["ImportErrors"] =
                        string.Join(Environment.NewLine, result.Errors);
                }

                return RedirectToPage("/ResearchQuestions/Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                await LoadPageDataAsync();

                return Page();
            }
        }
    }
}
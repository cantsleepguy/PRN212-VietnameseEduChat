using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Research;
using PRN212_VietnameseEduChat.Services.Interfaces;
using PRN212_VietnameseEduChat.Services.Security;

namespace PRN212_VietnameseEduChat.Pages.ResearchExperiments
{
    [Authorize(Roles = AppRoles.AcademicAdmin)]
    public class IndexModel : PageModel
    {
        private readonly IResearchBenchmarkService _researchBenchmarkService;
        private readonly IEmbeddingService _embeddingService;
        private readonly IResearchChunkingService _researchChunkingService;
        public List<ResearchChunkingStrategyOptionDto> ChunkingStrategies { get; set; } = new();

        public IndexModel(
            IResearchBenchmarkService researchBenchmarkService,
            IEmbeddingService embeddingService,
            IResearchChunkingService researchChunkingService)
        {
            _researchBenchmarkService = researchBenchmarkService;
            _embeddingService = embeddingService;
            _researchChunkingService = researchChunkingService;
        }

        public List<ResearchExperimentDto> Experiments { get; set; } = new();

        public ResearchExperimentDetailDto? CurrentDetail { get; set; }

        [BindProperty]
        public ResearchExperimentCreateDto Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? experimentId)
        {
            await LoadPageDataAsync();

            Input.EmbeddingProvider = "OpenAI";
            Input.EmbeddingModelName = _embeddingService.GetModelName();
            Input.EmbeddingDimensions = _embeddingService.GetDimensions();
            Input.AnswerModelName = "gpt-4o-mini";
            Input.ChunkingStrategyKey = "fixed-baseline";
            Input.ExperimentName = "RAG baseline - fixed-size 1200/200";

            if (experimentId.HasValue)
            {
                CurrentDetail = await _researchBenchmarkService
                    .GetExperimentDetailAsync(experimentId.Value);

                if (CurrentDetail == null)
                {
                    TempData["ErrorMessage"] =
                        "Không tìm thấy experiment.";

                    return RedirectToPage("/ResearchExperiments/Index");
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            try
            {
                var experimentId = await _researchBenchmarkService
                    .CreateExperimentAsync(Input);

                TempData["SuccessMessage"] =
                    "Đã tạo experiment. Bạn có thể bấm Chạy benchmark.";

                return RedirectToPage(
                    "/ResearchExperiments/Index",
                    new { experimentId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                await LoadPageDataAsync();

                return Page();
            }
        }

        public async Task<IActionResult> OnPostRunAsync(int experimentId)
        {
            try
            {
                await _researchBenchmarkService.RunExperimentAsync(
                    experimentId);

                TempData["SuccessMessage"] =
                    "Benchmark đã chạy xong.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage(
                "/ResearchExperiments/Index",
                new { experimentId });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int experimentId)
        {
            try
            {
                await _researchBenchmarkService.DeleteExperimentAsync(
                    experimentId);

                TempData["SuccessMessage"] =
                    "Đã xóa experiment.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage("/ResearchExperiments/Index");
        }

        private async Task LoadPageDataAsync()
        {
            Experiments = await _researchBenchmarkService
                .GetExperimentsAsync();

            ChunkingStrategies = _researchChunkingService.GetStrategies();
        }
    }
}
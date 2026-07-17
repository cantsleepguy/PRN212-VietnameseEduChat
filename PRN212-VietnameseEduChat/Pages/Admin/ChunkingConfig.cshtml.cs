using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Services.Implementations;
using PRN212_VietnameseEduChat.Services.Interfaces;
using PRN212_VietnameseEduChat.Services.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Pages.Admin
{
    [Authorize(Roles = AppRoles.AnyAdmin)]
    public class ChunkingConfigModel : PageModel
    {
        private readonly IChunkingConfigurationService _configurationService;
        private readonly IDocumentService _documentService;

        public ChunkingConfigModel(
            IChunkingConfigurationService configurationService,
            IDocumentService documentService)
        {
            _configurationService = configurationService;
            _documentService = documentService;
        }

        public List<ChunkingConfiguration> Configurations { get; set; } = new();

        public List<Document> Documents { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadAsync();
        }

        public async Task<IActionResult> OnPostActivateAsync(int configurationId)
        {
            try
            {
                await _configurationService.ActivateAsync(
                    configurationId,
                    GetCurrentUserId());

                TempData["SuccessMessage"] =
                    "Đã kích hoạt cấu hình chunking. " +
                    "Tài liệu mới sẽ dùng cấu hình này. " +
                    "Tài liệu cũ cần Re-index để áp dụng.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateAsync(
            int configurationId,
            int chunkSize,
            int chunkOverlap,
            string fixedSizeUnit)
        {
            try
            {
                await _configurationService.UpdateAsync(
                    configurationId,
                    chunkSize,
                    chunkOverlap,
                    fixedSizeUnit ?? "Characters",
                    GetCurrentUserId());

                TempData["SuccessMessage"] = "Đã cập nhật cấu hình chunking.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostReindexAsync(int documentId)
        {
            try
            {
                await _documentService.ReindexAsync(documentId);

                TempData["SuccessMessage"] =
                    "Đã re-index tài liệu theo cấu hình chunking hiện tại.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }

        private async Task LoadAsync()
        {
            await _configurationService.EnsureDefaultsAsync();

            var configurations = await _configurationService.GetAllAsync();

            Configurations = configurations
                .Where(configuration =>
                    configuration.StrategyKey ==
                    ChunkingConfigurationService.StrategyCharacter)
                .Take(1)
                .ToList();

            Documents = await _documentService.GetAllAsync();
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

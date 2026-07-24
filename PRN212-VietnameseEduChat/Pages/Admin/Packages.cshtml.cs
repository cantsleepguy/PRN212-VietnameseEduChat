using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Services.Interfaces;
using PRN212_VietnameseEduChat.Services.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Pages.Admin
{
    [Authorize(Roles = AppRoles.AnyAdmin)]
    public class PackagesModel : PageModel
    {
        private readonly IPackageService _packageService;

        public PackagesModel(IPackageService packageService)
        {
            _packageService = packageService;
        }

        public List<Package> Packages { get; set; } = new();

        public async Task OnGetAsync()
        {
            Packages = await _packageService.GetAllAsync();
        }

        public async Task<IActionResult> OnPostUpdateAsync(
            int packageId,
            string packageName,
            string? description,
            decimal price,
            int? durationDays,
            int? dailyQuestionLimit,
            bool isActive)
        {
            try
            {
                var package = await _packageService.GetByIdAsync(packageId);

                if (package == null)
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy gói dịch vụ.");
                }

                package.PackageName = packageName?.Trim()
                    ?? package.PackageName;
                package.Description = description?.Trim();
                package.Price = price;
                package.DurationDays = durationDays ?? 0;
                package.DailyQuestionLimit = dailyQuestionLimit;
                package.IsActive = isActive;

                await _packageService.UpdateAsync(package);

                TempData["SuccessMessage"] = "Đã cập nhật gói dịch vụ.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage();
        }
    }
}

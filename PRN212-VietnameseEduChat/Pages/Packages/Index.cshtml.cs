using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Subscriptions;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Pages.Packages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IPackageService _packageService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IPaymentService _paymentService;

        public IndexModel(
            IPackageService packageService,
            ISubscriptionService subscriptionService,
            IPaymentService paymentService)
        {
            _packageService = packageService;
            _subscriptionService = subscriptionService;
            _paymentService = paymentService;
        }

        public List<Package> Packages { get; set; } = new();

        public UserPackageInfoDto? CurrentPackageInfo { get; set; }

        public bool IsLowerTierPackage(Package package)
        {
            if (CurrentPackageInfo?.Package == null)
            {
                return false;
            }

            return GetPackageRank(package.PackageCode) <
                   GetPackageRank(CurrentPackageInfo.Package.PackageCode);
        }

        public async Task OnGetAsync()
        {
            var userId = GetCurrentUserId();

            Packages = await _packageService.GetActiveAsync();

            CurrentPackageInfo = await _subscriptionService
                .GetUserPackageInfoAsync(userId);
        }

        public async Task<IActionResult> OnPostBuyAsync(int packageId)
        {
            var userId = GetCurrentUserId();

            try
            {
                var targetPackage = await _packageService.GetByIdAsync(
                    packageId);

                var currentPackageInfo = await _subscriptionService
                    .GetUserPackageInfoAsync(userId);

                if (targetPackage != null &&
                    currentPackageInfo?.Package != null &&
                    GetPackageRank(targetPackage.PackageCode) <
                    GetPackageRank(currentPackageInfo.Package.PackageCode))
                {
                    throw new InvalidOperationException(
                        "Tài khoản đã sở hữu gói cao hơn nên không thể mua gói thấp hơn.");
                }

                var redirectUrl = await _paymentService.InitiatePaymentAsync(
                    userId,
                    packageId);

                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                return RedirectToPage("/Packages/Index");
            }
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

        private static int GetPackageRank(string packageCode)
        {
            return packageCode.Trim().ToLowerInvariant() switch
            {
                "free" => 0,
                "premium" => 1,
                "enterprise" => 2,
                _ => 0
            };
        }
    }
}

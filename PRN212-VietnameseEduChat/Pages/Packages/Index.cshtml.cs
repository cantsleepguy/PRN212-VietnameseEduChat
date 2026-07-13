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
    }
}

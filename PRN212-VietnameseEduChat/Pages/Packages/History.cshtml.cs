using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Pages.Packages
{
    [Authorize]
    public class HistoryModel : PageModel
    {
        private readonly IPaymentService _paymentService;
        private readonly ISubscriptionService _subscriptionService;

        public HistoryModel(
            IPaymentService paymentService,
            ISubscriptionService subscriptionService)
        {
            _paymentService = paymentService;
            _subscriptionService = subscriptionService;
        }

        public List<Payment> Payments { get; set; } = new();

        public List<UserSubscription> Subscriptions { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = GetCurrentUserId();

            Payments = await _paymentService.GetUserPaymentsAsync(userId);

            Subscriptions = await _subscriptionService
                .GetUserSubscriptionsAsync(userId);
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

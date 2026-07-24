using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Services.Interfaces;
using PRN212_VietnameseEduChat.Services.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Pages.Admin
{
    [Authorize(Roles = AppRoles.AnyAdmin)]
    public class PaymentsModel : PageModel
    {
        private readonly IPaymentService _paymentService;

        public PaymentsModel(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public List<Payment> Payments { get; set; } = new();

        public const int PageSize = 10;

        public int ItemsPerPage => PageSize;

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int TotalPayments { get; set; }

        public int TotalPages { get; set; }

        public decimal TotalRevenue { get; set; }

        public int SuccessCount { get; set; }

        public int PendingCount { get; set; }

        public int FailedCount { get; set; }

        public async Task OnGetAsync()
        {
            var allPayments = await _paymentService.GetAllPaymentsAsync();

            TotalPayments = allPayments.Count;
            TotalPages = Math.Max(
                1,
                (int)Math.Ceiling(TotalPayments / (double)PageSize));

            PageNumber = Math.Clamp(PageNumber, 1, TotalPages);

            Payments = allPayments
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            TotalRevenue = allPayments
                .Where(p => p.Status == "Success")
                .Sum(p => p.Amount);

            SuccessCount = allPayments.Count(p => p.Status == "Success");
            PendingCount = allPayments.Count(p => p.Status == "Pending");
            FailedCount = allPayments.Count(p => p.Status == "Failed");
        }
    }
}

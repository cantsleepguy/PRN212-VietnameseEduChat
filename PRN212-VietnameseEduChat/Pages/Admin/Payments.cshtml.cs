using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Services.Interfaces;
using PRN212_VietnameseEduChat.Services.Security;
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

        public decimal TotalRevenue { get; set; }

        public int SuccessCount { get; set; }

        public int PendingCount { get; set; }

        public int FailedCount { get; set; }

        public async Task OnGetAsync()
        {
            Payments = await _paymentService.GetAllPaymentsAsync();

            TotalRevenue = Payments
                .Where(p => p.Status == "Success")
                .Sum(p => p.Amount);

            SuccessCount = Payments.Count(p => p.Status == "Success");
            PendingCount = Payments.Count(p => p.Status == "Pending");
            FailedCount = Payments.Count(p => p.Status == "Failed");
        }
    }
}

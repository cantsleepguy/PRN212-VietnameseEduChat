using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Payments;
using PRN212_VietnameseEduChat.Services.Interfaces;

namespace PRN212_VietnameseEduChat.Pages.Packages
{
    [AllowAnonymous]
    public sealed class VnPayReturnModel : PageModel
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<VnPayReturnModel> _logger;

        public VnPayReturnModel(
            IPaymentService paymentService,
            ILogger<VnPayReturnModel> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        public VnPayReturnResultDto Result { get; set; }
            = new();

        public async Task OnGetAsync()
        {
            try
            {
                var callbackValues = Request.Query
                    .ToDictionary(
                        item => item.Key,
                        item => item.Value.ToString(),
                        StringComparer.OrdinalIgnoreCase);

                Result =
                    await _paymentService
                        .ProcessVnPayReturnAsync(
                            callbackValues);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Không thể xử lý Return URL VNPay.");

                Result = new VnPayReturnResultDto
                {
                    IsSignatureValid = false,
                    IsProviderSuccessful = false,
                    Message =
                        "Không thể xử lý kết quả thanh toán."
                };
            }
        }
    }
}
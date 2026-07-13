using System.Globalization;
using Microsoft.Extensions.Options;
using PRN212_VietnameseEduChat.BusinessObjects.Constants;
using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Payments;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Services.Interfaces;
using PRN212_VietnameseEduChat.Services.Options;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    public sealed class VnPayPaymentProvider
        : IPaymentProvider
    {
        private readonly VnPaySettings _settings;

        public VnPayPaymentProvider(
            IOptions<VnPaySettings> options)
        {
            _settings = options.Value;
        }

        public string ProviderName =>
            PaymentProviders.VnPay;

        public Task<PaymentInitResult>
            CreatePaymentUrlAsync(
                Payment payment,
                string clientIpAddress,
                string? bankCode,
                CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (payment.Amount <= 0)
            {
                throw new InvalidOperationException(
                    "Số tiền thanh toán không hợp lệ.");
            }

            var baseUrl =
                _settings.PublicBaseUrl.TrimEnd('/');

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException(
                    "Chưa cấu hình VnPay:PublicBaseUrl.");
            }

            var now = GetVietnamTime();

            var expiresAt = payment.ExpiresAt
                ?? now.AddMinutes(
                    _settings.ExpirationMinutes);

            var amount = checked(
                (long)decimal.Ceiling(payment.Amount));

            var orderInfo =
                $"Thanh toan {payment.PackageCodeSnapshot} " +
                $"{payment.TransactionId}";

            var parameters =
                new Dictionary<string, string>(
                    StringComparer.Ordinal)
                {
                    ["vnp_Version"] = _settings.Version,
                    ["vnp_Command"] = _settings.Command,
                    ["vnp_TmnCode"] = _settings.TmnCode,

                    ["vnp_Amount"] =
                        checked(amount * 100L)
                            .ToString(
                                CultureInfo.InvariantCulture),

                    ["vnp_CreateDate"] =
                        now.ToString(
                            "yyyyMMddHHmmss",
                            CultureInfo.InvariantCulture),

                    ["vnp_CurrCode"] = "VND",

                    ["vnp_IpAddr"] =
                        string.IsNullOrWhiteSpace(clientIpAddress)
                            ? "127.0.0.1"
                            : clientIpAddress,

                    ["vnp_Locale"] = _settings.Locale,
                    ["vnp_OrderInfo"] = orderInfo,
                    ["vnp_OrderType"] = _settings.OrderType,

                    ["vnp_ReturnUrl"] =
                        $"{baseUrl}/Packages/VnPayReturn",

                    ["vnp_TxnRef"] = payment.TransactionId,

                    ["vnp_ExpireDate"] =
                        expiresAt.ToString(
                            "yyyyMMddHHmmss",
                            CultureInfo.InvariantCulture)
                };

            if (!string.IsNullOrWhiteSpace(bankCode))
            {
                parameters["vnp_BankCode"] = bankCode;
            }

            var queryString =
                VnPaySecurity.BuildQueryString(parameters);

            var secureHash =
                VnPaySecurity.ComputeHmacSha512(
                    _settings.HashSecret,
                    queryString);

            var redirectUrl =
                $"{_settings.PaymentUrl}?{queryString}" +
                $"&vnp_SecureHash={secureHash}";

            return Task.FromResult(
                new PaymentInitResult
                {
                    RedirectUrl = redirectUrl,
                    OrderInfo = orderInfo,
                    RequestedBankCode = bankCode
                });
        }

        public bool ValidateCallbackSignature(
            IReadOnlyDictionary<string, string> values)
        {
            return VnPaySecurity.ValidateSignature(
                values,
                _settings.HashSecret);
        }

        public VnPayCallbackData ParseCallback(
            IReadOnlyDictionary<string, string> values)
        {
            var rawAmountText =
                GetValue(values, "vnp_Amount");

            long.TryParse(
                rawAmountText,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var rawAmount);

            DateTime? payDate = null;

            var payDateText =
                GetValue(values, "vnp_PayDate");

            if (DateTime.TryParseExact(
                    payDateText,
                    "yyyyMMddHHmmss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedPayDate))
            {
                payDate = parsedPayDate;
            }

            return new VnPayCallbackData
            {
                TransactionId =
                    GetValue(values, "vnp_TxnRef"),

                RawAmount = rawAmount,

                ResponseCode =
                    GetValue(values, "vnp_ResponseCode"),

                TransactionStatus =
                    GetValue(
                        values,
                        "vnp_TransactionStatus"),

                TransactionNo =
                    GetValue(
                        values,
                        "vnp_TransactionNo"),

                BankCode =
                    GetValue(values, "vnp_BankCode"),

                CardType =
                    GetValue(values, "vnp_CardType"),

                OrderInfo =
                    GetValue(values, "vnp_OrderInfo"),

                PayDate = payDate
            };
        }

        private static string GetValue(
            IReadOnlyDictionary<string, string> values,
            string key)
        {
            return values.TryGetValue(key, out var value)
                ? value
                : string.Empty;
        }

        private static DateTime GetVietnamTime()
        {
            var timeZoneId =
                OperatingSystem.IsWindows()
                    ? "SE Asia Standard Time"
                    : "Asia/Ho_Chi_Minh";

            var timeZone =
                TimeZoneInfo.FindSystemTimeZoneById(
                    timeZoneId);

            return TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                timeZone);
        }
    }
}
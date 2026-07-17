using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    internal static class VnPaySecurity
    {
        public static string BuildQueryString(
            IEnumerable<KeyValuePair<string, string>> values)
        {
            return string.Join(
                "&",
                values
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x.Value))
                    .OrderBy(
                        x => x.Key,
                        StringComparer.Ordinal)
                    .Select(x =>
                        $"{Encode(x.Key)}={Encode(x.Value)}"));
        }

        public static string ComputeHmacSha512(
            string secret,
            string data)
        {
            // Secret ở đây là VnPay:HashSecret, dùng để tạo chữ ký vnp_SecureHash.
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var dataBytes = Encoding.UTF8.GetBytes(data);

            using var hmac = new HMACSHA512(keyBytes);

            var hash = hmac.ComputeHash(dataBytes);

            return Convert.ToHexString(hash)
                .ToLowerInvariant();
        }

        public static bool ValidateSignature(
            IReadOnlyDictionary<string, string> values,
            string hashSecret)
        {
            if (!values.TryGetValue(
                    "vnp_SecureHash",
                    out var providedHash) ||
                string.IsNullOrWhiteSpace(providedHash))
            {
                return false;
            }

            var signedValues = values
                .Where(x =>
                    x.Key.StartsWith(
                        "vnp_",
                        StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(
                        x.Key,
                        "vnp_SecureHash",
                        StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(
                        x.Key,
                        "vnp_SecureHashType",
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            var hashData =
                BuildQueryString(signedValues);

            // Tạo lại chữ ký bằng VnPay:HashSecret để so sánh với vnp_SecureHash VNPay gửi về.
            var expectedHash =
                ComputeHmacSha512(
                    hashSecret,
                    hashData);

            return string.Equals(
                expectedHash,
                providedHash,
                StringComparison.OrdinalIgnoreCase);
        }

        private static string Encode(string value)
        {
            return WebUtility.UrlEncode(value);
        }
    }
}

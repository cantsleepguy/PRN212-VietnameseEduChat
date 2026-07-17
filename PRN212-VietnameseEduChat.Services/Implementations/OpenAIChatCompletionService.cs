using Microsoft.Extensions.Configuration;
using PRN212_VietnameseEduChat.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    public class OpenAIChatCompletionService : IChatCompletionService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public OpenAIChatCompletionService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GenerateAnswerAsync(string prompt)
        {
            // Lấy OpenAI API key từ cấu hình: appsettings/User Secrets/environment.
            var apiKey = _configuration["OpenAI:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "Chưa cấu hình OpenAI API key.");
            }

            var model = _configuration["OpenAI:ChatModel"]
                ?? "gpt-4o-mini";

            var requestBody = new
            {
                model,
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content =
                            "Bạn là trợ lý học tập tiếng Việt. " +
                            "Chỉ trả lời dựa trên tài liệu được cung cấp. " +
                            "Nếu tài liệu không có thông tin, hãy nói rõ là không tìm thấy trong tài liệu."
                    },
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                temperature = 0.2
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.openai.com/v1/chat/completions");

            // Dòng này gắn API key vào header để gọi OpenAI Chat API.
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            request.Content = JsonContent.Create(requestBody);

            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new InvalidOperationException(
                    $"Gọi OpenAI Chat API thất bại: {error}");
            }

            await using var stream =
                await response.Content.ReadAsStreamAsync();

            var result = await JsonSerializer.DeserializeAsync
                <OpenAIChatCompletionResponse>(
                    stream,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            var answer = result?
                .Choices?
                .FirstOrDefault()?
                .Message?
                .Content;

            if (string.IsNullOrWhiteSpace(answer))
            {
                throw new InvalidOperationException(
                    "OpenAI không trả về câu trả lời hợp lệ.");
            }

            return answer.Trim();
        }

        public async IAsyncEnumerable<string> GenerateAnswerStreamAsync(
            string prompt,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Lấy OpenAI API key từ cấu hình để gọi API streaming.
            var apiKey = _configuration["OpenAI:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "Chưa cấu hình OpenAI API key.");
            }

            var model = _configuration["OpenAI:ChatModel"]
                ?? "gpt-4o-mini";

            var requestBody = new
            {
                model,
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content =
                            "Bạn là trợ lý học tập tiếng Việt. " +
                            "Chỉ trả lời dựa trên tài liệu được cung cấp. " +
                            "Nếu tài liệu không có thông tin, hãy nói rõ là không tìm thấy trong tài liệu. " +
                            "Trả lời bằng định dạng Markdown khi phù hợp (danh sách, in đậm, tiêu đề)."
                    },
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                temperature = 0.2,
                stream = true
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.openai.com/v1/chat/completions");

            // Dòng này gắn API key vào header để gọi OpenAI Chat API dạng stream.
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            request.Content = JsonContent.Create(requestBody);

            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content
                    .ReadAsStringAsync(cancellationToken);

                throw new InvalidOperationException(
                    $"Gọi OpenAI Chat API thất bại: {error}");
            }

            await using var stream = await response.Content
                .ReadAsStreamAsync(cancellationToken);

            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var line = await reader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (!line.StartsWith("data: ", StringComparison.Ordinal))
                    continue;

                var payload = line.Substring("data: ".Length).Trim();

                if (payload == "[DONE]")
                    yield break;

                string? token = null;

                try
                {
                    using var json = JsonDocument.Parse(payload);

                    if (json.RootElement.TryGetProperty(
                            "choices",
                            out var choices) &&
                        choices.GetArrayLength() > 0 &&
                        choices[0].TryGetProperty(
                            "delta",
                            out var delta) &&
                        delta.TryGetProperty(
                            "content",
                            out var content))
                    {
                        token = content.GetString();
                    }
                }
                catch (JsonException)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(token))
                {
                    yield return token;
                }
            }
        }

        private class OpenAIChatCompletionResponse
        {
            public List<OpenAIChoice> Choices { get; set; } = new();
        }

        private class OpenAIChoice
        {
            public OpenAIMessage? Message { get; set; }
        }

        private class OpenAIMessage
        {
            public string Content { get; set; } = string.Empty;
        }
    }
}

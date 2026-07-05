using Microsoft.Extensions.Configuration;
using PRN212_VietnameseEduChat.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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

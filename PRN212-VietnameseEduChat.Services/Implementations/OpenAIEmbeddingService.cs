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
    public class OpenAIEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public OpenAIEmbeddingService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public string GetModelName()
        {
            return _configuration["OpenAI:EmbeddingModel"]
                ?? "text-embedding-3-small";
        }

        public int GetDimensions(string? modelName = null)
        {
            if (string.IsNullOrWhiteSpace(modelName))
            {
                modelName = GetModelName();
            }

            if (modelName.Equals(
                    "text-embedding-3-large",
                    StringComparison.OrdinalIgnoreCase))
            {
                return 3072;
            }

            if (modelName.Equals(
                    "text-embedding-3-small",
                    StringComparison.OrdinalIgnoreCase))
            {
                return 1536;
            }

            var value = _configuration["OpenAI:EmbeddingDimensions"];

            if (int.TryParse(value, out var dimensions))
            {
                return dimensions;
            }

            return 1536;
        }

        public async Task<float[]> CreateEmbeddingAsync(
            string text,
            string? modelName = null,
            int? dimensions = null)
        {
            var apiKey = _configuration["OpenAI:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "Chưa cấu hình OpenAI API key.");
            }

            var model = string.IsNullOrWhiteSpace(modelName)
                ? GetModelName()
                : modelName.Trim();

            var finalDimensions = dimensions ?? GetDimensions(model);

            var requestBody = new Dictionary<string, object>
            {
                ["model"] = model,
                ["input"] = text,
                ["encoding_format"] = "float",
                ["dimensions"] = finalDimensions
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.openai.com/v1/embeddings");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            request.Content = JsonContent.Create(requestBody);

            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new InvalidOperationException(
                    $"Gọi OpenAI Embedding API thất bại: {error}");
            }

            await using var stream =
                await response.Content.ReadAsStreamAsync();

            var result = await JsonSerializer.DeserializeAsync
                <OpenAIEmbeddingResponse>(
                    stream,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            var embedding = result?
                .Data?
                .FirstOrDefault()?
                .Embedding;

            if (embedding == null || embedding.Length == 0)
            {
                throw new InvalidOperationException(
                    "OpenAI không trả về embedding hợp lệ.");
            }

            return embedding;
        }

        private class OpenAIEmbeddingResponse
        {
            public List<OpenAIEmbeddingData> Data { get; set; }
                = new List<OpenAIEmbeddingData>();
        }

        private class OpenAIEmbeddingData
        {
            public float[] Embedding { get; set; }
                = Array.Empty<float>();
        }
    }
}

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

        private static readonly HashSet<string> LocalPythonModels =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "bge-m3",
                "multilingual-e5-base",
                "phobert-base"
            };

        public int GetDimensions(string? modelName = null)
        {
            if (string.IsNullOrWhiteSpace(modelName))
            {
                modelName = GetModelName();
            }

            if (modelName.Equals(
                    "bge-m3",
                    StringComparison.OrdinalIgnoreCase))
            {
                return 1024;
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

            if (modelName.Equals(
                    "multilingual-e5-base",
                    StringComparison.OrdinalIgnoreCase))
            {
                return 768;
            }

            if (modelName.Equals(
                    "phobert-base",
                    StringComparison.OrdinalIgnoreCase))
            {
                return 768;
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
            var model = string.IsNullOrWhiteSpace(modelName)
                ? GetModelName()
                : modelName.Trim();

            if (LocalPythonModels.Contains(model))
            {
                return await CreateLocalEmbeddingAsync(model, text);
            }

            return await CreateOpenAIEmbeddingAsync(
                text,
                model,
                dimensions);
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

        private async Task<float[]> CreateOpenAIEmbeddingAsync(
    string text,
    string model,
    int? dimensions)
        {
            // Lấy OpenAI API key từ cấu hình để gọi Embedding API.
            var apiKey = _configuration["OpenAI:ApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "Chưa cấu hình OpenAI API key.");
            }

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

            // Dòng này gắn API key vào header để gọi OpenAI Embedding API.
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

        private async Task<float[]> CreateLocalEmbeddingAsync(
            string modelName,
            string text)
        {
            var baseUrl = _configuration["LocalEmbedding:BaseUrl"]
                ?? "http://127.0.0.1:8001";

            var requestBody = new
            {
                model = modelName,
                text
            };

            using var response = await _httpClient.PostAsJsonAsync(
                $"{baseUrl.TrimEnd('/')}/embed",
                requestBody);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                throw new InvalidOperationException(
                    $"Gọi local embedding service ({modelName}) thất bại: {error}");
            }

            await using var stream =
                await response.Content.ReadAsStreamAsync();

            var result = await JsonSerializer.DeserializeAsync
                <LocalEmbeddingResponse>(
                    stream,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (result?.Embedding == null || result.Embedding.Length == 0)
            {
                throw new InvalidOperationException(
                    $"Local embedding service ({modelName}) không trả về embedding hợp lệ.");
            }

            return result.Embedding;
        }

        private class LocalEmbeddingResponse
        {
            public string Model { get; set; } = string.Empty;

            public int Dimensions { get; set; }

            public float[] Embedding { get; set; } = Array.Empty<float>();
        }
    }
}

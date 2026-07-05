using Microsoft.EntityFrameworkCore;
using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Chats;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.DataAccess.Context;
using PRN212_VietnameseEduChat.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    public class ChatService : IChatService
    {
        private const int TopK = 5;
        private const int RecentMessageCount = 8;
        private const double MinimumSimilarityScore = 0.15;

        private readonly ApplicationDbContext _context;
        private readonly IEmbeddingService _embeddingService;
        private readonly IChatCompletionService _chatCompletionService;

        public ChatService(
            ApplicationDbContext context,
            IEmbeddingService embeddingService,
            IChatCompletionService chatCompletionService)
        {
            _context = context;
            _embeddingService = embeddingService;
            _chatCompletionService = chatCompletionService;
        }

        public async Task<ChatAskResponseDto> AskAsync(
            ChatAskRequestDto request,
            int userId)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                throw new InvalidOperationException(
                    "Câu hỏi không được để trống.");
            }

            var question = request.Question.Trim();

            var chatSession = await GetOrCreateSessionAsync(
                request,
                userId);

            var userMessage = new ChatMessage
            {
                ChatSessionId = chatSession.ChatSessionId,
                Role = "User",
                Content = question,
                CreatedAt = DateTime.Now
            };

            _context.ChatMessages.Add(userMessage);

            var questionEmbedding =
                await _embeddingService.CreateEmbeddingAsync(question);

            var relevantChunks = await FindRelevantChunksAsync(
                questionEmbedding,
                chatSession.SubjectId);

            string answer;

            if (relevantChunks.Count == 0)
            {
                answer =
                    "Tôi không tìm thấy thông tin phù hợp trong các tài liệu đã được duyệt/index. " +
                    "Bạn có thể thử hỏi lại cụ thể hơn hoặc kiểm tra xem tài liệu đã được duyệt chưa.";
            }
            else
            {
                var recentMessages = await GetRecentMessagesAsync(
                    chatSession.ChatSessionId);

                var prompt = BuildPrompt(
                    question,
                    relevantChunks,
                    recentMessages);

                answer = await _chatCompletionService.GenerateAnswerAsync(
                    prompt);
            }

            var assistantMessage = new ChatMessage
            {
                ChatSessionId = chatSession.ChatSessionId,
                Role = "Assistant",
                Content = answer,
                CreatedAt = DateTime.Now
            };

            _context.ChatMessages.Add(assistantMessage);

            foreach (var chunk in relevantChunks)
            {
                _context.ChatMessageSources.Add(new ChatMessageSource
                {
                    ChatMessage = assistantMessage,
                    DocumentChunkId = chunk.DocumentChunkId,
                    SimilarityScore = chunk.SimilarityScore,
                    Excerpt = CreateExcerpt(chunk.Content),
                    CreatedAt = DateTime.Now
                });
            }

            chatSession.UpdatedAt = DateTime.Now;

            if (chatSession.Title == "Cuộc trò chuyện mới")
            {
                chatSession.Title = GenerateTitle(question);
            }

            await _context.SaveChangesAsync();

            return new ChatAskResponseDto
            {
                ChatSessionId = chatSession.ChatSessionId,
                Answer = answer,
                Sources = relevantChunks.Select(chunk => new ChatSourceDto
                {
                    DocumentChunkId = chunk.DocumentChunkId,
                    DocumentName = chunk.DocumentName,
                    ChunkIndex = chunk.ChunkIndex,
                    Excerpt = CreateExcerpt(chunk.Content),
                    SimilarityScore = chunk.SimilarityScore
                }).ToList()
            };
        }

        public async Task<List<ChatSessionDto>> GetUserSessionsAsync(
            int userId)
        {
            return await _context.ChatSessions
                .Where(cs => cs.UserId == userId && !cs.IsDeleted)
                .OrderByDescending(cs => cs.UpdatedAt ?? cs.CreatedAt)
                .Select(cs => new ChatSessionDto
                {
                    ChatSessionId = cs.ChatSessionId,
                    SubjectId = cs.SubjectId,
                    Title = cs.Title,
                    CreatedAt = cs.CreatedAt,
                    UpdatedAt = cs.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<ChatSessionDetailDto?> GetSessionDetailAsync(
            int chatSessionId,
            int userId)
        {
            var session = await _context.ChatSessions
                .Include(cs => cs.Messages)
                .FirstOrDefaultAsync(cs =>
                    cs.ChatSessionId == chatSessionId &&
                    cs.UserId == userId &&
                    !cs.IsDeleted);

            if (session == null)
            {
                return null;
            }

            return new ChatSessionDetailDto
            {
                ChatSessionId = session.ChatSessionId,
                SubjectId = session.SubjectId,
                Title = session.Title,
                Messages = session.Messages
                    .OrderBy(m => m.CreatedAt)
                    .Select(m => new ChatMessageDto
                    {
                        ChatMessageId = m.ChatMessageId,
                        Role = m.Role,
                        Content = m.Content,
                        CreatedAt = m.CreatedAt
                    })
                    .ToList()
            };
        }

        public async Task DeleteSessionAsync(
            int chatSessionId,
            int userId)
        {
            var session = await _context.ChatSessions
                .FirstOrDefaultAsync(cs =>
                    cs.ChatSessionId == chatSessionId &&
                    cs.UserId == userId &&
                    !cs.IsDeleted);

            if (session == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy cuộc trò chuyện.");
            }

            session.IsDeleted = true;
            session.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        private async Task<ChatSession> GetOrCreateSessionAsync(
            ChatAskRequestDto request,
            int userId)
        {
            if (request.ChatSessionId.HasValue)
            {
                var existingSession = await _context.ChatSessions
                    .FirstOrDefaultAsync(cs =>
                        cs.ChatSessionId == request.ChatSessionId.Value &&
                        cs.UserId == userId &&
                        !cs.IsDeleted);

                if (existingSession == null)
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy cuộc trò chuyện.");
                }

                return existingSession;
            }

            var newSession = new ChatSession
            {
                UserId = userId,
                SubjectId = request.SubjectId,
                Title = "Cuộc trò chuyện mới",
                CreatedAt = DateTime.Now
            };

            _context.ChatSessions.Add(newSession);

            await _context.SaveChangesAsync();

            return newSession;
        }

        private async Task<List<ScoredChunk>> FindRelevantChunksAsync(
            float[] questionEmbedding,
            int? subjectId)
        {
            var query = _context.DocumentChunks
                .Include(chunk => chunk.Document)
                .Where(chunk =>
                    chunk.Document != null &&
                    chunk.Document.Status == "Approved" &&
                    !string.IsNullOrWhiteSpace(chunk.EmbeddingJson));

            if (subjectId.HasValue)
            {
                query = query.Where(chunk =>
                    chunk.Document != null &&
                    chunk.Document.SubjectId == subjectId.Value);
            }

            var chunks = await query
                .Select(chunk => new
                {
                    chunk.DocumentChunkId,
                    chunk.ChunkIndex,
                    chunk.Content,
                    chunk.EmbeddingJson,
                    DocumentName = chunk.Document != null
                        ? chunk.Document.OriginalFileName
                        : null
                })
                .ToListAsync();

            var scoredChunks = new List<ScoredChunk>();

            foreach (var chunk in chunks)
            {
                var chunkEmbedding = DeserializeEmbedding(
                    chunk.EmbeddingJson);

                if (chunkEmbedding.Length == 0)
                {
                    continue;
                }

                var similarity = CosineSimilarity(
                    questionEmbedding,
                    chunkEmbedding);

                if (similarity < MinimumSimilarityScore)
                {
                    continue;
                }

                scoredChunks.Add(new ScoredChunk
                {
                    DocumentChunkId = chunk.DocumentChunkId,
                    ChunkIndex = chunk.ChunkIndex,
                    Content = chunk.Content,
                    DocumentName = chunk.DocumentName,
                    SimilarityScore = similarity
                });
            }

            return scoredChunks
                .OrderByDescending(chunk => chunk.SimilarityScore)
                .Take(TopK)
                .ToList();
        }

        private async Task<List<ChatMessageDto>> GetRecentMessagesAsync(
            int chatSessionId)
        {
            var messages = await _context.ChatMessages
                .Where(message => message.ChatSessionId == chatSessionId)
                .OrderByDescending(message => message.CreatedAt)
                .Take(RecentMessageCount)
                .Select(message => new ChatMessageDto
                {
                    ChatMessageId = message.ChatMessageId,
                    Role = message.Role,
                    Content = message.Content,
                    CreatedAt = message.CreatedAt
                })
                .ToListAsync();

            return messages
                .OrderBy(message => message.CreatedAt)
                .ToList();
        }

        private string BuildPrompt(
            string question,
            List<ScoredChunk> relevantChunks,
            List<ChatMessageDto> recentMessages)
        {
            var builder = new StringBuilder();

            builder.AppendLine("Bạn là chatbot hỗ trợ học tập cho sinh viên.");
            builder.AppendLine("Nhiệm vụ của bạn là trả lời câu hỏi dựa trên CONTEXT tài liệu bên dưới.");
            builder.AppendLine();
            builder.AppendLine("Quy tắc bắt buộc:");
            builder.AppendLine("1. Chỉ sử dụng thông tin có trong CONTEXT.");
            builder.AppendLine("2. Không được bịa thêm kiến thức ngoài tài liệu.");
            builder.AppendLine("3. Nếu CONTEXT không đủ thông tin, hãy nói: \"Tôi không tìm thấy thông tin này trong tài liệu.\"");
            builder.AppendLine("4. Trả lời bằng tiếng Việt, rõ ràng, dễ hiểu.");
            builder.AppendLine("5. Cuối câu trả lời nên có phần \"Nguồn tham khảo\" dựa trên tên file/chunk được cung cấp.");
            builder.AppendLine();

            builder.AppendLine("===== LỊCH SỬ HỘI THOẠI GẦN ĐÂY =====");

            if (recentMessages.Count == 0)
            {
                builder.AppendLine("Không có lịch sử trước đó.");
            }
            else
            {
                foreach (var message in recentMessages)
                {
                    builder.AppendLine($"{message.Role}: {message.Content}");
                }
            }

            builder.AppendLine();
            builder.AppendLine("===== CONTEXT TÀI LIỆU =====");

            for (var i = 0; i < relevantChunks.Count; i++)
            {
                var chunk = relevantChunks[i];

                builder.AppendLine($"[Nguồn {i + 1}]");
                builder.AppendLine($"File: {chunk.DocumentName}");
                builder.AppendLine($"ChunkIndex: {chunk.ChunkIndex}");
                builder.AppendLine($"SimilarityScore: {chunk.SimilarityScore:F4}");
                builder.AppendLine(chunk.Content);
                builder.AppendLine();
            }

            builder.AppendLine("===== CÂU HỎI HIỆN TẠI =====");
            builder.AppendLine(question);
            builder.AppendLine();

            builder.AppendLine("===== CÂU TRẢ LỜI =====");

            return builder.ToString();
        }

        private static float[] DeserializeEmbedding(string embeddingJson)
        {
            if (string.IsNullOrWhiteSpace(embeddingJson))
            {
                return Array.Empty<float>();
            }

            try
            {
                var floatArray = JsonSerializer.Deserialize<float[]>(
                    embeddingJson);

                if (floatArray != null && floatArray.Length > 0)
                {
                    return floatArray;
                }
            }
            catch
            {
                // Ignore and try double[] below.
            }

            try
            {
                var doubleArray = JsonSerializer.Deserialize<double[]>(
                    embeddingJson);

                if (doubleArray != null && doubleArray.Length > 0)
                {
                    return doubleArray
                        .Select(value => (float)value)
                        .ToArray();
                }
            }
            catch
            {
                // Ignore invalid embedding json.
            }

            return Array.Empty<float>();
        }

        private static double CosineSimilarity(
            float[] vectorA,
            float[] vectorB)
        {
            if (vectorA.Length == 0 ||
                vectorB.Length == 0 ||
                vectorA.Length != vectorB.Length)
            {
                return 0;
            }

            double dotProduct = 0;
            double magnitudeA = 0;
            double magnitudeB = 0;

            for (var i = 0; i < vectorA.Length; i++)
            {
                dotProduct += vectorA[i] * vectorB[i];
                magnitudeA += vectorA[i] * vectorA[i];
                magnitudeB += vectorB[i] * vectorB[i];
            }

            if (magnitudeA == 0 || magnitudeB == 0)
            {
                return 0;
            }

            return dotProduct /
                   (Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
        }

        private static string CreateExcerpt(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return string.Empty;
            }

            content = content.Trim();

            if (content.Length <= 300)
            {
                return content;
            }

            return content.Substring(0, 300) + "...";
        }

        private static string GenerateTitle(string question)
        {
            question = question.Trim();

            if (question.Length <= 50)
            {
                return question;
            }

            return question.Substring(0, 50) + "...";
        }

        private class ScoredChunk
        {
            public int DocumentChunkId { get; set; }

            public int ChunkIndex { get; set; }

            public string Content { get; set; } = string.Empty;

            public string? DocumentName { get; set; }

            public double SimilarityScore { get; set; }
        }
    }
}

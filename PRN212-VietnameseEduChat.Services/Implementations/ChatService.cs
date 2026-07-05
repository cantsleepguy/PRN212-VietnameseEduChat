using Microsoft.EntityFrameworkCore;
using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Chats;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.DataAccess.Context;
using PRN212_VietnameseEduChat.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _context;

        public ChatService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ChatAskResponseDto> AskAsync(
            ChatAskRequestDto request,
            int userId)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                throw new InvalidOperationException("Câu hỏi không được để trống.");
            }

            var chatSession = await GetOrCreateSessionAsync(request, userId);

            var userMessage = new ChatMessage
            {
                ChatSessionId = chatSession.ChatSessionId,
                Role = "User",
                Content = request.Question.Trim(),
                CreatedAt = DateTime.Now
            };

            _context.ChatMessages.Add(userMessage);

            var answer = await GenerateTemporaryAnswerAsync(request.Question);

            var assistantMessage = new ChatMessage
            {
                ChatSessionId = chatSession.ChatSessionId,
                Role = "Assistant",
                Content = answer,
                CreatedAt = DateTime.Now
            };

            _context.ChatMessages.Add(assistantMessage);

            chatSession.UpdatedAt = DateTime.Now;

            if (chatSession.Title == "Cuộc trò chuyện mới")
            {
                chatSession.Title = GenerateTitle(request.Question);
            }

            await _context.SaveChangesAsync();

            return new ChatAskResponseDto
            {
                ChatSessionId = chatSession.ChatSessionId,
                Answer = answer,
                Sources = new List<ChatSourceDto>()
            };
        }

        public async Task<List<ChatSessionDto>> GetUserSessionsAsync(int userId)
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
                throw new InvalidOperationException("Không tìm thấy cuộc trò chuyện.");
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
                    throw new InvalidOperationException("Không tìm thấy cuộc trò chuyện.");
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

        private Task<string> GenerateTemporaryAnswerAsync(string question)
        {
            var answer =
                "Đây là câu trả lời tạm thời. " +
                "Ở bước tiếp theo, hệ thống sẽ dùng embedding để tìm các đoạn tài liệu liên quan, " +
                "sau đó gọi AI để trả lời dựa trên nội dung tài liệu.";

            return Task.FromResult(answer);
        }

        private string GenerateTitle(string question)
        {
            question = question.Trim();

            if (question.Length <= 50)
            {
                return question;
            }

            return question.Substring(0, 50) + "...";
        }
    }
}

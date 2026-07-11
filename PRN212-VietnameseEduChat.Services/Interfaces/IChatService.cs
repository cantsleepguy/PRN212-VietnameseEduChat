using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Chats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Interfaces
{
    public interface IChatService
    {
        Task<ChatAskResponseDto> AskAsync(
            ChatAskRequestDto request,
            int userId);

        IAsyncEnumerable<ChatStreamEventDto> AskStreamAsync(
            ChatAskRequestDto request,
            int userId,
            CancellationToken cancellationToken = default);

        Task<List<ChatSessionDto>> GetUserSessionsAsync(
            int userId);

        Task<ChatSessionDetailDto?> GetSessionDetailAsync(
            int chatSessionId,
            int userId);

        Task DeleteSessionAsync(
            int chatSessionId,
            int userId);
    }
}

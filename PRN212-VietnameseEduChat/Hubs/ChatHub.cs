using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Chats;
using PRN212_VietnameseEduChat.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public async IAsyncEnumerable<ChatStreamEventDto> AskStream(
            ChatAskRequestDto request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (!int.TryParse(Context.UserIdentifier, out var userId))
            {
                yield return new ChatStreamEventDto
                {
                    Type = "Error",
                    Message = "Không xác định được người dùng hiện tại."
                };

                yield break;
            }

            await foreach (var streamEvent in _chatService.AskStreamAsync(
                request,
                userId,
                cancellationToken))
            {
                yield return streamEvent;
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace PRN212_VietnameseEduChat.Hubs
{
    [Authorize]
    public class SubjectHub : Hub
    {
    }
}

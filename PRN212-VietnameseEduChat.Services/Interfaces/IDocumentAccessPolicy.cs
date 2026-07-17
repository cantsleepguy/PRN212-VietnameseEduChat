using System.Security.Claims;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;

namespace PRN212_VietnameseEduChat.Services.Interfaces;

public interface IDocumentAccessPolicy
{
    Task<bool> CanReadAsync(
        Document document,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default);
}

using System.Security.Claims;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Services.Interfaces;
using PRN212_VietnameseEduChat.Services.Security;

namespace PRN212_VietnameseEduChat.Services.Implementations;

public sealed class DocumentAccessPolicy : IDocumentAccessPolicy
{
    private readonly ISubjectLecturerService _subjectLecturerService;

    public DocumentAccessPolicy(ISubjectLecturerService subjectLecturerService)
    {
        _subjectLecturerService = subjectLecturerService;
    }

    public async Task<bool> CanReadAsync(
        Document document,
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (user.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        if (user.IsInRole(AppRoles.SystemAdmin) ||
            user.IsInRole(AppRoles.AcademicAdmin))
        {
            return true;
        }

        if (user.IsInRole(AppRoles.Student))
        {
            return document.Status == "Approved";
        }

        if (!user.IsInRole(AppRoles.Lecturer) ||
            !int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return false;
        }

        if (document.UploadedBy == userId)
        {
            return true;
        }

        return document.SubjectId.HasValue &&
               await _subjectLecturerService.IsLecturerAssignedAsync(
                   document.SubjectId.Value,
                   userId);
    }
}

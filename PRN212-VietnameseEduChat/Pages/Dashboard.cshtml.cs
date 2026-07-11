using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Services.Interfaces;
using PRN212_VietnameseEduChat.Services.Security;
using System.Security.Claims;

namespace PRN212_VietnameseEduChat.Pages;

[Authorize]
public class DashboardModel : PageModel
{
    private readonly ISubjectLecturerService _subjectLecturerService;

    public DashboardModel(ISubjectLecturerService subjectLecturerService)
    {
        _subjectLecturerService = subjectLecturerService;
    }

    public List<Subject> ManagedSubjects { get; set; } = new();

    public async Task OnGetAsync()
    {
        if (!User.IsInRole(AppRoles.Lecturer))
        {
            return;
        }

        var lecturerIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(lecturerIdValue, out var lecturerId))
        {
            return;
        }

        ManagedSubjects = await _subjectLecturerService
            .GetAssignedSubjectsAsync(lecturerId);

        foreach (var subject in ManagedSubjects)
        {
            subject.Chapters = subject.Chapters
                .OrderBy(x => x.OrderIndex)
                .ToList();

            subject.Documents = subject.Documents
                .OrderByDescending(x => x.UploadedAt)
                .ToList();
        }
    }
}

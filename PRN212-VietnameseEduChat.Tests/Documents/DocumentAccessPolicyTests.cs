using System.Security.Claims;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Services.Implementations;
using PRN212_VietnameseEduChat.Services.Interfaces;
using PRN212_VietnameseEduChat.Services.Security;

namespace PRN212_VietnameseEduChat.Tests.Documents;

public sealed class DocumentAccessPolicyTests
{
    [Theory]
    [InlineData(AppRoles.SystemAdmin)]
    [InlineData(AppRoles.AcademicAdmin)]
    public async Task Admin_can_read_any_document(string role)
    {
        var policy = new DocumentAccessPolicy(new AssignmentService());
        var document = new Document { UploadedBy = 99, Status = "Rejected" };

        Assert.True(await policy.CanReadAsync(document, User(1, role)));
    }

    [Fact]
    public async Task Lecturer_can_read_owned_document()
    {
        var policy = new DocumentAccessPolicy(new AssignmentService());
        var document = new Document { UploadedBy = 7 };

        Assert.True(await policy.CanReadAsync(document, User(7, AppRoles.Lecturer)));
    }

    [Fact]
    public async Task Lecturer_can_read_assigned_subject_document()
    {
        var policy = new DocumentAccessPolicy(new AssignmentService((12, 7)));
        var document = new Document { UploadedBy = 99, SubjectId = 12 };

        Assert.True(await policy.CanReadAsync(document, User(7, AppRoles.Lecturer)));
        Assert.False(await policy.CanReadAsync(document, User(8, AppRoles.Lecturer)));
    }

    [Theory]
    [InlineData("Approved", true)]
    [InlineData("PendingApproval", false)]
    [InlineData("Rejected", false)]
    public async Task Student_can_read_only_approved_document(string status, bool expected)
    {
        var policy = new DocumentAccessPolicy(new AssignmentService());
        var document = new Document { Status = status };

        Assert.Equal(expected, await policy.CanReadAsync(document, User(3, AppRoles.Student)));
    }

    [Fact]
    public async Task Unauthenticated_user_cannot_read_document()
    {
        var policy = new DocumentAccessPolicy(new AssignmentService());

        Assert.False(await policy.CanReadAsync(new Document { Status = "Approved" }, new ClaimsPrincipal()));
    }

    private static ClaimsPrincipal User(int id, string role) =>
        new(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, id.ToString()),
            new Claim(ClaimTypes.Role, role)
        ], "Test"));

    private sealed class AssignmentService(params (int SubjectId, int LecturerId)[] assignments)
        : ISubjectLecturerService
    {
        public Task<bool> IsLecturerAssignedAsync(int subjectId, int lecturerId) =>
            Task.FromResult(assignments.Contains((subjectId, lecturerId)));

        public Task<List<User>> GetLecturersAsync() => throw new NotSupportedException();
        public Task<List<Subject>> GetAssignedSubjectsAsync(int lecturerId) => throw new NotSupportedException();
        public Task AssignAsync(int subjectId, int lecturerId, int assignedBy) => throw new NotSupportedException();
        public Task UnassignAsync(int subjectId, int lecturerId) => throw new NotSupportedException();
    }
}

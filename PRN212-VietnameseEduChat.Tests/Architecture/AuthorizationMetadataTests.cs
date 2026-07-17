using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using PRN212_VietnameseEduChat.Services.Security;

namespace PRN212_VietnameseEduChat.Tests.Architecture;

public sealed class AuthorizationMetadataTests
{
    [Theory]
    [InlineData(typeof(Pages.Documents.UploadModel), AppRoles.Lecturer)]
    [InlineData(typeof(Pages.ResearchQuestions.IndexModel), AppRoles.AcademicAdmin)]
    [InlineData(typeof(Pages.Admin.Users.IndexModel), AppRoles.SystemAdmin)]
    public void Sensitive_page_requires_expected_role(
        Type pageType,
        string expectedRole)
    {
        var attribute = pageType.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
        Assert.Contains(
            expectedRole,
            attribute!.Roles!.Split(
                ',',
                StringSplitOptions.TrimEntries |
                StringSplitOptions.RemoveEmptyEntries));
    }

    [Theory]
    [InlineData(typeof(Pages.DashboardModel))]
    [InlineData(typeof(Pages.Chat.IndexModel))]
    [InlineData(typeof(Pages.Documents.IndexModel))]
    public void Authenticated_page_requires_authorization(Type pageType)
    {
        var attribute = pageType.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attribute);
    }
}

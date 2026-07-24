using PRN212_VietnameseEduChat.BusinessObjects.Constants;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.DataAccess.Context;
using PRN212_VietnameseEduChat.Repositories.Implementations;
using PRN212_VietnameseEduChat.Tests.TestInfrastructure;

namespace PRN212_VietnameseEduChat.Tests.Payments;

public sealed class PaymentClaimTests
{
    [Fact]
    public async Task Pending_payment_can_be_claimed_only_once()
    {
        await using var database = SqlServerTestDatabase.Create();
        await database.CreateEmptyDatabaseAsync();

        var options = database.CreateOptions();
        await using var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var role = new Role { RoleName = "Student" };
        var user = new User { Email = "student@test.local", FullName = "Student", Password = "hashed", Role = role };
        var package = new Package
        {
            PackageCode = "TEST",
            PackageName = "Test package",
            Price = 10000,
            DurationDays = 30,
            MaxUploadSizeMb = 25
        };
        var payment = new Payment
        {
            User = user,
            Package = package,
            TransactionId = "PAY-CLAIM-1",
            PackageCodeSnapshot = package.PackageCode,
            PackageNameSnapshot = package.PackageName,
            PackageDurationDaysSnapshot = 30,
            Currency = "VND",
            Provider = PaymentProviders.VnPay,
            Status = PaymentStatuses.Pending,
            Amount = 10000,
            CreatedAt = DateTime.UtcNow
        };
        context.Add(payment);
        await context.SaveChangesAsync();
        var repository = new PaymentRepository(context);

        var first = await repository.TryClaimPendingAsync(payment.PaymentId);
        var second = await repository.TryClaimPendingAsync(payment.PaymentId);

        Assert.True(first);
        Assert.False(second);
    }
}

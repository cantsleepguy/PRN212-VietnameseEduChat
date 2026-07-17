using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PRN212_VietnameseEduChat.BusinessObjects.Constants;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.DataAccess.Context;
using PRN212_VietnameseEduChat.Repositories.Implementations;

namespace PRN212_VietnameseEduChat.Tests.Payments;

public sealed class PaymentClaimTests
{
    [Fact]
    public async Task Pending_payment_can_be_claimed_only_once()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var context = new TestDbContext(options);
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
            CreatedAt = DateTime.UtcNow,
            RowVersion = new byte[8]
        };
        context.Add(payment);
        await context.SaveChangesAsync();
        var repository = new PaymentRepository(context);

        var first = await repository.TryClaimPendingAsync(payment.PaymentId);
        var second = await repository.TryClaimPendingAsync(payment.PaymentId);

        Assert.True(first);
        Assert.False(second);
    }

    private sealed class TestDbContext(DbContextOptions<ApplicationDbContext> options)
        : ApplicationDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Payment>()
                .Property(x => x.RowVersion)
                .IsConcurrencyToken()
                .ValueGeneratedNever();
        }
    }
}

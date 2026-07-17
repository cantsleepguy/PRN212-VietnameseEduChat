using PRN212_VietnameseEduChat.Options;

namespace PRN212_VietnameseEduChat.Tests.Startup;

public sealed class StartupPolicyTests
{
    [Theory]
    [InlineData("Development", true, true)]
    [InlineData("Development", false, false)]
    [InlineData("Production", false, false)]
    public void Auto_migrate_follows_explicit_option(
        string environment,
        bool enabled,
        bool expected)
    {
        Assert.Equal(
            expected,
            StartupPolicy.ShouldAutoMigrate(
                environment,
                new DatabaseStartupOptions { AutoMigrate = enabled }));
    }

    [Theory]
    [InlineData("Development", true, true)]
    [InlineData("Development", false, false)]
    [InlineData("Production", false, false)]
    public void Demo_seed_follows_explicit_option(
        string environment,
        bool enabled,
        bool expected)
    {
        Assert.Equal(
            expected,
            StartupPolicy.ShouldSeedDemoData(
                environment,
                new DemoDataOptions { Enabled = enabled }));
    }
}

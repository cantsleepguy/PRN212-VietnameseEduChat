namespace PRN212_VietnameseEduChat.Options;

public static class StartupPolicy
{
    public static bool ShouldAutoMigrate(
        string environmentName,
        DatabaseStartupOptions options)
    {
        _ = environmentName;
        return options.AutoMigrate;
    }

    public static bool ShouldSeedDemoData(
        string environmentName,
        DemoDataOptions options)
    {
        _ = environmentName;
        return options.Enabled;
    }
}

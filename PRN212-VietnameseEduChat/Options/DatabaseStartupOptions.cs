namespace PRN212_VietnameseEduChat.Options;

public sealed class DatabaseStartupOptions
{
    public const string SectionName = "Database";
    public bool AutoMigrate { get; set; }
}

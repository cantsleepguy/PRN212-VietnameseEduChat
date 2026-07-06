using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.DataAccess.Context;
using PRN212_VietnameseEduChat.Repositories.Implementations;
using PRN212_VietnameseEduChat.Repositories.Interfaces;
using PRN212_VietnameseEduChat.Services.Implementations;
using PRN212_VietnameseEduChat.Services.Interfaces;
using ImageMagick;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();

builder.Services.AddScoped<
    IPasswordHasher<User>,
    PasswordHasher<User>>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/AccessDenied";
        options.Cookie.Name = "VietnameseEduChat.Auth";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });

builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();

builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<ISubjectService, SubjectService>();

builder.Services.AddScoped<IChapterRepository, ChapterRepository>();
builder.Services.AddScoped<IChapterService, ChapterService>();

builder.Services.AddScoped<ISubjectLecturerRepository, SubjectLecturerRepository>();
builder.Services.AddScoped<ISubjectLecturerService, SubjectLecturerService>();

builder.Services.AddScoped<IOcrService, OcrService>();
builder.Services.AddScoped<ITextExtractorService, TextExtractorService>();
builder.Services.AddScoped<IChunkService, ChunkService>();

builder.Services.AddHttpClient<IEmbeddingService, OpenAIEmbeddingService>();

builder.Services.AddHttpClient<IChatCompletionService, OpenAIChatCompletionService>();

builder.Services.AddScoped<IChatService, ChatService>();

builder.Services.AddScoped<IResearchQuestionService, ResearchQuestionService>();

builder.Services.AddScoped<IResearchBenchmarkService, ResearchBenchmarkService>();

builder.Services.AddScoped<IResearchChunkingService, ResearchChunkingService>();

builder.Services.AddScoped<IResearchIndexService, ResearchIndexService>();

var ghostscriptDirectory = @"C:\Program Files\gs\gs10.07.1\bin";

if (Directory.Exists(ghostscriptDirectory))
{
    MagickNET.SetGhostscriptDirectory(ghostscriptDirectory);
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
                  .GetRequiredService<ApplicationDbContext>();

    db.Database.Migrate();

    var seeder = scope.ServiceProvider
                      .GetRequiredService<IDatabaseSeeder>();

    await seeder.SeedAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", context =>
{
    context.Response.Redirect("/Login");
    return Task.CompletedTask;
});

app.MapRazorPages();

app.Run();

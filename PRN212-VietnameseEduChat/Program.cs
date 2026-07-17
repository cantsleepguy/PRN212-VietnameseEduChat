using ImageMagick;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.DataAccess.Context;
using PRN212_VietnameseEduChat.Repositories.Implementations;
using PRN212_VietnameseEduChat.Repositories.Interfaces;
using PRN212_VietnameseEduChat.Services.Implementations;
using PRN212_VietnameseEduChat.Services.Interfaces;
using System.Security.Claims;
using PRN212_VietnameseEduChat.Services.Options;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found.");

builder.Services.AddRazorPages();

builder.Services.AddSignalR();

builder.Services.Configure<
    Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit =
            200 * 1024 * 1024;
    });

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize =
        200 * 1024 * 1024;
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddScoped<
    IDatabaseSeeder,
    DatabaseSeeder>();

builder.Services.AddScoped<
    IPasswordHasher<User>,
    PasswordHasher<User>>();

builder.Services.AddScoped<
    IUserRepository,
    UserRepository>();

builder.Services.AddScoped<
    IRoleRepository,
    RoleRepository>();

builder.Services.AddScoped<
    IAuthService,
    AuthService>();

builder.Services.AddScoped<
    IUserManagementService,
    UserManagementService>();

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/AccessDenied";
        options.Cookie.Name = "VietnameseEduChat.Auth";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;

        options.Events.OnValidatePrincipal = async context =>
        {
            var userIdText = context.Principal?
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdText, out var userId))
            {
                context.RejectPrincipal();

                await context.HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return;
            }

            var userRepository = context.HttpContext
                .RequestServices
                .GetRequiredService<IUserRepository>();

            var user = await userRepository
                .GetByIdAsync(userId);

            if (user == null || user.IsLocked)
            {
                context.RejectPrincipal();

                await context.HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });

builder.Services.AddScoped<
    IDocumentRepository,
    DocumentRepository>();

builder.Services.AddScoped<
    IDocumentService,
    DocumentService>();

builder.Services
    .AddOptions<DocumentStorageOptions>()
    .Bind(builder.Configuration.GetSection(DocumentStorageOptions.SectionName));

builder.Services.AddSingleton<
    IDocumentFileValidator,
    DocumentFileValidator>();

builder.Services.AddSingleton<
    IDocumentStorage,
    LocalDocumentStorage>();

builder.Services.AddScoped<
    IDocumentAccessPolicy,
    DocumentAccessPolicy>();

builder.Services.AddScoped<
    ISubjectRepository,
    SubjectRepository>();

builder.Services.AddScoped<
    ISubjectService,
    SubjectService>();

builder.Services.AddScoped<
    IChapterRepository,
    ChapterRepository>();

builder.Services.AddScoped<
    IChapterService,
    ChapterService>();

builder.Services.AddScoped<
    ISubjectLecturerRepository,
    SubjectLecturerRepository>();

builder.Services.AddScoped<
    ISubjectLecturerService,
    SubjectLecturerService>();

builder.Services.AddScoped<
    IOcrService,
    OcrService>();

builder.Services.AddScoped<
    ITextExtractorService,
    TextExtractorService>();

builder.Services.AddScoped<
    IChunkService,
    ChunkService>();

builder.Services.AddHttpClient<
    IEmbeddingService,
    OpenAIEmbeddingService>();

builder.Services.AddHttpClient<
    IChatCompletionService,
    OpenAIChatCompletionService>();

builder.Services.AddScoped<
    IChatService,
    ChatService>();

builder.Services.AddScoped<
    IChunkingConfigurationRepository,
    ChunkingConfigurationRepository>();

builder.Services.AddScoped<
    IChunkingConfigurationService,
    ChunkingConfigurationService>();

builder.Services.AddScoped<
    IPackageRepository,
    PackageRepository>();

builder.Services.AddScoped<
    IPackageService,
    PackageService>();

builder.Services.AddScoped<
    IUserSubscriptionRepository,
    UserSubscriptionRepository>();

builder.Services.AddScoped<
    ISubscriptionService,
    SubscriptionService>();

builder.Services.AddScoped<
    IPaymentRepository,
    PaymentRepository>();

builder.Services.AddScoped<
    IPaymentQuoteService,
    PaymentQuoteService>();

builder.Services
    .AddOptions<VnPaySettings>()
    .Bind(builder.Configuration.GetSection("VnPay"))
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(settings.PaymentUrl),
        "Thiếu VnPay:PaymentUrl")
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(settings.TmnCode),
        "Thiếu VnPay:TmnCode")
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(settings.HashSecret),
        "Thiếu VnPay:HashSecret")
    .Validate(
        settings =>
            !string.IsNullOrWhiteSpace(settings.PublicBaseUrl),
        "Thiếu VnPay:PublicBaseUrl")
    .ValidateOnStart();

builder.Services.AddScoped<
    IPaymentProvider,
    VnPayPaymentProvider>();

builder.Services.AddScoped<
    IPaymentService,
    PaymentService>();

builder.Services.AddScoped<
    IDashboardService,
    DashboardService>();

builder.Services.AddScoped<
    IResearchQuestionService,
    ResearchQuestionService>();

builder.Services.AddScoped<
    IResearchBenchmarkService,
    ResearchBenchmarkService>();

builder.Services.AddScoped<
    IResearchChunkingService,
    ResearchChunkingService>();

builder.Services.AddScoped<
    IResearchIndexService,
    ResearchIndexService>();

var ghostscriptDirectory =
    @"C:\Program Files\gs\gs10.07.1\bin";

if (Directory.Exists(ghostscriptDirectory))
{
    MagickNET.SetGhostscriptDirectory(
        ghostscriptDirectory);
}

builder.Services.Configure<ForwardedHeadersOptions>(
    options =>
    {
        options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor |
            ForwardedHeaders.XForwardedProto |
            ForwardedHeaders.XForwardedHost;

        /*
         * Chỉ phù hợp cho môi trường Development dùng ngrok.
         * Cho phép proxy động của ngrok gửi forwarded headers.
         */
        if (builder.Environment.IsDevelopment())
        {
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        }
    });

var app = builder.Build();

app.UseForwardedHeaders();

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

app.MapGet(
    "/api/payments/vnpay-ipn",
    async (
        HttpRequest request,
        IPaymentService paymentService,
        ILogger<Program> logger,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var callbackValues = request.Query
                .ToDictionary(
                    item => item.Key,
                    item => item.Value.ToString(),
                    StringComparer.OrdinalIgnoreCase);

            var result =
                await paymentService.ProcessVnPayIpnAsync(
                    callbackValues,
                    cancellationToken);

            return Results.Json(new
            {
                RspCode = result.RspCode,
                Message = result.Message
            });
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Lỗi ngoài dự kiến khi xử lý IPN VNPay.");

            return Results.Json(new
            {
                RspCode = "99",
                Message = "Unknown error"
            });
        }
    })
    .AllowAnonymous();

app.MapHub<
    PRN212_VietnameseEduChat.Hubs.ChatHub>(
    "/hubs/chat");

app.MapHub<
    PRN212_VietnameseEduChat.Hubs.SubjectHub>(
    "/hubs/subjects");

app.Run();

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;

namespace PRN212_VietnameseEduChat.DataAccess.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

        public DbSet<Role> Roles => Set<Role>();

        public DbSet<Document> Documents => Set<Document>();

        public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();

        public DbSet<Subject> Subjects => Set<Subject>();

        public DbSet<Chapter> Chapters => Set<Chapter>();

        public DbSet<SubjectLecturer> SubjectLecturers =>
            Set<SubjectLecturer>();

        public DbSet<ChatSession> ChatSessions => Set<ChatSession>();

        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

        public DbSet<ChatMessageSource> ChatMessageSources =>
            Set<ChatMessageSource>();

        public DbSet<ResearchQuestion> ResearchQuestions =>
            Set<ResearchQuestion>();

        public DbSet<ResearchExperiment> ResearchExperiments =>
            Set<ResearchExperiment>();

        public DbSet<ResearchResult> ResearchResults =>
            Set<ResearchResult>();

        public DbSet<ResearchDocumentChunk> ResearchDocumentChunks =>
            Set<ResearchDocumentChunk>();

        public DbSet<ChunkingConfiguration> ChunkingConfigurations =>
            Set<ChunkingConfiguration>();

        public DbSet<Package> Packages => Set<Package>();

        public DbSet<UserSubscription> UserSubscriptions =>
            Set<UserSubscription>();

        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(ApplicationDbContext).Assembly);

            modelBuilder.Entity<ChatSession>()
                        .HasKey(cs => cs.ChatSessionId);

            modelBuilder.Entity<ChatSession>()
                        .Property(cs => cs.Title)
                        .HasMaxLength(255)
                        .IsRequired();

            modelBuilder.Entity<ChatSession>()
                        .HasOne(cs => cs.User)
                        .WithMany()
                        .HasForeignKey(cs => cs.UserId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatSession>()
                        .HasOne(cs => cs.Subject)
                        .WithMany()
                        .HasForeignKey(cs => cs.SubjectId)
                        .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ChatMessage>()
                        .HasKey(cm => cm.ChatMessageId);

            modelBuilder.Entity<ChatMessage>()
                        .Property(cm => cm.Role)
                        .HasMaxLength(20)
                        .IsRequired();

            modelBuilder.Entity<ChatMessage>()
                        .Property(cm => cm.Content)
                        .IsRequired();

            modelBuilder.Entity<ChatMessage>()
                        .HasOne(cm => cm.ChatSession)
                        .WithMany(cs => cs.Messages)
                        .HasForeignKey(cm => cm.ChatSessionId)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatMessageSource>()
                        .HasKey(cms => cms.ChatMessageSourceId);

            modelBuilder.Entity<ChatMessageSource>()
                        .Property(cms => cms.Excerpt)
                        .HasMaxLength(1000);

            modelBuilder.Entity<ChatMessageSource>()
                        .HasOne(cms => cms.ChatMessage)
                        .WithMany(cm => cm.Sources)
                        .HasForeignKey(cms => cms.ChatMessageId)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatMessageSource>()
                        .HasOne(cms => cms.DocumentChunk)
                        .WithMany()
                        .HasForeignKey(cms => cms.DocumentChunkId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ResearchQuestion>()
                        .HasKey(rq => rq.ResearchQuestionId);

            modelBuilder.Entity<ResearchQuestion>()
                        .Property(rq => rq.Question)
                        .IsRequired();

            modelBuilder.Entity<ResearchQuestion>()
                        .Property(rq => rq.GroundTruthAnswer)
                        .IsRequired();

            modelBuilder.Entity<ResearchQuestion>()
                        .HasOne(rq => rq.Subject)
                        .WithMany()
                        .HasForeignKey(rq => rq.SubjectId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ResearchQuestion>()
                        .HasOne(rq => rq.Chapter)
                        .WithMany()
                        .HasForeignKey(rq => rq.ChapterId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ResearchExperiment>()
                        .HasKey(re => re.ResearchExperimentId);

            modelBuilder.Entity<ResearchExperiment>()
                        .Property(re => re.ExperimentName)
                        .HasMaxLength(255)
                        .IsRequired();

            modelBuilder.Entity<ResearchExperiment>()
                        .Property(re => re.ExperimentType)
                        .HasMaxLength(50)
                        .IsRequired();

            modelBuilder.Entity<ResearchExperiment>()
                        .Property(re => re.Status)
                        .HasMaxLength(50)
                        .IsRequired();

            modelBuilder.Entity<ResearchResult>()
                        .HasKey(rr => rr.ResearchResultId);

            modelBuilder.Entity<ResearchResult>()
                        .HasOne(rr => rr.ResearchExperiment)
                        .WithMany(re => re.Results)
                        .HasForeignKey(rr => rr.ResearchExperimentId)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ResearchResult>()
                        .HasOne(rr => rr.ResearchQuestion)
                        .WithMany(rq => rq.Results)
                        .HasForeignKey(rr => rr.ResearchQuestionId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ResearchDocumentChunk>()
                        .HasKey(x => x.ResearchDocumentChunkId);

            modelBuilder.Entity<ResearchDocumentChunk>()
                        .Property(x => x.ChunkingStrategyKey)
                        .HasMaxLength(100)
                        .IsRequired();

            modelBuilder.Entity<ResearchDocumentChunk>()
                        .Property(x => x.ChunkingStrategyName)
                        .HasMaxLength(255)
                        .IsRequired();

            modelBuilder.Entity<ResearchDocumentChunk>()
                        .Property(x => x.EmbeddingProvider)
                        .HasMaxLength(100)
                        .IsRequired();

            modelBuilder.Entity<ResearchDocumentChunk>()
                        .Property(x => x.EmbeddingModelName)
                        .HasMaxLength(255)
                        .IsRequired();

            modelBuilder.Entity<ResearchDocumentChunk>()
                        .HasOne(x => x.Document)
                        .WithMany()
                        .HasForeignKey(x => x.DocumentId)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ResearchDocumentChunk>()
                        .HasIndex(x => new
                        {
                            x.DocumentId,
                            x.ChunkingStrategyKey,
                            x.EmbeddingModelName,
                            x.ChunkIndex
                        })
                        .IsUnique();

            modelBuilder.Entity<ResearchExperiment>()
                        .Property(x => x.ChunkingStrategyKey)
                        .HasMaxLength(100)
                        .HasDefaultValue("fixed-baseline");

            modelBuilder.Entity<ResearchExperiment>()
                        .Property(x => x.EmbeddingProvider)
                        .HasMaxLength(100)
                        .HasDefaultValue("OpenAI");

            modelBuilder.Entity<ChunkingConfiguration>()
                        .HasKey(cc => cc.ChunkingConfigurationId);

            modelBuilder.Entity<ChunkingConfiguration>()
                        .Property(cc => cc.StrategyKey)
                        .HasMaxLength(50)
                        .IsRequired();

            modelBuilder.Entity<ChunkingConfiguration>()
                        .Property(cc => cc.StrategyName)
                        .HasMaxLength(255)
                        .IsRequired();

            modelBuilder.Entity<ChunkingConfiguration>()
                        .Property(cc => cc.FixedSizeUnit)
                        .HasMaxLength(20)
                        .IsRequired();

            modelBuilder.Entity<ChunkingConfiguration>()
                        .HasOne(cc => cc.UpdatedByUser)
                        .WithMany()
                        .HasForeignKey(cc => cc.UpdatedBy)
                        .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Package>()
                        .HasKey(p => p.PackageId);

            modelBuilder.Entity<Package>()
                        .Property(p => p.PackageCode)
                        .HasMaxLength(50)
                        .IsRequired();

            modelBuilder.Entity<Package>()
                        .Property(p => p.PackageName)
                        .HasMaxLength(255)
                        .IsRequired();

            modelBuilder.Entity<Package>()
                        .Property(p => p.Description)
                        .HasMaxLength(1000);

            modelBuilder.Entity<Package>()
                        .Property(p => p.Price)
                        .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Package>()
                        .HasIndex(p => p.PackageCode)
                        .IsUnique();

            modelBuilder.Entity<UserSubscription>()
                        .HasKey(us => us.UserSubscriptionId);

            modelBuilder.Entity<UserSubscription>()
                        .Property(us => us.Status)
                        .HasMaxLength(20)
                        .IsRequired();

            modelBuilder.Entity<UserSubscription>()
                        .HasOne(us => us.User)
                        .WithMany()
                        .HasForeignKey(us => us.UserId)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserSubscription>()
                        .HasOne(us => us.Package)
                        .WithMany(p => p.Subscriptions)
                        .HasForeignKey(us => us.PackageId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                        .HasKey(p => p.PaymentId);

            modelBuilder.Entity<Payment>()
                        .Property(p => p.PaymentType)
                        .HasMaxLength(30)
                        .IsRequired();

            modelBuilder.Entity<Payment>()
                        .Property(p => p.PackageCodeSnapshot)
                        .HasMaxLength(50)
                        .IsRequired();

            modelBuilder.Entity<Payment>()
                        .Property(p => p.PackageNameSnapshot)
                        .HasMaxLength(255)
                        .IsRequired();

            modelBuilder.Entity<Payment>()
                        .Property(p => p.PackagePriceSnapshot)
                        .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Payment>()
                        .Property(p => p.GrossAmount)
                        .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Payment>()
                        .Property(p => p.CreditAmount)
                        .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Payment>()
                        .Property(p => p.Amount)
                        .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Payment>()
                        .Property(p => p.Currency)
                        .HasMaxLength(10)
                        .IsRequired();

            modelBuilder.Entity<Payment>()
                        .Property(p => p.Status)
                        .HasMaxLength(30)
                        .IsRequired();

            modelBuilder.Entity<Payment>()
                        .Property(p => p.TransactionId)
                        .HasMaxLength(100)
                        .IsRequired();

            modelBuilder.Entity<Payment>()
                        .Property(p => p.Provider)
                        .HasMaxLength(30)
                        .IsRequired();

            modelBuilder.Entity<Payment>()
                        .Property(p => p.ProviderPaymentLinkId)
                        .HasMaxLength(100);

            modelBuilder.Entity<Payment>()
                        .Property(p => p.ProviderReference)
                        .HasMaxLength(100);

            modelBuilder.Entity<Payment>()
                        .Property(p => p.CheckoutUrl)
                        .HasMaxLength(1000);

            modelBuilder.Entity<Payment>()
                        .Property(p => p.QrCode)
                        .HasMaxLength(2000);

            modelBuilder.Entity<Payment>()
                        .Property(p => p.BankBin)
                        .HasMaxLength(20);

            modelBuilder.Entity<Payment>()
                        .Property(p => p.BankAccountNumber)
                        .HasMaxLength(50);

            modelBuilder.Entity<Payment>()
                        .Property(p => p.BankAccountName)
                        .HasMaxLength(255);

            modelBuilder.Entity<Payment>()
                        .Property(p => p.TransferDescription)
                        .HasMaxLength(100);

            modelBuilder.Entity<Payment>()
                        .Property(p => p.FailureReason)
                        .HasMaxLength(1000);

            modelBuilder.Entity<Payment>()
                        .Property(p => p.VnPayResponseCode)
                        .HasMaxLength(10);

            modelBuilder.Entity<Payment>()
                        .Property(p => p.VnPayTransactionStatus)
                        .HasMaxLength(10);

            modelBuilder.Entity<Payment>()
                        .Property(p => p.VnPayBankCode)
                        .HasMaxLength(30);

            modelBuilder.Entity<Payment>()
                        .Property(p => p.VnPayCardType)
                        .HasMaxLength(30);

            modelBuilder.Entity<Payment>()
                        .Property(p => p.RowVersion)
                        .IsRowVersion();

            modelBuilder.Entity<Payment>()
                        .HasIndex(p => p.TransactionId)
                        .IsUnique();

            modelBuilder.Entity<Payment>()
                        .HasIndex(p => p.OrderCode)
                        .IsUnique()
                        .HasFilter("[OrderCode] IS NOT NULL");

            modelBuilder.Entity<Payment>()
                        .HasIndex(p => p.ProviderPaymentLinkId)
                        .IsUnique()
                        .HasFilter("[ProviderPaymentLinkId] IS NOT NULL");

            modelBuilder.Entity<Payment>()
                        .HasIndex(p => p.ProviderReference)
                        .IsUnique()
                        .HasFilter("[ProviderReference] IS NOT NULL");

            modelBuilder.Entity<Payment>()
                        .HasOne(p => p.User)
                        .WithMany()
                        .HasForeignKey(p => p.UserId)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Payment>()
                        .HasOne(p => p.Package)
                        .WithMany(pk => pk.Payments)
                        .HasForeignKey(p => p.PackageId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                        .HasOne(p => p.SourceSubscription)
                        .WithMany()
                        .HasForeignKey(p => p.SourceSubscriptionId)
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserSubscription>()
                        .Property(x => x.RowVersion)
                        .IsRowVersion();
        }
    }
}

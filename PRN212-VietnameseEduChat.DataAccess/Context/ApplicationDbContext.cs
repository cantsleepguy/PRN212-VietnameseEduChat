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
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

        public DbSet<Role> Roles => Set<Role>();

        public DbSet<Document> Documents => Set<Document>();

        public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();

        public DbSet<Subject> Subjects => Set<Subject>();

        public DbSet<Chapter> Chapters => Set<Chapter>();

        public DbSet<SubjectLecturer> SubjectLecturers => Set<SubjectLecturer>();

        public DbSet<ChatSession> ChatSessions => Set<ChatSession>();

        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

        public DbSet<ChatMessageSource> ChatMessageSources => Set<ChatMessageSource>();

        public DbSet<ResearchQuestion> ResearchQuestions => Set<ResearchQuestion>();

        public DbSet<ResearchExperiment> ResearchExperiments => Set<ResearchExperiment>();

        public DbSet<ResearchResult> ResearchResults => Set<ResearchResult>();

        public DbSet<ResearchDocumentChunk> ResearchDocumentChunks => Set<ResearchDocumentChunk>();

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
        }
    }
}

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
        }
    }
}

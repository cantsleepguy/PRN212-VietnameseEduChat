using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;

namespace PRN212_VietnameseEduChat.DataAccess.Configurations
{
    public class DocumentConfiguration
        : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            builder.HasKey(x => x.DocumentId);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.OriginalFileName)
                .HasMaxLength(255);

            builder.Property(x => x.StoredFileName)
                .HasMaxLength(255);

            builder.Property(x => x.ContentType)
                .HasMaxLength(100);

            builder.Property(x => x.FilePath)
                .HasMaxLength(500);

            builder.Property(x => x.Status)
                .HasMaxLength(50);

            builder.Property(x => x.ErrorMessage)
                .HasMaxLength(1000);

            builder.Property(x => x.RejectionReason)
                .HasMaxLength(1000);

            builder.HasOne(x => x.User)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.UploadedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Reviewer)
                   .WithMany()
                   .HasForeignKey(x => x.ReviewedBy)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Subject)
                   .WithMany(x => x.Documents)
                   .HasForeignKey(x => x.SubjectId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Chapter)
                   .WithMany(x => x.Documents)
                   .HasForeignKey(x => x.ChapterId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

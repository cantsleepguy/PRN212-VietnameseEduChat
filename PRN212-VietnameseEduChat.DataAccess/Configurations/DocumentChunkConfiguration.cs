using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.DataAccess.Configurations
{
    public class DocumentChunkConfiguration
        : IEntityTypeConfiguration<DocumentChunk>
    {
        public void Configure(EntityTypeBuilder<DocumentChunk> builder)
        {
            builder.HasKey(x => x.DocumentChunkId);

            builder.Property(x => x.Content)
                .IsRequired();

            builder.Property(x => x.EmbeddingJson)
                .IsRequired();

            builder.Property(x => x.EmbeddingModel)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasOne(x => x.Document)
                .WithMany(x => x.Chunks)
                .HasForeignKey(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

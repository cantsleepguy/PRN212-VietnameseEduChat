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
    public class SubjectLecturerConfiguration
        : IEntityTypeConfiguration<SubjectLecturer>
    {
        public void Configure(EntityTypeBuilder<SubjectLecturer> builder)
        {
            builder.HasKey(x => x.SubjectLecturerId);

            builder.HasIndex(x => new
            {
                x.SubjectId,
                x.LecturerId
            })
            .IsUnique();

            builder.HasOne(x => x.Subject)
                .WithMany(x => x.SubjectLecturers)
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Lecturer)
                .WithMany()
                .HasForeignKey(x => x.LecturerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AssignedByUser)
                .WithMany()
                .HasForeignKey(x => x.AssignedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

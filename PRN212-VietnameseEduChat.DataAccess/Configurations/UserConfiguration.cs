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
    public class UserConfiguration
        : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.UserId);

            builder.Property(x => x.FullName)
                   .HasMaxLength(100);

            builder.Property(x => x.Email)
                   .HasMaxLength(255);

            builder.Property(x => x.Password)
                   .HasMaxLength(500);

            builder.HasOne(x => x.Role)
                   .WithMany(x => x.Users)
                   .HasForeignKey(x => x.RoleId);

            builder.HasIndex(x => x.Email).IsUnique();

            builder.Property(x => x.Email)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.Property(x => x.Password)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(x => x.FullName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.IsLocked)
                    .IsRequired()
                    .HasDefaultValue(false);
        }
    }
}

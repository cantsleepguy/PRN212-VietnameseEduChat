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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>()
                .HasKey(r => r.RoleId);

            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            //-------------------------------------
            // Seed Roles
            //-------------------------------------

            modelBuilder.Entity<Role>().HasData(

                new Role
                {
                    RoleId = 1,
                    RoleName = "Admin"
                },

                new Role
                {
                    RoleId = 2,
                    RoleName = "Head of Department"
                },

                new Role
                {
                    RoleId = 3,
                    RoleName = "Lecturer"
                },

                new Role
                {
                    RoleId = 4,
                    RoleName = "Student"
                },

                new Role
                {
                    RoleId = 5,
                    RoleName = "Guest"
                }

            );

            //-------------------------------------
            // Seed Users
            //-------------------------------------

            modelBuilder.Entity<User>().HasData(

                new User
                {
                    UserId = 1,
                    FullName = "System Administrator",
                    Email = "admin@educhat.com",
                    Password = "123456",
                    RoleId = 1
                },

                new User
                {
                    UserId = 2,
                    FullName = "Head of IT Department",
                    Email = "hod@educhat.com",
                    Password = "123456",
                    RoleId = 2
                },

                new User
                {
                    UserId = 3,
                    FullName = "Nguyen Van A",
                    Email = "lecturer@educhat.com",
                    Password = "123456",
                    RoleId = 3
                },

                new User
                {
                    UserId = 4,
                    FullName = "Tran Thi B",
                    Email = "student@educhat.com",
                    Password = "123456",
                    RoleId = 4
                }

            );
        }
    }
}

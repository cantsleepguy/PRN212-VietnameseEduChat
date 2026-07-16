using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Repositories.Interfaces;
using PRN212_VietnameseEduChat.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    public class PackageService : IPackageService
    {
        public const string CodeFree = "Free";
        public const string CodePremium = "Premium";
        public const string CodeEnterprise = "Enterprise";

        private readonly IPackageRepository _repository;

        public PackageService(IPackageRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Package>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<List<Package>> GetActiveAsync()
        {
            return await _repository.GetActiveAsync();
        }

        public async Task<Package?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Package> GetFreePackageAsync()
        {
            var freePackage = await _repository.GetByCodeAsync(CodeFree);

            if (freePackage != null)
            {
                return freePackage;
            }

            return new Package
            {
                PackageCode = CodeFree,
                PackageName = "Gói Miễn Phí",
                Price = 0,
                DurationDays = 0,
                DailyQuestionLimit = 10,
                MaxUploadSizeMb = 10,
                MaxDocuments = 5,
                AllowAiFeatures = false,
                IsActive = true
            };
        }

        public async Task UpdateAsync(Package package)
        {
            if (package.Price < 0)
            {
                throw new InvalidOperationException(
                    "Giá gói không được âm.");
            }

            if (package.DurationDays < 0)
            {
                throw new InvalidOperationException(
                    "Thời hạn gói không được âm.");
            }

            if (package.MaxUploadSizeMb <= 0)
            {
                throw new InvalidOperationException(
                    "Dung lượng upload tối đa phải lớn hơn 0.");
            }

            await _repository.UpdateAsync(package);
        }

        public async Task EnsureDefaultsAsync()
        {
            await EnsurePackageAsync(new Package
            {
                PackageCode = CodeFree,
                PackageName = "Gói Miễn Phí",
                Description = "Dành cho sinh viên mới bắt đầu. 10 câu hỏi mỗi ngày, tối đa 5 tài liệu.",
                Price = 0,
                DurationDays = 0,
                DailyQuestionLimit = 10,
                MaxUploadSizeMb = 10,
                MaxDocuments = 5,
                AllowAiFeatures = false,
                IsActive = true,
                CreatedAt = DateTime.Now
            });

            await EnsurePackageAsync(new Package
            {
                PackageCode = CodePremium,
                PackageName = "Gói Premium",
                Description = "Dành cho sinh viên học tập nghiêm túc. 100 câu hỏi mỗi ngày, tối đa 50 tài liệu, đầy đủ tính năng AI.",
                Price = 99000,
                DurationDays = 30,
                DailyQuestionLimit = 100,
                MaxUploadSizeMb = 25,
                MaxDocuments = 50,
                AllowAiFeatures = true,
                IsActive = true,
                CreatedAt = DateTime.Now
            });

            await EnsurePackageAsync(new Package
            {
                PackageCode = CodeEnterprise,
                PackageName = "Gói Enterprise",
                Description = "Dành cho tổ chức/lớp học. Không giới hạn câu hỏi và tài liệu, đầy đủ tính năng AI.",
                Price = 499000,
                DurationDays = 30,
                DailyQuestionLimit = null,
                MaxUploadSizeMb = 100,
                MaxDocuments = null,
                AllowAiFeatures = true,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
        }

        private async Task EnsurePackageAsync(Package package)
        {
            var existing = await _repository.GetByCodeAsync(package.PackageCode);

            if (existing == null)
            {
                await _repository.AddAsync(package);
            }
        }
    }
}

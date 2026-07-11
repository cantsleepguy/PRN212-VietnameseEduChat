using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Interfaces
{
    public interface IPackageService
    {
        Task<List<Package>> GetAllAsync();

        Task<List<Package>> GetActiveAsync();

        Task<Package?> GetByIdAsync(int id);

        Task<Package> GetFreePackageAsync();

        Task UpdateAsync(Package package);

        Task EnsureDefaultsAsync();
    }
}

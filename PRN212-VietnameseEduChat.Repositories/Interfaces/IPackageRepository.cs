using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Repositories.Interfaces
{
    public interface IPackageRepository
    {
        Task<List<Package>> GetAllAsync();

        Task<List<Package>> GetActiveAsync();

        Task<Package?> GetByIdAsync(int id);

        Task<Package?> GetByCodeAsync(string packageCode);

        Task AddAsync(Package package);

        Task UpdateAsync(Package package);
    }
}

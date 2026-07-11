using Microsoft.EntityFrameworkCore;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.DataAccess.Context;
using PRN212_VietnameseEduChat.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Repositories.Implementations
{
    public class PackageRepository : IPackageRepository
    {
        private readonly ApplicationDbContext _context;

        public PackageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Package>> GetAllAsync()
        {
            return await _context.Packages
                .OrderBy(x => x.Price)
                .ToListAsync();
        }

        public async Task<List<Package>> GetActiveAsync()
        {
            return await _context.Packages
                .Where(x => x.IsActive)
                .OrderBy(x => x.Price)
                .ToListAsync();
        }

        public async Task<Package?> GetByIdAsync(int id)
        {
            return await _context.Packages
                .FirstOrDefaultAsync(x => x.PackageId == id);
        }

        public async Task<Package?> GetByCodeAsync(string packageCode)
        {
            return await _context.Packages
                .FirstOrDefaultAsync(x => x.PackageCode == packageCode);
        }

        public async Task AddAsync(Package package)
        {
            _context.Packages.Add(package);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Package package)
        {
            _context.Packages.Update(package);

            await _context.SaveChangesAsync();
        }
    }
}

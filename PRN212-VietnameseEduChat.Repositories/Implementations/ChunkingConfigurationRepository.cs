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
    public class ChunkingConfigurationRepository : IChunkingConfigurationRepository
    {
        private readonly ApplicationDbContext _context;

        public ChunkingConfigurationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ChunkingConfiguration>> GetAllAsync()
        {
            return await _context.ChunkingConfigurations
                .Include(x => x.UpdatedByUser)
                .OrderBy(x => x.ChunkingConfigurationId)
                .ToListAsync();
        }

        public async Task<ChunkingConfiguration?> GetActiveAsync()
        {
            return await _context.ChunkingConfigurations
                .Where(x => x.IsActive)
                .OrderBy(x => x.ChunkingConfigurationId)
                .FirstOrDefaultAsync();
        }

        public async Task<ChunkingConfiguration?> GetByIdAsync(int id)
        {
            return await _context.ChunkingConfigurations
                .FirstOrDefaultAsync(x => x.ChunkingConfigurationId == id);
        }

        public async Task AddAsync(ChunkingConfiguration configuration)
        {
            _context.ChunkingConfigurations.Add(configuration);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ChunkingConfiguration configuration)
        {
            _context.ChunkingConfigurations.Update(configuration);

            await _context.SaveChangesAsync();
        }

        public async Task DeactivateAllAsync()
        {
            var configurations = await _context.ChunkingConfigurations
                .Where(x => x.IsActive)
                .ToListAsync();

            foreach (var configuration in configurations)
            {
                configuration.IsActive = false;
            }

            if (configurations.Count > 0)
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}

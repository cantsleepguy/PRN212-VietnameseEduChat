using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Repositories.Interfaces
{
    public interface IChunkingConfigurationRepository
    {
        Task<List<ChunkingConfiguration>> GetAllAsync();

        Task<ChunkingConfiguration?> GetActiveAsync();

        Task<ChunkingConfiguration?> GetByIdAsync(int id);

        Task AddAsync(ChunkingConfiguration configuration);

        Task UpdateAsync(ChunkingConfiguration configuration);

        Task DeactivateAllAsync();
    }
}

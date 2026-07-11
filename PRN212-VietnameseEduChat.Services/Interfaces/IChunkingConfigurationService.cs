using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Interfaces
{
    public interface IChunkingConfigurationService
    {
        Task<List<ChunkingConfiguration>> GetAllAsync();

        Task<ChunkingConfiguration> GetActiveAsync();

        Task ActivateAsync(int configurationId, int userId);

        Task UpdateAsync(
            int configurationId,
            int chunkSize,
            int chunkOverlap,
            string fixedSizeUnit,
            int userId);

        Task EnsureDefaultsAsync();
    }
}

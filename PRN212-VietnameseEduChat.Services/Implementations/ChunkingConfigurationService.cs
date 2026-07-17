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
    public class ChunkingConfigurationService : IChunkingConfigurationService
    {
        public const string StrategyParagraph = "Paragraph";
        public const string StrategyCharacter = "Character";
        public const string StrategyFixedSize = "FixedSize";

        private readonly IChunkingConfigurationRepository _repository;

        public ChunkingConfigurationService(
            IChunkingConfigurationRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ChunkingConfiguration>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<ChunkingConfiguration> GetActiveAsync()
        {
            var configurations = await _repository.GetAllAsync();
            var active = configurations.FirstOrDefault(configuration =>
                configuration.StrategyKey == StrategyCharacter &&
                configuration.IsActive);

            if (active != null)
            {
                return active;
            }

            var characterConfig = configurations.FirstOrDefault(configuration =>
                configuration.StrategyKey == StrategyCharacter);

            if (characterConfig != null)
            {
                return characterConfig;
            }

            return new ChunkingConfiguration
            {
                StrategyKey = StrategyCharacter,
                StrategyName = "Character Chunking (mặc định)",
                ChunkSize = 1200,
                ChunkOverlap = 200,
                FixedSizeUnit = "Characters",
                IsActive = true
            };
        }

        public async Task ActivateAsync(int configurationId, int userId)
        {
            var configuration = await _repository.GetByIdAsync(configurationId);

            if (configuration == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy cấu hình chunking.");
            }

            await _repository.DeactivateAllAsync();

            configuration.IsActive = true;
            configuration.UpdatedAt = DateTime.Now;
            configuration.UpdatedBy = userId;

            await _repository.UpdateAsync(configuration);
        }

        public async Task UpdateAsync(
            int configurationId,
            int chunkSize,
            int chunkOverlap,
            string fixedSizeUnit,
            int userId)
        {
            var configuration = await _repository.GetByIdAsync(configurationId);

            if (configuration == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy cấu hình chunking.");
            }

            if (chunkSize < 100 || chunkSize > 8000)
            {
                throw new InvalidOperationException(
                    "Chunk size phải nằm trong khoảng 100 - 8000.");
            }

            if (chunkOverlap < 0 || chunkOverlap >= chunkSize)
            {
                throw new InvalidOperationException(
                    "Chunk overlap phải >= 0 và nhỏ hơn chunk size.");
            }

            if (fixedSizeUnit != "Characters" && fixedSizeUnit != "Tokens")
            {
                throw new InvalidOperationException(
                    "Đơn vị fixed size chỉ hỗ trợ Characters hoặc Tokens.");
            }

            configuration.ChunkSize = chunkSize;
            configuration.ChunkOverlap = chunkOverlap;
            configuration.FixedSizeUnit = fixedSizeUnit;
            configuration.UpdatedAt = DateTime.Now;
            configuration.UpdatedBy = userId;

            await _repository.UpdateAsync(configuration);
        }

        public async Task EnsureDefaultsAsync()
        {
            var existing = await _repository.GetAllAsync();

            if (existing.Count == 0)
            {
                await _repository.AddAsync(new ChunkingConfiguration
                {
                    StrategyKey = StrategyCharacter,
                    StrategyName = "Character Chunking",
                    ChunkSize = 1200,
                    ChunkOverlap = 200,
                    FixedSizeUnit = "Characters",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                });

                return;
            }

            var characterConfig = existing.FirstOrDefault(x =>
                x.StrategyKey == StrategyCharacter);

            if (characterConfig == null)
            {
                await _repository.DeactivateAllAsync();

                await _repository.AddAsync(new ChunkingConfiguration
                {
                    StrategyKey = StrategyCharacter,
                    StrategyName = "Character Chunking",
                    ChunkSize = 1200,
                    ChunkOverlap = 200,
                    FixedSizeUnit = "Characters",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                });

                return;
            }

            await _repository.DeactivateAllAsync();

            characterConfig.IsActive = true;

            await _repository.UpdateAsync(characterConfig);
        }
    }
}

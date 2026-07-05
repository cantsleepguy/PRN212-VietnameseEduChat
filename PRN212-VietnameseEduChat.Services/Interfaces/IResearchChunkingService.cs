using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Research;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Interfaces
{
    public interface IResearchChunkingService
    {
        List<ResearchChunkingStrategyOptionDto> GetStrategies();

        ResearchChunkingStrategyOptionDto GetStrategy(string strategyKey);

        List<string> Chunk(string text, string strategyKey);
    }
}

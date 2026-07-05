using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Research;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Interfaces
{
    public interface IResearchBenchmarkService
    {
        Task<List<ResearchExperimentDto>> GetExperimentsAsync();

        Task<ResearchExperimentDetailDto?> GetExperimentDetailAsync(int experimentId);

        Task<int> CreateExperimentAsync(ResearchExperimentCreateDto input);

        Task RunExperimentAsync(int experimentId);

        Task DeleteExperimentAsync(int experimentId);
    }
}

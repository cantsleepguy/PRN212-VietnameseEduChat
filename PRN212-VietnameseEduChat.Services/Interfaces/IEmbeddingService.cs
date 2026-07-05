using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Interfaces
{
    public interface IEmbeddingService
    {
        Task<float[]> CreateEmbeddingAsync(
            string text,
            string? modelName = null,
            int? dimensions = null);

        string GetModelName();

        int GetDimensions(string? modelName = null);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Documents;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;

namespace PRN212_VietnameseEduChat.Services.Interfaces
{
    public interface IChunkService
    {
        List<string> Chunk(string text);

        List<ChunkResultDto> Chunk(
            string text,
            ChunkingConfiguration configuration);
    }
}

using PRN212_VietnameseEduChat.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    public class ChunkService : IChunkService
    {
        public List<string> Chunk(string text)
        {
            const int maxChunkLength = 1200;
            const int overlap = 200;

            var chunks = new List<string>();

            if (string.IsNullOrWhiteSpace(text))
                return chunks;

            var normalizedText = Regex
                .Replace(text, @"\s+", " ")
                .Trim();

            var step = maxChunkLength - overlap;

            for (int start = 0; start < normalizedText.Length; start += step)
            {
                var length = Math.Min(
                    maxChunkLength,
                    normalizedText.Length - start);

                var chunk = normalizedText
                    .Substring(start, length)
                    .Trim();

                if (!string.IsNullOrWhiteSpace(chunk))
                {
                    chunks.Add(chunk);
                }

                if (start + length >= normalizedText.Length)
                    break;
            }

            return chunks;
        }
    }
}

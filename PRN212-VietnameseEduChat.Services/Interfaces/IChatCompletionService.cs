using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Interfaces
{
    public interface IChatCompletionService
    {
        Task<string> GenerateAnswerAsync(string prompt);

        IAsyncEnumerable<string> GenerateAnswerStreamAsync(
            string prompt,
            CancellationToken cancellationToken = default);
    }
}

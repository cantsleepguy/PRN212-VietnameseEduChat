using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task<List<Payment>> GetAllAsync();

        Task<List<Payment>> GetByUserAsync(int userId);

        Task<Payment?> GetByIdAsync(int id);

        Task<Payment?> GetByTransactionIdAsync(string transactionId);

        Task AddAsync(Payment payment);

        Task UpdateAsync(Payment payment);

        Task<Payment?> GetByOrderCodeAsync(long orderCode);

        Task<bool> TryClaimPendingAsync(
            int paymentId,
            CancellationToken cancellationToken = default);
    }
}

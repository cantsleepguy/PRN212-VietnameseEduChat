using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<string> InitiatePaymentAsync(int userId, int packageId);

        Task<Payment?> GetByTransactionIdAsync(string transactionId);

        Task ConfirmPaymentAsync(
            string transactionId,
            int userId,
            bool isSuccessful);

        Task<List<Payment>> GetUserPaymentsAsync(int userId);

        Task<List<Payment>> GetAllPaymentsAsync();
    }
}

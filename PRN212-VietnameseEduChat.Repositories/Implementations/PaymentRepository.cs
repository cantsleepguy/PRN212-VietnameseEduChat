using Microsoft.EntityFrameworkCore;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.DataAccess.Context;
using PRN212_VietnameseEduChat.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Repositories.Implementations
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Payment>> GetAllAsync()
        {
            return await _context.Payments
                .Include(x => x.User)
                .Include(x => x.Package)
                .Include(x => x.SourceSubscription)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Payment>> GetByUserAsync(int userId)
        {
            return await _context.Payments
                .Include(x => x.Package)
                .Include(x => x.SourceSubscription)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _context.Payments
                .Include(x => x.User)
                .Include(x => x.Package)
                .Include(x => x.SourceSubscription)
                .FirstOrDefaultAsync(x => x.PaymentId == id);
        }

        public async Task<Payment?> GetByTransactionIdAsync(
            string transactionId)
        {
            return await _context.Payments
                .Include(x => x.User)
                .Include(x => x.Package)
                .Include(x => x.SourceSubscription)
                .FirstOrDefaultAsync(x =>
                    x.TransactionId == transactionId);
        }

        public async Task<Payment?> GetByOrderCodeAsync(long orderCode)
        {
            return await _context.Payments
                .Include(x => x.User)
                .Include(x => x.Package)
                .Include(x => x.SourceSubscription)
                .FirstOrDefaultAsync(x =>
                    x.OrderCode == orderCode);
        }

        public async Task AddAsync(Payment payment)
        {
            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);

            await _context.SaveChangesAsync();
        }
    }
}
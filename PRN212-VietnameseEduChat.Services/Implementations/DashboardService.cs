using Microsoft.EntityFrameworkCore;
using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Dashboard;
using PRN212_VietnameseEduChat.DataAccess.Context;
using PRN212_VietnameseEduChat.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private const int ActiveUserWindowDays = 7;
        private const int ChartWindowDays = 14;
        private const int ResponseTimeSampleSize = 200;

        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatsDto> GetStatisticsAsync()
        {
            var now = DateTime.Now;
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var activeSince = today.AddDays(-ActiveUserWindowDays);
            var chartSince = today.AddDays(-(ChartWindowDays - 1));

            var stats = new DashboardStatsDto();

            stats.TotalUsers = await _context.Users.CountAsync();

            stats.ActiveUsers = await _context.ChatSessions
                .Where(cs =>
                    (cs.UpdatedAt ?? cs.CreatedAt) >= activeSince)
                .Select(cs => cs.UserId)
                .Distinct()
                .CountAsync();

            stats.NewUsersThisMonth = await _context.UserSubscriptions
                .Where(us => us.CreatedAt >= monthStart)
                .Select(us => us.UserId)
                .Distinct()
                .CountAsync();

            stats.TotalDocuments = await _context.Documents.CountAsync();

            stats.TotalChunks = await _context.DocumentChunks.CountAsync();

            stats.DocumentsUploadedToday = await _context.Documents
                .CountAsync(d =>
                    d.UploadedAt >= today &&
                    d.UploadedAt < tomorrow);

            stats.TotalConversations = await _context.ChatSessions
                .CountAsync(cs => !cs.IsDeleted);

            stats.TotalQuestions = await _context.ChatMessages
                .CountAsync(m => m.Role == "User");

            stats.AverageResponseTimeSeconds =
                await ComputeAverageResponseTimeAsync();

            var successfulPayments = _context.Payments
                .Where(p => p.Status == "Success");

            stats.RevenueToday = await successfulPayments
                .Where(p => p.PaidAt >= today && p.PaidAt < tomorrow)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            stats.RevenueThisMonth = await successfulPayments
                .Where(p => p.PaidAt >= monthStart)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            stats.SuccessfulPayments = await _context.Payments
                .CountAsync(p => p.Status == "Success");

            stats.FailedPayments = await _context.Payments
                .CountAsync(p => p.Status == "Failed");

            stats.ActivePackages = await _context.Packages
                .CountAsync(p => p.IsActive);

            stats.RevenueByPackage = await successfulPayments
                .GroupBy(p => p.Package!.PackageName)
                .Select(g => new RevenueByPackageDto
                {
                    PackageName = g.Key,
                    Revenue = g.Sum(p => p.Amount)
                })
                .OrderByDescending(x => x.Revenue)
                .ToListAsync();

            var revenueRaw = await successfulPayments
                .Where(p => p.PaidAt >= chartSince)
                .GroupBy(p => p.PaidAt!.Value.Date)
                .Select(g => new
                {
                    Day = g.Key,
                    Revenue = g.Sum(p => p.Amount)
                })
                .ToListAsync();

            stats.RevenueByDay = Enumerable
                .Range(0, ChartWindowDays)
                .Select(offset => chartSince.AddDays(offset))
                .Select(day => new RevenueByDayDto
                {
                    Day = day,
                    Revenue = revenueRaw
                        .FirstOrDefault(x => x.Day == day)?.Revenue ?? 0
                })
                .ToList();

            var distributionRaw = await _context.UserSubscriptions
                .Where(us => us.Status == "Active" && us.EndDate >= now)
                .GroupBy(us => us.Package!.PackageName)
                .Select(g => new PackageDistributionDto
                {
                    PackageName = g.Key,
                    SubscriberCount = g.Select(us => us.UserId)
                        .Distinct()
                        .Count()
                })
                .ToListAsync();

            var subscribedUserCount = distributionRaw
                .Sum(x => x.SubscriberCount);

            var freeUsers = stats.TotalUsers - subscribedUserCount;

            if (freeUsers > 0)
            {
                distributionRaw.Add(new PackageDistributionDto
                {
                    PackageName = "Free (mặc định)",
                    SubscriberCount = freeUsers
                });
            }

            stats.PackageDistribution = distributionRaw;

            var questionsRaw = await _context.ChatMessages
                .Where(m => m.Role == "User" && m.CreatedAt >= chartSince)
                .GroupBy(m => m.CreatedAt.Date)
                .Select(g => new
                {
                    Day = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            stats.QuestionsByDay = Enumerable
                .Range(0, ChartWindowDays)
                .Select(offset => chartSince.AddDays(offset))
                .Select(day => new QuestionsByDayDto
                {
                    Day = day,
                    QuestionCount = questionsRaw
                        .FirstOrDefault(x => x.Day == day)?.Count ?? 0
                })
                .ToList();

            return stats;
        }

        private async Task<double> ComputeAverageResponseTimeAsync()
        {
            var recentMessages = await _context.ChatMessages
                .OrderByDescending(m => m.ChatMessageId)
                .Take(ResponseTimeSampleSize)
                .Select(m => new
                {
                    m.ChatSessionId,
                    m.Role,
                    m.CreatedAt,
                    m.ChatMessageId
                })
                .ToListAsync();

            var durations = new List<double>();

            var ordered = recentMessages
                .OrderBy(m => m.ChatMessageId)
                .ToList();

            for (var i = 1; i < ordered.Count; i++)
            {
                var previous = ordered[i - 1];
                var current = ordered[i];

                if (previous.Role == "User" &&
                    current.Role == "Assistant" &&
                    previous.ChatSessionId == current.ChatSessionId)
                {
                    var seconds = (current.CreatedAt - previous.CreatedAt)
                        .TotalSeconds;

                    if (seconds >= 0 && seconds < 300)
                    {
                        durations.Add(seconds);
                    }
                }
            }

            return durations.Count > 0
                ? Math.Round(durations.Average(), 2)
                : 0;
        }
    }
}

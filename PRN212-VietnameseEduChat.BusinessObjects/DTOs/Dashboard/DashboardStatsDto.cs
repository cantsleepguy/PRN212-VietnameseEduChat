using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.DTOs.Dashboard
{
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }

        public int ActiveUsers { get; set; }

        public int NewUsersThisMonth { get; set; }

        public int TotalDocuments { get; set; }

        public int TotalChunks { get; set; }

        public int DocumentsUploadedToday { get; set; }

        public int TotalConversations { get; set; }

        public int TotalQuestions { get; set; }

        public double AverageResponseTimeSeconds { get; set; }

        public decimal RevenueToday { get; set; }

        public decimal RevenueThisMonth { get; set; }

        public int SuccessfulPayments { get; set; }

        public int FailedPayments { get; set; }

        public int ActivePackages { get; set; }

        public List<RevenueByPackageDto> RevenueByPackage { get; set; } = new();

        public List<RevenueByDayDto> RevenueByDay { get; set; } = new();

        public List<PackageDistributionDto> PackageDistribution { get; set; } = new();

        public List<QuestionsByDayDto> QuestionsByDay { get; set; } = new();
    }

    public class RevenueByPackageDto
    {
        public string PackageName { get; set; } = string.Empty;

        public decimal Revenue { get; set; }
    }

    public class RevenueByDayDto
    {
        public DateTime Day { get; set; }

        public decimal Revenue { get; set; }
    }

    public class PackageDistributionDto
    {
        public string PackageName { get; set; } = string.Empty;

        public int SubscriberCount { get; set; }
    }

    public class QuestionsByDayDto
    {
        public DateTime Day { get; set; }

        public int QuestionCount { get; set; }
    }
}

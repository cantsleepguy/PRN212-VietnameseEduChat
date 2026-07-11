using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Dashboard;
using PRN212_VietnameseEduChat.Services.Interfaces;
using PRN212_VietnameseEduChat.Services.Security;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Pages.Admin
{
    [Authorize(Roles = AppRoles.AnyAdmin)]
    public class DashboardModel : PageModel
    {
        private readonly IDashboardService _dashboardService;

        public DashboardModel(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public DashboardStatsDto Stats { get; set; } = new();

        public async Task OnGetAsync()
        {
            Stats = await _dashboardService.GetStatisticsAsync();
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PRN212_VietnameseEduChat.Pages;

[Authorize]
public class DashboardModel : PageModel
{
    public void OnGet()
    {

    }
}

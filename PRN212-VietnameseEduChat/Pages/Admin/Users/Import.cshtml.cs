using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Users;
using PRN212_VietnameseEduChat.Services.Interfaces;
using PRN212_VietnameseEduChat.Services.Security;

namespace PRN212_VietnameseEduChat.Pages.Admin.Users
{
    [Authorize(Roles = AppRoles.SystemAdmin)]
    public class ImportModel : PageModel
    {
        private const long MaxFileSize =
            2 * 1024 * 1024;

        private readonly IUserManagementService
            _userManagementService;

        public ImportModel(
            IUserManagementService userManagementService)
        {
            _userManagementService =
                userManagementService;
        }

        [BindProperty]
        [Required(
            ErrorMessage = "Vui lòng chọn file CSV.")]
        public IFormFile? CsvFile { get; set; }

        public UserImportResultDto? ImportResult { get; set; }

        public List<string> RoleNames { get; set; }
            = new();

        public async Task OnGetAsync()
        {
            await LoadRolesAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadRolesAsync();

            if (CsvFile == null || CsvFile.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(CsvFile),
                    "Vui lòng chọn file CSV.");

                return Page();
            }

            var extension = Path.GetExtension(
                CsvFile.FileName);

            if (!string.Equals(
                    extension,
                    ".csv",
                    StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    nameof(CsvFile),
                    "Chỉ chấp nhận file có định dạng .csv.");

                return Page();
            }

            if (CsvFile.Length > MaxFileSize)
            {
                ModelState.AddModelError(
                    nameof(CsvFile),
                    "File CSV không được lớn hơn 2 MB.");

                return Page();
            }

            try
            {
                await using var stream =
                    CsvFile.OpenReadStream();

                ImportResult =
                    await _userManagementService
                        .ImportUsersFromCsvAsync(
                            stream,
                            HttpContext.RequestAborted);
            }
            catch (OperationCanceledException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Quá trình import đã bị hủy.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);
            }

            return Page();
        }

        public IActionResult OnGetTemplate()
        {
            var csv =
                "\uFEFFFullName,Email,Password,Role\r\n" +
                "Nguyễn Văn A,student01@gmail.com,123456,Student\r\n" +
                "Trần Thị B,lecturer01@gmail.com,123456,Lecturer\r\n";

            var bytes = Encoding.UTF8.GetBytes(csv);

            return File(
                bytes,
                "text/csv; charset=utf-8",
                "user-import-template.csv");
        }

        private async Task LoadRolesAsync()
        {
            RoleNames =
                await _userManagementService
                    .GetRoleNamesAsync();
        }
    }
}
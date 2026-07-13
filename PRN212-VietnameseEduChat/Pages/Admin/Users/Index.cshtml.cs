using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Users;
using PRN212_VietnameseEduChat.Services.Interfaces;
using PRN212_VietnameseEduChat.Services.Security;

namespace PRN212_VietnameseEduChat.Pages.Admin.Users
{
    [Authorize(Roles = AppRoles.SystemAdmin)]
    public class IndexModel : PageModel
    {
        private readonly IUserManagementService _userManagementService;

        public IndexModel(
            IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        public List<UserManagementItemDto> Users { get; set; }
            = new();

        public List<string> RoleNames { get; set; }
            = new();

        public int CurrentUserId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? RoleFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty]
        public CreateUserInputModel CreateInput { get; set; }
            = new();

        public async Task<IActionResult> OnGetAsync()
        {
            if (!TryGetCurrentUserId(out var currentUserId))
            {
                return Challenge();
            }

            CurrentUserId = currentUserId;

            await LoadPageDataAsync();

            if (string.IsNullOrWhiteSpace(CreateInput.RoleName))
            {
                CreateInput.RoleName = AppRoles.Student;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!TryGetCurrentUserId(out var currentUserId))
            {
                return Challenge();
            }

            CurrentUserId = currentUserId;

            if (!ModelState.IsValid)
            {
                await LoadPageDataAsync();

                return Page();
            }

            try
            {
                await _userManagementService.CreateUserAsync(
                    CreateInput.FullName,
                    CreateInput.Email,
                    CreateInput.Password,
                    CreateInput.RoleName);

                TempData["SuccessMessage"] =
                    $"Đã tạo tài khoản {CreateInput.Email}.";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                await LoadPageDataAsync();

                return Page();
            }

            return RedirectToPageWithFilters();
        }

        public async Task<IActionResult> OnPostChangeRoleAsync(
            int userId,
            string? newRoleName)
        {
            if (!TryGetCurrentUserId(out var currentUserId))
            {
                return Challenge();
            }

            try
            {
                await _userManagementService.ChangeRoleAsync(
                    userId,
                    newRoleName ?? string.Empty,
                    currentUserId);

                TempData["SuccessMessage"] =
                    "Đã cập nhật role của tài khoản.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPageWithFilters();
        }

        public async Task<IActionResult> OnPostSetLockAsync(
            int userId,
            bool isLocked)
        {
            if (!TryGetCurrentUserId(out var currentUserId))
            {
                return Challenge();
            }

            try
            {
                await _userManagementService.SetLockedAsync(
                    userId,
                    isLocked,
                    currentUserId);

                TempData["SuccessMessage"] = isLocked
                    ? "Đã khóa tài khoản."
                    : "Đã mở khóa tài khoản.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPageWithFilters();
        }

        private async Task LoadPageDataAsync()
        {
            bool? isLocked = StatusFilter switch
            {
                "active" => false,
                "locked" => true,
                _ => null
            };

            Users = await _userManagementService.GetUsersAsync(
                Keyword,
                RoleFilter,
                isLocked);

            RoleNames =
                await _userManagementService.GetRoleNamesAsync();
        }

        private bool TryGetCurrentUserId(
            out int currentUserId)
        {
            var userIdText = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            return int.TryParse(
                userIdText,
                out currentUserId);
        }

        private IActionResult RedirectToPageWithFilters()
        {
            return RedirectToPage(
                new
                {
                    keyword = Keyword,
                    roleFilter = RoleFilter,
                    statusFilter = StatusFilter
                });
        }

        public class CreateUserInputModel
        {
            [Required(
                ErrorMessage = "Vui lòng nhập họ tên.")]
            [StringLength(
                100,
                ErrorMessage = "Họ tên không được dài quá 100 ký tự.")]
            [Display(Name = "Họ và tên")]
            public string FullName { get; set; }
                = string.Empty;

            [Required(
                ErrorMessage = "Vui lòng nhập email.")]
            [EmailAddress(
                ErrorMessage = "Email không hợp lệ.")]
            [StringLength(
                255,
                ErrorMessage = "Email không được dài quá 255 ký tự.")]
            [Display(Name = "Email")]
            public string Email { get; set; }
                = string.Empty;

            [Required(
                ErrorMessage = "Vui lòng nhập mật khẩu.")]
            [MinLength(
                6,
                ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
            [StringLength(
                100,
                ErrorMessage = "Mật khẩu không được dài quá 100 ký tự.")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; }
                = string.Empty;

            [Required(
                ErrorMessage = "Vui lòng chọn role.")]
            [Display(Name = "Role")]
            public string RoleName { get; set; }
                = AppRoles.Student;
        }
    }
}
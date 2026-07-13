using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Users;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Repositories.Interfaces;
using PRN212_VietnameseEduChat.Services.Interfaces;
using PRN212_VietnameseEduChat.Services.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    public class UserManagementService : IUserManagementService
    {
        private static readonly HashSet<string> AllowedRoles =
            new(StringComparer.OrdinalIgnoreCase)
            {
        AppRoles.Student,
        AppRoles.Lecturer,
        AppRoles.AcademicAdmin,
        AppRoles.SystemAdmin
            };

        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserManagementService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<List<UserManagementItemDto>>
            GetUsersAsync(
                string? keyword = null,
                string? roleName = null,
                bool? isLocked = null)
        {
            var users = await _userRepository.GetAllAsync(
                keyword,
                roleName,
                isLocked);

            return users.Select(x =>
                new UserManagementItemDto
                {
                    UserId = x.UserId,
                    FullName = x.FullName,
                    Email = x.Email,
                    RoleName = x.Role?.RoleName
                               ?? "Không xác định",
                    IsLocked = x.IsLocked
                })
                .ToList();
        }

        public async Task<List<string>> GetRoleNamesAsync()
        {
            var roles = await _roleRepository.GetAllAsync();

            return roles
                .Where(x => AllowedRoles.Contains(x.RoleName))
                .Select(x => x.RoleName)
                .ToList();
        }

        public async Task CreateUserAsync(
            string fullName,
            string email,
            string password,
            string roleName)
        {
            fullName = fullName?.Trim() ?? string.Empty;
            email = email?.Trim().ToLowerInvariant()
                    ?? string.Empty;
            password ??= string.Empty;
            roleName = roleName?.Trim() ?? string.Empty;

            ValidateCreateInput(
                fullName,
                email,
                password,
                roleName);

            var emailExists =
                await _userRepository.EmailExistsAsync(email);

            if (emailExists)
            {
                throw new InvalidOperationException(
                    "Email này đã được sử dụng.");
            }

            var role =
                await _roleRepository.GetByNameAsync(roleName);

            if (role == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy role đã chọn.");
            }

            var user = new User
            {
                FullName = fullName,
                Email = email,
                RoleId = role.RoleId,
                IsLocked = false
            };

            user.Password = _passwordHasher.HashPassword(
                user,
                password);

            await _userRepository.AddAsync(user);
        }

        public async Task ChangeRoleAsync(
            int userId,
            string newRoleName,
            int currentAdminUserId)
        {
            newRoleName = newRoleName?.Trim()
                          ?? string.Empty;

            ValidateRoleName(newRoleName);

            if (userId == currentAdminUserId)
            {
                throw new InvalidOperationException(
                    "Bạn không thể tự thay đổi role của chính mình.");
            }

            var user =
                await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy tài khoản.");
            }

            var newRole =
                await _roleRepository.GetByNameAsync(
                    newRoleName);

            if (newRole == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy role đã chọn.");
            }

            var currentRoleName = user.Role?.RoleName;

            if (currentRoleName == newRoleName)
            {
                return;
            }

            if (currentRoleName == AppRoles.SystemAdmin &&
                !user.IsLocked &&
                newRoleName != AppRoles.SystemAdmin)
            {
                await EnsureNotLastActiveSystemAdminAsync();
            }

            user.RoleId = newRole.RoleId;
            user.Role = newRole;

            await _userRepository.UpdateAsync(user);
        }

        public async Task SetLockedAsync(
            int userId,
            bool isLocked,
            int currentAdminUserId)
        {
            if (userId == currentAdminUserId)
            {
                throw new InvalidOperationException(
                    "Bạn không thể tự khóa tài khoản của chính mình.");
            }

            var user =
                await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy tài khoản.");
            }

            if (user.IsLocked == isLocked)
            {
                return;
            }

            if (isLocked &&
                !user.IsLocked &&
                user.Role?.RoleName == AppRoles.SystemAdmin)
            {
                await EnsureNotLastActiveSystemAdminAsync();
            }

            user.IsLocked = isLocked;

            await _userRepository.UpdateAsync(user);
        }

        private static void ValidateCreateInput(
            string fullName,
            string email,
            string password,
            string roleName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new InvalidOperationException(
                    "Họ tên không được để trống.");
            }

            if (fullName.Length > 100)
            {
                throw new InvalidOperationException(
                    "Họ tên không được dài quá 100 ký tự.");
            }

            if (string.IsNullOrWhiteSpace(email) ||
                !new EmailAddressAttribute().IsValid(email))
            {
                throw new InvalidOperationException(
                    "Email không hợp lệ.");
            }

            if (email.Length > 255)
            {
                throw new InvalidOperationException(
                    "Email không được dài quá 255 ký tự.");
            }

            if (password.Length < 6)
            {
                throw new InvalidOperationException(
                    "Mật khẩu phải có ít nhất 6 ký tự.");
            }

            ValidateRoleName(roleName);
        }

        private static void ValidateRoleName(
            string roleName)
        {
            if (!AllowedRoles.Contains(roleName))
            {
                throw new InvalidOperationException(
                    "Role không hợp lệ.");
            }
        }

        private async Task EnsureNotLastActiveSystemAdminAsync()
        {
            var activeSystemAdminCount =
                await _userRepository
                    .CountActiveByRoleNameAsync(
                        AppRoles.SystemAdmin);

            if (activeSystemAdminCount <= 1)
            {
                throw new InvalidOperationException(
                    "Không thể khóa hoặc hạ quyền System Admin đang hoạt động cuối cùng.");
            }
        }

        public async Task<UserImportResultDto> ImportUsersFromCsvAsync(Stream csvStream, CancellationToken cancellationToken = default)
        {
            const int maxRows = 1000;

            if (csvStream == null || !csvStream.CanRead)
            {
                throw new InvalidOperationException(
                    "Không thể đọc file CSV.");
            }

            var result = new UserImportResultDto();

            var roles = await _roleRepository.GetAllAsync();

            var roleLookup = roles
                .Where(x => AllowedRoles.Contains(x.RoleName))
                .ToDictionary(
                    x => x.RoleName,
                    x => x,
                    StringComparer.OrdinalIgnoreCase);

            using var buffer = new MemoryStream();

            await csvStream.CopyToAsync(
                buffer,
                cancellationToken);

            if (buffer.Length == 0)
            {
                throw new InvalidOperationException(
                    "File CSV đang trống.");
            }

            var delimiter = DetectDelimiter(buffer);

            buffer.Position = 0;

            using var parser = new TextFieldParser(
                buffer,
                Encoding.UTF8,
                detectEncoding: true);

            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(delimiter);
            parser.HasFieldsEnclosedInQuotes = true;
            parser.TrimWhiteSpace = true;

            if (parser.EndOfData)
            {
                throw new InvalidOperationException(
                    "File CSV không có dữ liệu.");
            }

            var headers = parser.ReadFields();

            if (headers == null || headers.Length == 0)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy dòng tiêu đề CSV.");
            }

            var headerMap = BuildHeaderMap(headers);

            EnsureRequiredHeaders(headerMap);

            var parsedRows = new List<ParsedImportRow>();

            var emailsInsideFile = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

            var rowNumber = 1;

            while (!parser.EndOfData)
            {
                cancellationToken.ThrowIfCancellationRequested();

                rowNumber++;

                string[]? fields;

                try
                {
                    fields = parser.ReadFields();
                }
                catch (MalformedLineException)
                {
                    result.TotalRows++;

                    result.Rows.Add(new UserImportRowResultDto
                    {
                        RowNumber = rowNumber,
                        IsSuccess = false,
                        Message = "Cấu trúc dòng CSV không hợp lệ."
                    });

                    continue;
                }

                if (fields == null ||
                    fields.All(string.IsNullOrWhiteSpace))
                {
                    continue;
                }

                result.TotalRows++;

                if (result.TotalRows > maxRows)
                {
                    throw new InvalidOperationException(
                        $"Mỗi lần chỉ được import tối đa {maxRows} tài khoản.");
                }

                var fullName = GetField(
                    fields,
                    headerMap["fullname"])
                    .Trim();

                var email = GetField(
                    fields,
                    headerMap["email"])
                    .Trim()
                    .ToLowerInvariant();

                var password = GetField(
                    fields,
                    headerMap["password"]);

                var roleName = GetField(
                    fields,
                    headerMap["role"])
                    .Trim();

                try
                {
                    ValidateCreateInput(
                        fullName,
                        email,
                        password,
                        roleName);

                    if (!roleLookup.TryGetValue(
                            roleName,
                            out var role))
                    {
                        throw new InvalidOperationException(
                            $"Không tìm thấy role '{roleName}'.");
                    }

                    if (!emailsInsideFile.Add(email))
                    {
                        throw new InvalidOperationException(
                            "Email bị lặp lại trong file CSV.");
                    }

                    parsedRows.Add(new ParsedImportRow(
                        rowNumber,
                        fullName,
                        email,
                        password,
                        role));
                }
                catch (Exception ex)
                {
                    result.Rows.Add(new UserImportRowResultDto
                    {
                        RowNumber = rowNumber,
                        FullName = fullName,
                        Email = email,
                        IsSuccess = false,
                        Message = ex.Message
                    });
                }
            }

            var existingEmails =
                await _userRepository.GetExistingEmailsAsync(
                    parsedRows.Select(x => x.Email));

            foreach (var row in parsedRows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (existingEmails.Contains(row.Email))
                {
                    result.Rows.Add(new UserImportRowResultDto
                    {
                        RowNumber = row.RowNumber,
                        FullName = row.FullName,
                        Email = row.Email,
                        IsSuccess = false,
                        Message = "Email đã tồn tại trong hệ thống."
                    });

                    continue;
                }

                var user = new User
                {
                    FullName = row.FullName,
                    Email = row.Email,
                    RoleId = row.Role.RoleId,
                    IsLocked = false
                };

                user.Password = _passwordHasher.HashPassword(
                    user,
                    row.Password);

                try
                {
                    await _userRepository.AddAsync(user);

                    existingEmails.Add(row.Email);

                    result.Rows.Add(new UserImportRowResultDto
                    {
                        RowNumber = row.RowNumber,
                        FullName = row.FullName,
                        Email = row.Email,
                        IsSuccess = true,
                        Message = "Tạo tài khoản thành công."
                    });
                }
                catch (DbUpdateException)
                {
                    result.Rows.Add(new UserImportRowResultDto
                    {
                        RowNumber = row.RowNumber,
                        FullName = row.FullName,
                        Email = row.Email,
                        IsSuccess = false,
                        Message =
                            "Không thể tạo tài khoản. Email có thể đã tồn tại."
                    });
                }
                catch
                {
                    result.Rows.Add(new UserImportRowResultDto
                    {
                        RowNumber = row.RowNumber,
                        FullName = row.FullName,
                        Email = row.Email,
                        IsSuccess = false,
                        Message =
                            "Có lỗi xảy ra khi lưu tài khoản."
                    });
                }
            }

            result.Rows = result.Rows
                .OrderBy(x => x.RowNumber)
                .ToList();

            return result;
        }

        private static string DetectDelimiter(
    MemoryStream buffer)
        {
            buffer.Position = 0;

            string headerLine;

            using (var reader = new StreamReader(
                       buffer,
                       Encoding.UTF8,
                       detectEncodingFromByteOrderMarks: true,
                       bufferSize: 1024,
                       leaveOpen: true))
            {
                headerLine = reader.ReadLine()
                             ?? string.Empty;
            }

            buffer.Position = 0;

            var commaCount = headerLine.Count(x => x == ',');
            var semicolonCount = headerLine.Count(x => x == ';');

            return semicolonCount > commaCount
                ? ";"
                : ",";
        }

        private static Dictionary<string, int> BuildHeaderMap(
            string[] headers)
        {
            var result = new Dictionary<string, int>(
                StringComparer.OrdinalIgnoreCase);

            for (var index = 0;
                 index < headers.Length;
                 index++)
            {
                var normalizedHeader =
                    NormalizeHeader(headers[index]);

                if (!string.IsNullOrWhiteSpace(normalizedHeader) &&
                    !result.ContainsKey(normalizedHeader))
                {
                    result[normalizedHeader] = index;
                }
            }

            return result;
        }

        private static string NormalizeHeader(
            string header)
        {
            return (header ?? string.Empty)
                .Trim()
                .TrimStart('\uFEFF')
                .Replace(" ", string.Empty)
                .Replace("_", string.Empty)
                .ToLowerInvariant();
        }

        private static void EnsureRequiredHeaders(
            Dictionary<string, int> headerMap)
        {
            var requiredHeaders = new[]
            {
        "fullname",
        "email",
        "password",
        "role"
    };

            var missingHeaders = requiredHeaders
                .Where(x => !headerMap.ContainsKey(x))
                .ToList();

            if (missingHeaders.Count > 0)
            {
                throw new InvalidOperationException(
                    "File CSV phải có đủ các cột: " +
                    "FullName, Email, Password, Role.");
            }
        }

        private static string GetField(
            string[] fields,
            int index)
        {
            if (index < 0 || index >= fields.Length)
            {
                return string.Empty;
            }

            return fields[index] ?? string.Empty;
        }

        private sealed record ParsedImportRow(
            int RowNumber,
            string FullName,
            string Email,
            string Password,
            Role Role);
    }
}

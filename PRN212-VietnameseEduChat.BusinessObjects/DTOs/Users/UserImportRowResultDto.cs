using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.DTOs.Users
{
    public class UserImportRowResultDto
    {
        public int RowNumber { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public bool IsSuccess { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}

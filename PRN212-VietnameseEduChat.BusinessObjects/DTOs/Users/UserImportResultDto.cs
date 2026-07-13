using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.BusinessObjects.DTOs.Users
{
    public class UserImportResultDto
    {
        public int TotalRows { get; set; }

        public List<UserImportRowResultDto> Rows { get; set; }
            = new();

        public int SuccessCount =>
            Rows.Count(x => x.IsSuccess);

        public int FailureCount =>
            Rows.Count(x => !x.IsSuccess);
    }
}

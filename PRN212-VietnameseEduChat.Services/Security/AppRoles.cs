using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Security
{
    public static class AppRoles
    {
        public const string Student = "Student";

        public const string Lecturer = "Lecturer";

        public const string AcademicAdmin = "AcademicAdmin";

        public const string SystemAdmin = "SystemAdmin";

        public const string LecturerOrAcademicAdmin =
            Lecturer + "," + AcademicAdmin;

        public const string AnyAdmin =
            AcademicAdmin + "," + SystemAdmin;
    }
}

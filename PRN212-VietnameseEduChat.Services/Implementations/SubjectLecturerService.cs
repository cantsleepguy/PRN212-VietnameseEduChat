using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.Repositories.Interfaces;
using PRN212_VietnameseEduChat.Services.Interfaces;
using PRN212_VietnameseEduChat.Services.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    public class SubjectLecturerService : ISubjectLecturerService
    {
        private readonly ISubjectLecturerRepository _assignmentRepository;
        private readonly ISubjectRepository _subjectRepository;
        private readonly IUserRepository _userRepository;

        public SubjectLecturerService(
            ISubjectLecturerRepository assignmentRepository,
            ISubjectRepository subjectRepository,
            IUserRepository userRepository)
        {
            _assignmentRepository = assignmentRepository;
            _subjectRepository = subjectRepository;
            _userRepository = userRepository;
        }

        public async Task<List<User>> GetLecturersAsync()
        {
            return await _userRepository.GetByRoleNameAsync(
                AppRoles.Lecturer);
        }

        public async Task<List<Subject>> GetAssignedSubjectsAsync(
            int lecturerId)
        {
            return await _assignmentRepository
                .GetAssignedSubjectsAsync(lecturerId);
        }

        public async Task<bool> IsLecturerAssignedAsync(
            int subjectId,
            int lecturerId)
        {
            var subject = await _subjectRepository.GetByIdAsync(subjectId);

            if (subject == null || !subject.IsActive)
            {
                return false;
            }

            return await _assignmentRepository.ExistsAsync(
                subjectId,
                lecturerId);
        }

        public async Task AssignAsync(
            int subjectId,
            int lecturerId,
            int assignedBy)
        {
            var subject = await _subjectRepository.GetByIdAsync(subjectId);

            if (subject == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy môn học.");
            }

            if (!subject.IsActive)
            {
                throw new InvalidOperationException(
                    "Môn học này đang bị ẩn nên không thể phân công giảng viên.");
            }

            var lecturer = await _userRepository.GetByIdAsync(lecturerId);

            if (lecturer == null ||
                lecturer.Role == null ||
                lecturer.Role.RoleName != AppRoles.Lecturer)
            {
                throw new InvalidOperationException(
                    "Người được phân công phải là Lecturer.");
            }

            var alreadyAssigned = await _assignmentRepository.ExistsAsync(
                subjectId,
                lecturerId);

            if (alreadyAssigned)
            {
                throw new InvalidOperationException(
                    "Lecturer này đã được phân công vào môn học này.");
            }

            var assignment = new SubjectLecturer
            {
                SubjectId = subjectId,
                LecturerId = lecturerId,
                AssignedBy = assignedBy,
                AssignedAt = DateTime.Now
            };

            await _assignmentRepository.AddAsync(assignment);
        }

        public async Task UnassignAsync(
            int subjectId,
            int lecturerId)
        {
            var assignment = await _assignmentRepository.GetAsync(
                subjectId,
                lecturerId);

            if (assignment == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy phân công.");
            }

            await _assignmentRepository.DeleteAsync(assignment);
        }
    }
}

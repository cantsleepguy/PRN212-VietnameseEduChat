using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PRN212_VietnameseEduChat.BusinessObjects.DTOs.Research;
using PRN212_VietnameseEduChat.BusinessObjects.Entities;
using PRN212_VietnameseEduChat.DataAccess.Context;
using PRN212_VietnameseEduChat.Services.Interfaces;
using System.Text;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    public class ResearchQuestionService : IResearchQuestionService
    {
        private const int MaxQuestionCount = 50;

        private readonly ApplicationDbContext _context;

        public ResearchQuestionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ResearchQuestionDto>> GetAllAsync()
        {
            return await _context.ResearchQuestions
                .Include(q => q.Subject)
                .Include(q => q.Chapter)
                .Include(q => q.SourceDocument)
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new ResearchQuestionDto
                {
                    ResearchQuestionId = q.ResearchQuestionId,
                    SubjectId = q.SubjectId,
                    SubjectName = q.Subject != null
                        ? q.Subject.SubjectName
                        : null,
                    ChapterId = q.ChapterId,
                    ChapterName = q.Chapter != null
                        ? q.Chapter.ChapterName
                        : null,
                    SourceDocumentId = q.SourceDocumentId,
                    SourceDocumentName = q.SourceDocument != null
                        ? q.SourceDocument.OriginalFileName
                        : null,
                    Question = q.Question,
                    GroundTruthAnswer = q.GroundTruthAnswer,
                    ExpectedKeywords = q.ExpectedKeywords,
                    ExpectedSource = q.ExpectedSource,
                    CreatedAt = q.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<ResearchQuestionDto?> GetByIdAsync(int id)
        {
            return await _context.ResearchQuestions
                .Include(q => q.Subject)
                .Include(q => q.Chapter)
                .Include(q => q.SourceDocument)
                .Where(q => q.ResearchQuestionId == id)
                .Select(q => new ResearchQuestionDto
                {
                    ResearchQuestionId = q.ResearchQuestionId,
                    SubjectId = q.SubjectId,
                    SubjectName = q.Subject != null
                        ? q.Subject.SubjectName
                        : null,
                    ChapterId = q.ChapterId,
                    ChapterName = q.Chapter != null
                        ? q.Chapter.ChapterName
                        : null,
                    SourceDocumentId = q.SourceDocumentId,
                    SourceDocumentName = q.SourceDocument != null
                        ? q.SourceDocument.OriginalFileName
                        : null,
                    Question = q.Question,
                    GroundTruthAnswer = q.GroundTruthAnswer,
                    ExpectedKeywords = q.ExpectedKeywords,
                    ExpectedSource = q.ExpectedSource,
                    CreatedAt = q.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ResearchQuestionFormOptionsDto> GetFormOptionsAsync()
        {
            var subjects = await _context.Subjects
                .OrderBy(s => s.SubjectName)
                .Select(s => new ResearchSelectOptionDto
                {
                    Id = s.SubjectId,
                    Name = s.SubjectName
                })
                .ToListAsync();

            var chapters = await _context.Chapters
                .Include(c => c.Subject)
                .OrderBy(c => c.Subject != null ? c.Subject.SubjectName : "")
                .ThenBy(c => c.ChapterName)
                .Select(c => new ResearchSelectOptionDto
                {
                    Id = c.ChapterId,
                    Name = c.Subject != null
                        ? c.Subject.SubjectName + " - " + c.ChapterName
                        : c.ChapterName
                })
                .ToListAsync();

            var documents = await _context.Documents
                .Include(d => d.Subject)
                .Include(d => d.Chapter)
                .Where(d => d.Status == "Approved")
                .OrderByDescending(d => d.UploadedAt)
                .Select(d => new ResearchSelectOptionDto
                {
                    Id = d.DocumentId,
                    Name =
                        d.OriginalFileName +
                        (d.Subject != null
                            ? " | " + d.Subject.SubjectName
                            : "") +
                        (d.Chapter != null
                            ? " | " + d.Chapter.ChapterName
                            : "")
                })
                .ToListAsync();

            return new ResearchQuestionFormOptionsDto
            {
                Subjects = subjects,
                Chapters = chapters,
                Documents = documents
            };
        }

        public async Task CreateAsync(ResearchQuestionInputDto input)
        {
            var currentCount = await _context.ResearchQuestions.CountAsync();

            if (currentCount >= MaxQuestionCount)
            {
                throw new InvalidOperationException(
                    "Test set đã đủ 50 câu hỏi. Không thể thêm câu mới.");
            }

            ValidateInput(input);

            var normalized = await NormalizeSubjectAndChapterAsync(input);

            var question = new ResearchQuestion
            {
                SubjectId = normalized.SubjectId,
                ChapterId = normalized.ChapterId,
                SourceDocumentId = input.SourceDocumentId,
                Question = input.Question.Trim(),
                GroundTruthAnswer = input.GroundTruthAnswer.Trim(),
                ExpectedKeywords = input.ExpectedKeywords?.Trim(),
                ExpectedSource = input.ExpectedSource?.Trim(),
                CreatedAt = DateTime.Now
            };

            _context.ResearchQuestions.Add(question);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(
            int id,
            ResearchQuestionInputDto input)
        {
            ValidateInput(input);

            var question = await _context.ResearchQuestions
                .FirstOrDefaultAsync(q => q.ResearchQuestionId == id);

            if (question == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy câu hỏi test set.");
            }

            var normalized = await NormalizeSubjectAndChapterAsync(input);

            question.SubjectId = normalized.SubjectId;
            question.ChapterId = normalized.ChapterId;
            question.SourceDocumentId = input.SourceDocumentId;
            question.Question = input.Question.Trim();
            question.GroundTruthAnswer = input.GroundTruthAnswer.Trim();
            question.ExpectedKeywords = input.ExpectedKeywords?.Trim();
            question.ExpectedSource = input.ExpectedSource?.Trim();

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var question = await _context.ResearchQuestions
                .Include(q => q.Results)
                .FirstOrDefaultAsync(q => q.ResearchQuestionId == id);

            if (question == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy câu hỏi test set.");
            }

            if (question.Results.Count > 0)
            {
                throw new InvalidOperationException(
                    "Không thể xóa câu hỏi đã có kết quả benchmark. Hãy xóa kết quả benchmark trước.");
            }

            _context.ResearchQuestions.Remove(question);

            await _context.SaveChangesAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.ResearchQuestions.CountAsync();
        }

        private static void ValidateInput(ResearchQuestionInputDto input)
        {
            if (string.IsNullOrWhiteSpace(input.Question))
            {
                throw new InvalidOperationException(
                    "Question không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(input.GroundTruthAnswer))
            {
                throw new InvalidOperationException(
                    "Ground truth answer không được để trống.");
            }
        }

        private async Task<(int? SubjectId, int? ChapterId)> NormalizeSubjectAndChapterAsync(
            ResearchQuestionInputDto input)
        {
            int? subjectId = input.SubjectId;
            int? chapterId = input.ChapterId;

            if (input.SourceDocumentId.HasValue)
            {
                var document = await _context.Documents
                    .FirstOrDefaultAsync(d =>
                        d.DocumentId == input.SourceDocumentId.Value &&
                        d.Status == "Approved");

                if (document == null)
                {
                    throw new InvalidOperationException(
                        "Tài liệu nguồn không tồn tại hoặc chưa được duyệt.");
                }

                subjectId ??= document.SubjectId;
                chapterId ??= document.ChapterId;
            }

            if (subjectId.HasValue)
            {
                var subjectExists = await _context.Subjects
                    .AnyAsync(s => s.SubjectId == subjectId.Value);

                if (!subjectExists)
                {
                    throw new InvalidOperationException(
                        "Môn học không tồn tại.");
                }
            }

            if (chapterId.HasValue)
            {
                var chapterExists = await _context.Chapters
                    .AnyAsync(c => c.ChapterId == chapterId.Value);

                if (!chapterExists)
                {
                    throw new InvalidOperationException(
                        "Chương không tồn tại.");
                }
            }

            return (subjectId, chapterId);
        }

        public async Task<ResearchQuestionImportResultDto> ImportCsvAsync(IFormFile file)
        {
            var result = new ResearchQuestionImportResultDto();

            if (file == null || file.Length == 0)
            {
                throw new InvalidOperationException("Vui lòng chọn file CSV.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (extension != ".csv")
            {
                throw new InvalidOperationException("Chỉ hỗ trợ import file .csv.");
            }

            var currentCount = await _context.ResearchQuestions.CountAsync();

            if (currentCount >= MaxQuestionCount)
            {
                throw new InvalidOperationException(
                    "Test set đã đủ 50 câu hỏi. Không thể import thêm.");
            }

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(
                stream,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true);

            var headerLine = await reader.ReadLineAsync();

            if (string.IsNullOrWhiteSpace(headerLine))
            {
                throw new InvalidOperationException("File CSV không có header.");
            }

            var headers = ParseCsvLine(headerLine)
                .Select(NormalizeHeader)
                .ToList();

            var rowNumber = 1;

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                rowNumber++;

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                result.TotalRows++;

                try
                {
                    if (currentCount + result.SuccessCount >= MaxQuestionCount)
                    {
                        result.FailedCount++;
                        result.Errors.Add(
                            $"Dòng {rowNumber}: Test set đã đạt giới hạn 50 câu hỏi.");

                        continue;
                    }

                    var values = ParseCsvLine(line);

                    var row = BuildCsvRow(headers, values);

                    var questionText = GetCsvValue(row, "question");
                    var groundTruth = GetCsvValue(
                        row,
                        "groundtruthanswer",
                        "groundtruth",
                        "answer",
                        "expectedanswer");

                    if (string.IsNullOrWhiteSpace(questionText))
                    {
                        throw new InvalidOperationException("Question không được để trống.");
                    }

                    if (string.IsNullOrWhiteSpace(groundTruth))
                    {
                        throw new InvalidOperationException("GroundTruthAnswer không được để trống.");
                    }

                    var normalizedQuestion = questionText.Trim().ToLower();

                    var duplicated = await _context.ResearchQuestions
                        .AnyAsync(q => q.Question.ToLower() == normalizedQuestion);

                    if (duplicated)
                    {
                        throw new InvalidOperationException(
                            "Question đã tồn tại trong test set.");
                    }

                    var subjectName = GetCsvValue(row, "subjectname", "subject");
                    var chapterName = GetCsvValue(row, "chaptername", "chapter");
                    var sourceDocumentName = GetCsvValue(
                        row,
                        "sourcedocumentname",
                        "sourcedocument",
                        "document",
                        "documentname",
                        "filename");

                    var expectedKeywords = GetCsvValue(
                        row,
                        "expectedkeywords",
                        "keywords");

                    var expectedSource = GetCsvValue(
                        row,
                        "expectedsource",
                        "source",
                        "note",
                        "expectednote");

                    int? subjectId = null;
                    int? chapterId = null;
                    int? sourceDocumentId = null;

                    if (!string.IsNullOrWhiteSpace(subjectName))
                    {
                        var subject = await _context.Subjects
                            .FirstOrDefaultAsync(s =>
                                s.SubjectName.ToLower() == subjectName.Trim().ToLower());

                        if (subject == null)
                        {
                            throw new InvalidOperationException(
                                $"Không tìm thấy môn học: {subjectName}");
                        }

                        subjectId = subject.SubjectId;
                    }

                    if (!string.IsNullOrWhiteSpace(chapterName))
                    {
                        var chapterQuery = _context.Chapters.AsQueryable();

                        if (subjectId.HasValue)
                        {
                            chapterQuery = chapterQuery.Where(c =>
                                c.SubjectId == subjectId.Value);
                        }

                        var chapter = await chapterQuery
                            .FirstOrDefaultAsync(c =>
                                c.ChapterName.ToLower() == chapterName.Trim().ToLower());

                        if (chapter == null)
                        {
                            throw new InvalidOperationException(
                                $"Không tìm thấy chương: {chapterName}");
                        }

                        chapterId = chapter.ChapterId;

                        if (!subjectId.HasValue)
                        {
                            subjectId = chapter.SubjectId;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(sourceDocumentName))
                    {
                        var document = await _context.Documents
                            .FirstOrDefaultAsync(d =>
                                d.Status == "Approved" &&
                                d.OriginalFileName.ToLower() ==
                                sourceDocumentName.Trim().ToLower());

                        if (document == null)
                        {
                            throw new InvalidOperationException(
                                $"Không tìm thấy tài liệu Approved: {sourceDocumentName}");
                        }

                        sourceDocumentId = document.DocumentId;

                        if (!subjectId.HasValue)
                        {
                            subjectId = document.SubjectId;
                        }

                        if (!chapterId.HasValue)
                        {
                            chapterId = document.ChapterId;
                        }
                    }

                    var researchQuestion = new ResearchQuestion
                    {
                        SubjectId = subjectId,
                        ChapterId = chapterId,
                        SourceDocumentId = sourceDocumentId,
                        Question = questionText.Trim(),
                        GroundTruthAnswer = groundTruth.Trim(),
                        ExpectedKeywords = expectedKeywords?.Trim(),
                        ExpectedSource = expectedSource?.Trim(),
                        CreatedAt = DateTime.Now
                    };

                    _context.ResearchQuestions.Add(researchQuestion);

                    await _context.SaveChangesAsync();

                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    result.FailedCount++;

                    result.Errors.Add(
                        $"Dòng {rowNumber}: {ex.Message}");
                }
            }

            return result;
        }

        private static Dictionary<string, string> BuildCsvRow(
    List<string> headers,
    List<string> values)
        {
            var row = new Dictionary<string, string>();

            for (var i = 0; i < headers.Count; i++)
            {
                var value = i < values.Count
                    ? values[i]
                    : string.Empty;

                row[headers[i]] = value;
            }

            return row;
        }

        private static string? GetCsvValue(
            Dictionary<string, string> row,
            params string[] possibleNames)
        {
            foreach (var name in possibleNames)
            {
                var normalizedName = NormalizeHeader(name);

                if (row.TryGetValue(normalizedName, out var value))
                {
                    return value;
                }
            }

            return null;
        }

        private static string NormalizeHeader(string value)
        {
            return value
                .Trim()
                .Replace(" ", "")
                .Replace("_", "")
                .Replace("-", "")
                .ToLowerInvariant();
        }

        private static List<string> ParseCsvLine(string line)
        {
            var values = new List<string>();
            var current = new StringBuilder();
            var insideQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var character = line[i];

                if (character == '"')
                {
                    if (insideQuotes &&
                        i + 1 < line.Length &&
                        line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        insideQuotes = !insideQuotes;
                    }
                }
                else if (character == ',' && !insideQuotes)
                {
                    values.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                {
                    current.Append(character);
                }
            }

            values.Add(current.ToString().Trim());

            return values;
        }
    }
}
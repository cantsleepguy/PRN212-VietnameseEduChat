using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using PRN212_VietnameseEduChat.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using DocumentFormat.OpenXml.Wordprocessing;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    public class TextExtractorService : ITextExtractorService
    {
        public Task<string> ExtractAsync(
            string filePath,
            string extension)
        {
            extension = extension.ToLowerInvariant();

            var text = extension switch
            {
                ".pdf" => ExtractPdf(filePath),
                ".docx" => ExtractDocx(filePath),
                ".pptx" => ExtractPptx(filePath),
                _ => throw new InvalidOperationException(
                    "Định dạng tài liệu không được hỗ trợ.")
            };

            return Task.FromResult(text);
        }

        private string ExtractPdf(string filePath)
        {
            var builder = new StringBuilder();

            using var document = PdfDocument.Open(filePath);

            foreach (var page in document.GetPages())
            {
                builder.AppendLine(page.Text);
            }

            return builder.ToString();
        }

        private string ExtractDocx(string filePath)
        {
            var builder = new StringBuilder();

            using var document = WordprocessingDocument.Open(
                filePath,
                false);

            var body = document.MainDocumentPart?
                .Document
                .Body;

            if (body == null)
                return string.Empty;

            foreach (var paragraph in body.Elements<Paragraph>())
            {
                builder.AppendLine(paragraph.InnerText);
            }

            return builder.ToString();
        }

        private string ExtractPptx(string filePath)
        {
            var builder = new StringBuilder();

            using var document = PresentationDocument.Open(
                filePath,
                false);

            var presentationPart = document.PresentationPart;

            if (presentationPart?.Presentation?.SlideIdList == null)
                return string.Empty;

            var slideIds = presentationPart
                .Presentation
                .SlideIdList
                .Elements<SlideId>();

            foreach (var slideId in slideIds)
            {
                var relationshipId = slideId.RelationshipId?.Value;

                if (string.IsNullOrEmpty(relationshipId))
                    continue;

                var slidePart = (SlidePart)presentationPart
                    .GetPartById(relationshipId);

                var texts = slidePart.Slide
                    .Descendants<DocumentFormat.OpenXml.Drawing.Text>();

                foreach (var text in texts)
                {
                    builder.Append(text.Text);
                    builder.Append(' ');
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}

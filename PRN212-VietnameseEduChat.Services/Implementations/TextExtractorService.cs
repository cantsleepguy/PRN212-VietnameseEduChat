using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Wordprocessing;
using ImageMagick;
using PRN212_VietnameseEduChat.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    public class TextExtractorService : ITextExtractorService
    {
        private readonly IOcrService _ocrService;

        private const int PdfPageTextThreshold = 50;
        private const int PdfOcrDensity = 200;

        public TextExtractorService(IOcrService ocrService)
        {
            _ocrService = ocrService;
        }

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

            var pagesNeedOcr = new List<int>();

            using (var document = PdfDocument.Open(filePath))
            {
                var pageIndex = 0;

                foreach (var page in document.GetPages())
                {
                    pageIndex++;

                    var pageText = page.Text?.Trim() ?? string.Empty;

                    builder.AppendLine($"===== PDF - Trang {pageIndex} =====");

                    if (!string.IsNullOrWhiteSpace(pageText))
                    {
                        builder.AppendLine(pageText);
                    }

                    if (pageText.Length < PdfPageTextThreshold)
                    {
                        pagesNeedOcr.Add(pageIndex - 1);
                    }

                    builder.AppendLine();
                }
            }

            if (pagesNeedOcr.Count > 0)
            {
                builder.AppendLine("===== OCR từ các trang PDF dạng ảnh =====");

                foreach (var pageIndex in pagesNeedOcr)
                {
                    var ocrText = ExtractPdfPageByOcr(
                        filePath,
                        pageIndex);

                    if (!string.IsNullOrWhiteSpace(ocrText))
                    {
                        builder.AppendLine(
                            $"----- OCR PDF - Trang {pageIndex + 1} -----");

                        builder.AppendLine(ocrText);
                        builder.AppendLine();
                    }
                }
            }

            return builder.ToString();
        }

        private string ExtractPdfPageByOcr(
            string filePath,
            int pageIndex)
        {
            try
            {
                var settings = new MagickReadSettings
                {
                    Density = new Density(PdfOcrDensity, PdfOcrDensity),
                    FrameIndex = (uint)pageIndex,
                    FrameCount = 1u
                };

                using var images = new MagickImageCollection();

                images.Read(filePath, settings);

                if (images.Count == 0)
                {
                    return string.Empty;
                }

                using var image = images[0];

                image.Format = MagickFormat.Png;

                using var memoryStream = new MemoryStream();

                image.Write(memoryStream);

                return _ocrService.ExtractTextFromImageBytes(
                    memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Không thể OCR trang PDF {pageIndex + 1}. " +
                    $"Hãy kiểm tra Ghostscript đã được cài chưa. Chi tiết: {ex.Message}");
            }
        }

        private string ExtractDocx(string filePath)
        {
            var builder = new StringBuilder();

            using var document = WordprocessingDocument.Open(
                filePath,
                false);

            var body = document.MainDocumentPart?
                .Document?
                .Body;

            builder.AppendLine("===== Nội dung text trong DOCX =====");

            if (body != null)
            {
                foreach (var paragraph in body.Elements<Paragraph>())
                {
                    if (!string.IsNullOrWhiteSpace(paragraph.InnerText))
                    {
                        builder.AppendLine(paragraph.InnerText);
                    }
                }
            }

            builder.AppendLine();

            builder.AppendLine("===== OCR từ hình ảnh trong DOCX =====");

            var imageIndex = 0;

            if (document.MainDocumentPart != null)
            {
                foreach (var imagePart in GetImageParts(document.MainDocumentPart))
                {
                    imageIndex++;

                    var imageBytes = ReadPartBytes(imagePart);

                    var ocrText = _ocrService.ExtractTextFromImageBytes(
                        imageBytes);

                    if (!string.IsNullOrWhiteSpace(ocrText))
                    {
                        builder.AppendLine(
                            $"----- Ảnh DOCX {imageIndex} -----");

                        builder.AppendLine(ocrText);
                        builder.AppendLine();
                    }
                }
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
            {
                return string.Empty;
            }

            var slideIds = presentationPart
                .Presentation
                .SlideIdList
                .Elements<SlideId>();

            var slideNumber = 0;

            foreach (var slideId in slideIds)
            {
                slideNumber++;

                var relationshipId = slideId.RelationshipId?.Value;

                if (string.IsNullOrEmpty(relationshipId))
                {
                    continue;
                }

                var slidePart = (SlidePart)presentationPart
                    .GetPartById(relationshipId);

                if (slidePart == null)
                {
                    continue;
                }

                builder.AppendLine($"===== Slide {slideNumber} =====");

                var texts = slidePart.Slide?
                    .Descendants<DocumentFormat.OpenXml.Drawing.Text>() ?? Enumerable.Empty<DocumentFormat.OpenXml.Drawing.Text>();

                foreach (var text in texts)
                {
                    if (!string.IsNullOrWhiteSpace(text.Text))
                    {
                        builder.Append(text.Text);
                        builder.Append(' ');
                    }
                }

                builder.AppendLine();
                builder.AppendLine();

                var imageIndex = 0;

                foreach (var imagePart in slidePart.ImageParts)
                {
                    imageIndex++;

                    var imageBytes = ReadPartBytes(imagePart);

                    var ocrText = _ocrService.ExtractTextFromImageBytes(
                        imageBytes);

                    if (!string.IsNullOrWhiteSpace(ocrText))
                    {
                        builder.AppendLine(
                            $"----- OCR ảnh {imageIndex} trong slide {slideNumber} -----");

                        builder.AppendLine(ocrText);
                        builder.AppendLine();
                    }
                }
            }

            return builder.ToString();
        }

        private static byte[] ReadPartBytes(OpenXmlPart part)
        {
            using var stream = part.GetStream();
            using var memoryStream = new MemoryStream();

            stream.CopyTo(memoryStream);

            return memoryStream.ToArray();
        }

        private static IEnumerable<ImagePart> GetImageParts(
            OpenXmlPartContainer container)
        {
            foreach (var partReference in container.Parts)
            {
                var part = partReference.OpenXmlPart;

                if (part is ImagePart imagePart)
                {
                    yield return imagePart;
                }

                if (part is OpenXmlPartContainer childContainer)
                {
                    foreach (var childImagePart in GetImageParts(childContainer))
                    {
                        yield return childImagePart;
                    }
                }
            }
        }
    }
}
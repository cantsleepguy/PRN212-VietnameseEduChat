using Microsoft.AspNetCore.Hosting;
using PRN212_VietnameseEduChat.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace PRN212_VietnameseEduChat.Services.Implementations
{
    public class OcrService : IOcrService
    {
        private readonly string _tessDataPath;

        public OcrService(IWebHostEnvironment environment)
        {
            _tessDataPath = Path.Combine(
                environment.ContentRootPath,
                "tessdata");
        }

        public string ExtractTextFromImageBytes(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
            {
                return string.Empty;
            }

            if (!Directory.Exists(_tessDataPath))
            {
                throw new DirectoryNotFoundException(
                    $"Không tìm thấy thư mục tessdata tại: {_tessDataPath}");
            }

            var vieFile = Path.Combine(_tessDataPath, "vie.traineddata");
            var engFile = Path.Combine(_tessDataPath, "eng.traineddata");

            if (!File.Exists(vieFile))
            {
                throw new FileNotFoundException(
                    $"Không tìm thấy file vie.traineddata tại: {vieFile}");
            }

            if (!File.Exists(engFile))
            {
                throw new FileNotFoundException(
                    $"Không tìm thấy file eng.traineddata tại: {engFile}");
            }

            try
            {
                using var engine = new TesseractEngine(
                    _tessDataPath,
                    "vie+eng",
                    EngineMode.Default);

                using var image = Pix.LoadFromMemory(imageBytes);

                using var page = engine.Process(image);

                return page.GetText()?.Trim() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
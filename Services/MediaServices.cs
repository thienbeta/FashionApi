using FashionApi.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using System.IO.Compression;
using System.Threading.Tasks;

namespace FashionApi.Services
{
    public class MediaServices : IMediaServices
    {
        private readonly IWebHostEnvironment _env;

        public MediaServices(IWebHostEnvironment env)
        {
            _env = env;
        }

        private string GetUploadPath(string subFolder)
        {
            var basePath = Path.Combine(_env.WebRootPath, "uploads", subFolder);
            Directory.CreateDirectory(basePath);
            return basePath;
        }

        public async Task<string> SaveOptimizedImageAsync(IFormFile file, string subFolder = "images")
        {
            var uploads = GetUploadPath(subFolder);

            var fileName = Guid.NewGuid() + ".webp";
            var outputPath = Path.Combine(uploads, fileName);

            using var image = await Image.LoadAsync(file.OpenReadStream());

            if (image.Width > 2500)
                image.Mutate(x => x.Resize(2500, 0)); // Resize theo chiều rộng, giữ tỉ lệ

            await image.SaveAsWebpAsync(outputPath, new WebpEncoder
            {
                Quality = 80,
                FileFormat = WebpFileFormatType.Lossy
            });

            return $"/uploads/{subFolder}/{fileName}";
        }

        public async Task<string> SaveCompressedPdfAsync(IFormFile file, string subFolder = "pdfs")
        {
            var uploads = GetUploadPath(subFolder);

            var inputPath = Path.Combine(uploads, "temp_" + Guid.NewGuid() + ".pdf");
            var outputFileName = "compressed_" + Guid.NewGuid() + ".pdf";
            var outputPath = Path.Combine(uploads, outputFileName);

            using (var stream = new FileStream(inputPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var args = $"-sDEVICE=pdfwrite -dCompatibilityLevel=1.4 -dPDFSETTINGS=/ebook " +
                       $"-dNOPAUSE -dQUIET -dBATCH -sOutputFile=\"{outputPath}\" \"{inputPath}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = "gswin64c",
                Arguments = args,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(startInfo);
            await process.WaitForExitAsync();

            File.Delete(inputPath);
            return $"/uploads/{subFolder}/{outputFileName}";
        }

        public async Task<string> SaveAsZipAsync(IFormFile file, string subFolder = "archives")
        {
            var uploads = GetUploadPath(subFolder);
            var zipFileName = Path.GetFileNameWithoutExtension(file.FileName) + "_" + Guid.NewGuid() + ".zip";
            var zipPath = Path.Combine(uploads, zipFileName);

            using (var fs = new FileStream(zipPath, FileMode.Create))
            using (var archive = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                var entry = archive.CreateEntry(file.FileName);
                using var entryStream = entry.Open();
                await file.CopyToAsync(entryStream);
            }

            return $"/uploads/{subFolder}/{zipFileName}";
        }

        public async Task<bool> DeleteImageAsync(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return false;

            try
            {
                // Remove leading slash and construct full path
                var fullPath = Path.Combine(_env.WebRootPath, imagePath.TrimStart('/'));

                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                    return true;
                }

                return false;
            }
            catch
            {
                // Log error if needed, but return false to indicate failure
                return false;
            }
        }
    }
}
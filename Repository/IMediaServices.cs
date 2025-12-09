using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace FashionApi.Repository
{
    public interface IMediaServices
    {
        Task<string> SaveOptimizedImageAsync(IFormFile file, string subFolder = "images");
        Task<string> SaveCompressedPdfAsync(IFormFile file, string subFolder = "pdfs");
        Task<string> SaveAsZipAsync(IFormFile file, string subFolder = "archives");
        Task<bool> DeleteImageAsync(string imagePath);
    }
}

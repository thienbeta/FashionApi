using FashionApi.DTO;
using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Models.View;
using FashionApi.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace FashionApi.Services
{
    public class HashtagServices : IHashtagServices
    {
        private readonly string _filePath;
        private readonly string _imageFolderPath;
        private readonly IWebHostEnvironment _env;

        public HashtagServices(IWebHostEnvironment env)
        {
            _env = env;
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), "hashtag.json");
            _imageFolderPath = Path.Combine(_env.WebRootPath, "DanhMuc");

            if (!Directory.Exists(_imageFolderPath))
                Directory.CreateDirectory(_imageFolderPath);
        }

        private async Task<List<Hashtag>> ReadFromFileAsync()
        {
            if (!File.Exists(_filePath)) return new List<Hashtag>();
            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<Hashtag>>(json) ?? new List<Hashtag>();
        }

        private async Task WriteToFileAsync(List<Hashtag> hashtags)
        {
            var json = JsonSerializer.Serialize(hashtags, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }

        private async Task<string?> SaveImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var path = Path.Combine(_imageFolderPath, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/DanhMuc/{fileName}";
        }

        public async Task<List<HashtagView>> GetAllAsync()
        {
            var list = await ReadFromFileAsync();
            return list.Select(h => new HashtagView
            {
                MaHashtag = h.MaHashtag,
                TenHashtag = h.TenHashtag,
                MoTa = h.MoTa,
                HinhAnh = h.HinhAnh,
                NgayTao = h.NgayTao,
                TrangThai = h.TrangThai
            }).ToList();
        }

        public async Task<HashtagView?> GetByIdAsync(int id)
        {
            var list = await ReadFromFileAsync();
            var hashtag = list.FirstOrDefault(h => h.MaHashtag == id);
            if (hashtag == null) return null;

            return new HashtagView
            {
                MaHashtag = hashtag.MaHashtag,
                TenHashtag = hashtag.TenHashtag,
                MoTa = hashtag.MoTa,
                HinhAnh = hashtag.HinhAnh,
                NgayTao = hashtag.NgayTao,
                TrangThai = hashtag.TrangThai
            };
        }

        public async Task<List<HashtagView>> SearchAsync(string keyword)
        {
            var list = await ReadFromFileAsync();

            bool isInt = int.TryParse(keyword, out int intKeyword);

            var result = list.Where(h =>
                (!string.IsNullOrEmpty(h.TenHashtag) && h.TenHashtag.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(h.MoTa) && h.MoTa.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                (isInt && h.MaHashtag == intKeyword) ||
                (isInt && h.TrangThai == intKeyword)
            )
            .Select(h => new HashtagView
            {
                MaHashtag = h.MaHashtag,
                TenHashtag = h.TenHashtag,
                MoTa = h.MoTa,
                HinhAnh = h.HinhAnh,
                NgayTao = h.NgayTao,
                TrangThai = h.TrangThai
            }).ToList();

            return result;
        }



        public async Task<List<SanPham>> GetSanPhamsByHashtagIdAsync(int id)
        {
            var list = await ReadFromFileAsync();
            return list.FirstOrDefault(h => h.MaHashtag == id)?.SanPhams.ToList() ?? new List<SanPham>();
        }

        public async Task<bool> CreateAsync(HashtagCreate model, IFormFile? file)
        {
            var list = await ReadFromFileAsync();

            bool isDuplicate = list.Any(h =>
                string.Equals(h.TenHashtag.Trim(), model.TenHashtag.Trim(), StringComparison.OrdinalIgnoreCase));

            if (isDuplicate)
                return false;

            int newId = list.Any() ? list.Max(h => h.MaHashtag) + 1 : 1;
            string? imageUrl = await SaveImageAsync(file);

            var newHashtag = new Hashtag
            {
                MaHashtag = newId,
                TenHashtag = model.TenHashtag.Trim(),
                MoTa = model.MoTa,
                HinhAnh = imageUrl,
                NgayTao = DateTime.UtcNow,
                TrangThai = 1
            };

            list.Add(newHashtag);
            await WriteToFileAsync(list);
            return true;
        }


        public async Task<bool> UpdateAsync(HashtagEdit model, IFormFile? file)
        {
            var list = await ReadFromFileAsync();
            var hashtag = list.FirstOrDefault(h => h.MaHashtag == model.MaHashtag);
            if (hashtag == null) return false;

            bool isDuplicate = list.Any(h =>
                h.MaHashtag != model.MaHashtag &&
                h.TenHashtag.Trim().Equals(model.TenHashtag.Trim(), StringComparison.OrdinalIgnoreCase));

            if (isDuplicate)
                return false;

            if (file != null)
                hashtag.HinhAnh = await SaveImageAsync(file);

            hashtag.TenHashtag = model.TenHashtag.Trim();
            hashtag.MoTa = model.MoTa;
            hashtag.TrangThai = model.TrangThai;

            await WriteToFileAsync(list);
            return true;
        }


        public async Task<bool> DeleteAsync(int id)
        {
            var list = await ReadFromFileAsync();
            var hashtag = list.FirstOrDefault(h => h.MaHashtag == id);
            if (hashtag == null) return false;

            list.Remove(hashtag);
            await WriteToFileAsync(list);
            return true;
        }
    }
}

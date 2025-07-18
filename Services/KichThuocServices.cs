using FashionApi.DTO;
using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Models.View;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using FashionApi.Repository;
using System.Text.Json;

namespace FashionApi.Services
{
    public class KichThuocServices : IKichThuocServices
    {
        private readonly string _filePath;
        private readonly string _imageFolderPath;
        private readonly IWebHostEnvironment _env;

        public KichThuocServices(IWebHostEnvironment env)
        {
            _env = env;
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), "kichthuoc.json");
            _imageFolderPath = Path.Combine(_env.WebRootPath, "DanhMuc");

            if (!Directory.Exists(_imageFolderPath))
                Directory.CreateDirectory(_imageFolderPath);
        }

        private async Task<List<KichThuoc>> ReadFromFileAsync()
        {
            if (!File.Exists(_filePath)) return new List<KichThuoc>();
            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<KichThuoc>>(json) ?? new List<KichThuoc>();
        }

        private async Task WriteToFileAsync(List<KichThuoc> kichThuocs)
        {
            var json = JsonSerializer.Serialize(kichThuocs, new JsonSerializerOptions { WriteIndented = true });
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

        public async Task<List<KichThuocView>> GetAllAsync()
        {
            var list = await ReadFromFileAsync();
            return list.Select(k => new KichThuocView
            {
                MaKichThuoc = k.MaKichThuoc,
                TenKichThuoc = k.TenKichThuoc,
                MoTa = k.MoTa,
                HinhAnh = k.HinhAnh,
                NgayTao = k.NgayTao,
                TrangThai = k.TrangThai
            }).ToList();
        }

        public async Task<KichThuocView?> GetByIdAsync(int id)
        {
            var list = await ReadFromFileAsync();
            var item = list.FirstOrDefault(k => k.MaKichThuoc == id);
            if (item == null) return null;

            return new KichThuocView
            {
                MaKichThuoc = item.MaKichThuoc,
                TenKichThuoc = item.TenKichThuoc,
                MoTa = item.MoTa,
                HinhAnh = item.HinhAnh,
                NgayTao = item.NgayTao,
                TrangThai = item.TrangThai
            };
        }

        public async Task<List<KichThuocView>> SearchAsync(string keyword)
        {
            var list = await ReadFromFileAsync();

            bool isInt = int.TryParse(keyword, out int intKeyword);

            var result = list.Where(k =>
                (!string.IsNullOrEmpty(k.TenKichThuoc) && k.TenKichThuoc.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(k.MoTa) && k.MoTa.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                (isInt && k.MaKichThuoc == intKeyword) ||
                (isInt && k.TrangThai == intKeyword)
            )
            .Select(k => new KichThuocView
            {
                MaKichThuoc = k.MaKichThuoc,
                TenKichThuoc = k.TenKichThuoc,
                MoTa = k.MoTa,
                HinhAnh = k.HinhAnh,
                NgayTao = k.NgayTao,
                TrangThai = k.TrangThai
            }).ToList();

            return result;
        }


        public async Task<List<SanPham>> GetSanPhamsByKichThuocIdAsync(int id)
        {
            var list = await ReadFromFileAsync();
            return list.FirstOrDefault(k => k.MaKichThuoc == id)?.SanPhams.ToList() ?? new List<SanPham>();
        }

        public async Task<bool> CreateAsync(KichThuocCreate model, IFormFile? file)
        {
            var list = await ReadFromFileAsync();

            if (list.Any(k => k.TenKichThuoc.Trim().Equals(model.TenKichThuoc.Trim(), StringComparison.OrdinalIgnoreCase)))
                return false;

            int newId = list.Any() ? list.Max(k => k.MaKichThuoc) + 1 : 1;
            string? imageUrl = await SaveImageAsync(file);

            var newItem = new KichThuoc
            {
                MaKichThuoc = newId,
                TenKichThuoc = model.TenKichThuoc.Trim(),
                MoTa = model.MoTa,
                HinhAnh = imageUrl,
                NgayTao = DateTime.UtcNow,
                TrangThai = 1
            };

            list.Add(newItem);
            await WriteToFileAsync(list);
            return true;
        }

        public async Task<bool> UpdateAsync(KichThuocEdit model, IFormFile? file)
        {
            var list = await ReadFromFileAsync();
            var item = list.FirstOrDefault(k => k.MaKichThuoc == model.MaKichThuoc);
            if (item == null) return false;

            if (list.Any(k => k.MaKichThuoc != model.MaKichThuoc &&
                             k.TenKichThuoc.Trim().Equals(model.TenKichThuoc.Trim(), StringComparison.OrdinalIgnoreCase)))
                return false;

            if (file != null)
                item.HinhAnh = await SaveImageAsync(file);

            item.TenKichThuoc = model.TenKichThuoc.Trim();
            item.MoTa = model.MoTa;
            item.TrangThai = model.TrangThai;

            await WriteToFileAsync(list);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var list = await ReadFromFileAsync();
            var item = list.FirstOrDefault(k => k.MaKichThuoc == id);
            if (item == null) return false;

            list.Remove(item);
            await WriteToFileAsync(list);
            return true;
        }
    }
}

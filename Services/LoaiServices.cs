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
    public class LoaiServices : ILoaiServices
    {
        private readonly string _filePath;
        private readonly string _imageFolderPath;
        private readonly IWebHostEnvironment _env;

        public LoaiServices(IWebHostEnvironment env)
        {
            _env = env;
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), "loai.json");
            _imageFolderPath = Path.Combine(_env.WebRootPath, "DanhMuc");

            if (!Directory.Exists(_imageFolderPath))
                Directory.CreateDirectory(_imageFolderPath);
        }

        private async Task<List<Loai>> ReadFromFileAsync()
        {
            if (!File.Exists(_filePath)) return new List<Loai>();
            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<Loai>>(json) ?? new List<Loai>();
        }

        private async Task WriteToFileAsync(List<Loai> loais)
        {
            var json = JsonSerializer.Serialize(loais, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }

        private async Task<string?> SaveImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var path = Path.Combine(_imageFolderPath, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/DanhMuc/{fileName}";
        }

        public async Task<List<LoaiView>> GetAllAsync()
        {
            var list = await ReadFromFileAsync();
            return list.Select(l => new LoaiView
            {
                MaLoai = l.MaLoai,
                TenLoai = l.TenLoai,
                MoTa = l.MoTa,
                KiHieu = l.KiHieu,
                HinhAnh = l.HinhAnh,
                NgayTao = l.NgayTao,
                TrangThai = l.TrangThai
            }).ToList();
        }

        public async Task<LoaiView?> GetByIdAsync(int id)
        {
            var list = await ReadFromFileAsync();
            var loai = list.FirstOrDefault(l => l.MaLoai == id);
            if (loai == null) return null;

            return new LoaiView
            {
                MaLoai = loai.MaLoai,
                TenLoai = loai.TenLoai,
                MoTa = loai.MoTa,
                KiHieu = loai.KiHieu,
                HinhAnh = loai.HinhAnh,
                NgayTao = loai.NgayTao,
                TrangThai = loai.TrangThai
            };
        }

        public async Task<List<LoaiView>> SearchAsync(string keyword)
        {
            var list = await ReadFromFileAsync();
            bool isInt = int.TryParse(keyword, out int intKeyword);

            return list.Where(l =>
                (!string.IsNullOrEmpty(l.TenLoai) && l.TenLoai.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(l.MoTa) && l.MoTa.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(l.KiHieu) && l.KiHieu.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                (isInt && l.MaLoai == intKeyword) ||
                (isInt && l.TrangThai == intKeyword)
            )
            .Select(l => new LoaiView
            {
                MaLoai = l.MaLoai,
                TenLoai = l.TenLoai,
                MoTa = l.MoTa,
                KiHieu = l.KiHieu,
                HinhAnh = l.HinhAnh,
                NgayTao = l.NgayTao,
                TrangThai = l.TrangThai
            }).ToList();
        }

        public async Task<List<SanPham>> GetSanPhamsByLoaiIdAsync(int id)
        {
            var list = await ReadFromFileAsync();
            return list.FirstOrDefault(l => l.MaLoai == id)?.SanPhams.ToList() ?? new List<SanPham>();
        }

        public async Task<bool> CreateAsync(LoaiCreate model, IFormFile? file)
        {
            var list = await ReadFromFileAsync();

            bool isDuplicate = list.Any(l =>
                l.TenLoai.Trim().Equals(model.TenLoai.Trim(), StringComparison.OrdinalIgnoreCase) ||
                l.KiHieu.Trim().Equals(model.KiHieu.Trim(), StringComparison.OrdinalIgnoreCase)
            );

            if (isDuplicate) return false;

            int newId = list.Any() ? list.Max(l => l.MaLoai) + 1 : 1;
            string? imageUrl = await SaveImageAsync(file);

            var newLoai = new Loai
            {
                MaLoai = newId,
                TenLoai = model.TenLoai.Trim(),
                MoTa = model.MoTa,
                KiHieu = model.KiHieu.Trim(),
                HinhAnh = imageUrl,
                NgayTao = DateTime.UtcNow,
                TrangThai = 1
            };

            list.Add(newLoai);
            await WriteToFileAsync(list);
            return true;
        }

        public async Task<bool> UpdateAsync(LoaiEdit model, IFormFile? file)
        {
            var list = await ReadFromFileAsync();
            var loai = list.FirstOrDefault(l => l.MaLoai == model.MaLoai);
            if (loai == null) return false;

            bool isDuplicate = list.Any(l =>
                l.MaLoai != model.MaLoai &&
                (l.TenLoai.Trim().Equals(model.TenLoai.Trim(), StringComparison.OrdinalIgnoreCase) ||
                 l.KiHieu.Trim().Equals(model.KiHieu.Trim(), StringComparison.OrdinalIgnoreCase))
            );

            if (isDuplicate) return false;

            if (file != null)
                loai.HinhAnh = await SaveImageAsync(file);

            loai.TenLoai = model.TenLoai.Trim();
            loai.MoTa = model.MoTa;
            loai.KiHieu = model.KiHieu.Trim();
            loai.TrangThai = model.TrangThai;

            await WriteToFileAsync(list);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var list = await ReadFromFileAsync();
            var loai = list.FirstOrDefault(l => l.MaLoai == id);
            if (loai == null) return false;

            list.Remove(loai);
            await WriteToFileAsync(list);
            return true;
        }
    }
}

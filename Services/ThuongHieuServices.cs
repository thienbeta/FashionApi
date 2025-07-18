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
    public class ThuongHieuServices : IThuongHieuServices
    {
        private readonly string _filePath;
        private readonly string _imageFolderPath;
        private readonly IWebHostEnvironment _env;

        public ThuongHieuServices(IWebHostEnvironment env)
        {
            _env = env;
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), "thuonghieu.json");
            _imageFolderPath = Path.Combine(_env.WebRootPath, "DanhMuc");

            if (!Directory.Exists(_imageFolderPath))
                Directory.CreateDirectory(_imageFolderPath);
        }

        private async Task<List<ThuongHieu>> ReadFromFileAsync()
        {
            if (!File.Exists(_filePath)) return new List<ThuongHieu>();
            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<ThuongHieu>>(json) ?? new List<ThuongHieu>();
        }

        private async Task WriteToFileAsync(List<ThuongHieu> list)
        {
            var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }

        private async Task<string?> SaveImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var path = Path.Combine(_imageFolderPath, fileName);

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/DanhMuc/{fileName}";
        }

        public async Task<List<ThuongHieuView>> GetAllAsync()
        {
            var list = await ReadFromFileAsync();
            return list.Select(th => new ThuongHieuView
            {
                MaThuongHieu = th.MaThuongHieu,
                TenThuongHieu = th.TenThuongHieu,
                MoTa = th.MoTa,
                HinhAnh = th.HinhAnh,
                NgayTao = th.NgayTao,
                TrangThai = th.TrangThai
            }).ToList();
        }

        public async Task<ThuongHieuView?> GetByIdAsync(int id)
        {
            var list = await ReadFromFileAsync();
            var th = list.FirstOrDefault(x => x.MaThuongHieu == id);
            if (th == null) return null;

            return new ThuongHieuView
            {
                MaThuongHieu = th.MaThuongHieu,
                TenThuongHieu = th.TenThuongHieu,
                MoTa = th.MoTa,
                HinhAnh = th.HinhAnh,
                NgayTao = th.NgayTao,
                TrangThai = th.TrangThai
            };
        }

        public async Task<List<ThuongHieuView>> SearchAsync(string keyword)
        {
            var list = await ReadFromFileAsync();
            bool isInt = int.TryParse(keyword, out int intKeyword);

            var result = list.Where(th =>
                (!string.IsNullOrEmpty(th.TenThuongHieu) && th.TenThuongHieu.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(th.MoTa) && th.MoTa.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                (isInt && th.MaThuongHieu == intKeyword) ||
                (isInt && th.TrangThai == intKeyword)
            ).Select(th => new ThuongHieuView
            {
                MaThuongHieu = th.MaThuongHieu,
                TenThuongHieu = th.TenThuongHieu,
                MoTa = th.MoTa,
                HinhAnh = th.HinhAnh,
                NgayTao = th.NgayTao,
                TrangThai = th.TrangThai
            }).ToList();

            return result;
        }

        public async Task<List<SanPham>> GetSanPhamsByThuongHieuIdAsync(int id)
        {
            var list = await ReadFromFileAsync();
            return list.FirstOrDefault(th => th.MaThuongHieu == id)?.SanPhams.ToList() ?? new List<SanPham>();
        }

        public async Task<bool> CreateAsync(ThuongHieuCreate model, IFormFile? file)
        {
            var list = await ReadFromFileAsync();

            bool isDuplicate = list.Any(th =>
                th.TenThuongHieu.Trim().Equals(model.TenThuongHieu.Trim(), StringComparison.OrdinalIgnoreCase));
            if (isDuplicate) return false;

            int newId = list.Any() ? list.Max(th => th.MaThuongHieu) + 1 : 1;
            string? imageUrl = await SaveImageAsync(file);

            var newTH = new ThuongHieu
            {
                MaThuongHieu = newId,
                TenThuongHieu = model.TenThuongHieu.Trim(),
                MoTa = model.MoTa,
                HinhAnh = imageUrl,
                NgayTao = DateTime.UtcNow,
                TrangThai = 1
            };

            list.Add(newTH);
            await WriteToFileAsync(list);
            return true;
        }

        public async Task<bool> UpdateAsync(ThuongHieuEdit model, IFormFile? file)
        {
            var list = await ReadFromFileAsync();
            var th = list.FirstOrDefault(x => x.MaThuongHieu == model.MaThuongHieu);
            if (th == null) return false;

            bool isDuplicate = list.Any(x =>
                x.MaThuongHieu != model.MaThuongHieu &&
                x.TenThuongHieu.Trim().Equals(model.TenThuongHieu.Trim(), StringComparison.OrdinalIgnoreCase));
            if (isDuplicate) return false;

            if (file != null)
                th.HinhAnh = await SaveImageAsync(file);

            th.TenThuongHieu = model.TenThuongHieu.Trim();
            th.MoTa = model.MoTa;
            th.TrangThai = model.TrangThai;

            await WriteToFileAsync(list);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var list = await ReadFromFileAsync();
            var th = list.FirstOrDefault(x => x.MaThuongHieu == id);
            if (th == null) return false;

            list.Remove(th);
            await WriteToFileAsync(list);
            return true;
        }
    }
}

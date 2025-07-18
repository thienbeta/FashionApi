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
    public class MauServices : IMauServices
    {
        private readonly string _filePath;
        private readonly string _imageFolderPath;
        private readonly IWebHostEnvironment _env;

        public MauServices(IWebHostEnvironment env)
        {
            _env = env;
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), "mau.json");
            _imageFolderPath = Path.Combine(_env.WebRootPath, "DanhMuc");

            if (!Directory.Exists(_imageFolderPath))
                Directory.CreateDirectory(_imageFolderPath);
        }

        private async Task<List<Mau>> ReadFromFileAsync()
        {
            if (!File.Exists(_filePath)) return new List<Mau>();
            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<List<Mau>>(json) ?? new List<Mau>();
        }

        private async Task WriteToFileAsync(List<Mau> list)
        {
            var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }

        private async Task<string?> SaveImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var path = Path.Combine(_imageFolderPath, fileName);

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/DanhMuc/{fileName}";
        }

        public async Task<List<MauView>> GetAllAsync()
        {
            var list = await ReadFromFileAsync();
            return list.Select(m => new MauView
            {
                MaMau = m.MaMau,
                TenMau = m.TenMau,
                MoTa = m.MoTa,
                CodeMau = m.CodeMau,
                HinhAnh = m.HinhAnh,
                NgayTao = m.NgayTao,
                TrangThai = m.TrangThai
            }).ToList();
        }

        public async Task<MauView?> GetByIdAsync(int id)
        {
            var list = await ReadFromFileAsync();
            var item = list.FirstOrDefault(m => m.MaMau == id);
            if (item == null) return null;

            return new MauView
            {
                MaMau = item.MaMau,
                TenMau = item.TenMau,
                MoTa = item.MoTa,
                CodeMau = item.CodeMau,
                HinhAnh = item.HinhAnh,
                NgayTao = item.NgayTao,
                TrangThai = item.TrangThai
            };
        }

        public async Task<List<MauView>> SearchAsync(string keyword)
        {
            var list = await ReadFromFileAsync();
            bool isInt = int.TryParse(keyword, out int intKeyword);

            var result = list.Where(m =>
                (!string.IsNullOrEmpty(m.TenMau) && m.TenMau.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(m.MoTa) && m.MoTa.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(m.CodeMau) && m.CodeMau.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                (isInt && m.MaMau == intKeyword) ||
                (isInt && m.TrangThai == intKeyword)
            ).Select(m => new MauView
            {
                MaMau = m.MaMau,
                TenMau = m.TenMau,
                MoTa = m.MoTa,
                CodeMau = m.CodeMau,
                HinhAnh = m.HinhAnh,
                NgayTao = m.NgayTao,
                TrangThai = m.TrangThai
            }).ToList();

            return result;
        }

        public async Task<List<SanPham>> GetSanPhamsByMauIdAsync(int id)
        {
            var list = await ReadFromFileAsync();
            return list.FirstOrDefault(m => m.MaMau == id)?.SanPhams.ToList() ?? new List<SanPham>();
        }

        public async Task<bool> CreateAsync(MauCreate model, IFormFile? file)
        {
            var list = await ReadFromFileAsync();

            bool isDuplicate = list.Any(m =>
                m.CodeMau.Trim().Equals(model.CodeMau.Trim(), StringComparison.OrdinalIgnoreCase) ||
                m.TenMau?.Trim().Equals(model.TenMau?.Trim(), StringComparison.OrdinalIgnoreCase) == true);

            if (isDuplicate) return false;

            int newId = list.Count > 0 ? list.Max(m => m.MaMau) + 1 : 1;
            string? imageUrl = await SaveImageAsync(file);

            var newItem = new Mau
            {
                MaMau = newId,
                TenMau = model.TenMau?.Trim(),
                MoTa = model.MoTa,
                CodeMau = model.CodeMau.Trim(),
                HinhAnh = imageUrl,
                NgayTao = DateTime.UtcNow,
                TrangThai = 1
            };

            list.Add(newItem);
            await WriteToFileAsync(list);
            return true;
        }

        public async Task<bool> UpdateAsync(MauEdit model, IFormFile? file)
        {
            var list = await ReadFromFileAsync();
            var item = list.FirstOrDefault(m => m.MaMau == model.MaMau);
            if (item == null) return false;

            bool isDuplicate = list.Any(m =>
                m.MaMau != model.MaMau &&
                (m.CodeMau.Trim().Equals(model.CodeMau.Trim(), StringComparison.OrdinalIgnoreCase) ||
                 m.TenMau?.Trim().Equals(model.TenMau?.Trim(), StringComparison.OrdinalIgnoreCase) == true));
            if (isDuplicate) return false;

            if (file != null)
                item.HinhAnh = await SaveImageAsync(file);

            item.TenMau = model.TenMau?.Trim();
            item.MoTa = model.MoTa;
            item.CodeMau = model.CodeMau.Trim();
            item.TrangThai = model.TrangThai;

            await WriteToFileAsync(list);
            return true;
        }


        public async Task<bool> DeleteAsync(int id)
        {
            var list = await ReadFromFileAsync();
            var item = list.FirstOrDefault(m => m.MaMau == id);
            if (item == null) return false;

            list.Remove(item);
            await WriteToFileAsync(list);
            return true;
        }
    }
}

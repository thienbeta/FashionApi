using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FashionApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SanPhamController : ControllerBase
    {
        private readonly ISanPhamServices _sanPhamServices;

        public SanPhamController(ISanPhamServices sanPhamServices)
        {
            _sanPhamServices = sanPhamServices;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _sanPhamServices.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{maSanPham}")]
        public async Task<IActionResult> GetById(string maSanPham)
        {
            var result = await _sanPhamServices.GetByIdAsync(maSanPham);
            if (result == null)
                return NotFound(new { message = "Không tìm thấy sản phẩm." });

            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            var result = await _sanPhamServices.SearchAsync(keyword);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] SanPhamCreate model)
        {
            var success = await _sanPhamServices.CreateAsync(model);
            if (!success)
                return BadRequest(new { message = "Tạo sản phẩm thất bại. Kiểm tra dữ liệu." });

            return Ok(new { message = "Tạo sản phẩm thành công." });
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] SanPhamEdit model)
        {
            var success = await _sanPhamServices.UpdateAsync(model);
            if (!success)
                return BadRequest(new { message = "Cập nhật sản phẩm thất bại. Kiểm tra dữ liệu hoặc sản phẩm không tồn tại." });

            return Ok(new { message = "Cập nhật sản phẩm thành công." });
        }

        [HttpDelete("delete/{maSanPham}")]
        public async Task<IActionResult> Delete(string maSanPham)
        {
            var success = await _sanPhamServices.DeleteAsync(maSanPham);
            if (!success)
                return NotFound(new { message = "Không tìm thấy sản phẩm để xóa." });

            return Ok(new { message = "Xóa sản phẩm thành công." });
        }

        [HttpPost("bulk-create")]
        public async Task<IActionResult> BulkCreate([FromBody] List<SanPhamCreate> models)
        {
            if (models == null || !models.Any())
                return BadRequest("Không có data để tạo.");

            var errors = new List<string>();
            foreach (var m in models)
            {
                var ok = await _sanPhamServices.CreateAsync(m);
                if (!ok)
                    errors.Add($"Tạo thất bại cho slug={m.Slug}");
            }

            if (errors.Any())
                return BadRequest(new { message = "Có lỗi khi tạo một số sản phẩm", details = errors });

            return Ok(new { message = "Bulk create thành công." });
        }

    }
}

using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FashionApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoaiController : ControllerBase
    {
        private readonly ILoaiServices _loaiServices;

        public LoaiController(ILoaiServices loaiServices)
        {
            _loaiServices = loaiServices;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _loaiServices.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _loaiServices.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Không tìm thấy loại." });

            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            var result = await _loaiServices.SearchAsync(keyword);
            return Ok(result);
        }

        [HttpGet("{id}/sanphams")]
        public async Task<IActionResult> GetSanPhamsByLoaiId(int id)
        {
            var result = await _loaiServices.GetSanPhamsByLoaiIdAsync(id);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] LoaiCreate model, IFormFile? file)
        {
            var result = await _loaiServices.CreateAsync(model, file);
            if (!result)
                return BadRequest(new { message = "Tên loại hoặc ký hiệu đã tồn tại." });

            return Ok(new { message = "Thêm loại thành công." });
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromForm] LoaiEdit model, IFormFile? file)
        {
            var result = await _loaiServices.UpdateAsync(model, file);
            if (!result)
                return BadRequest(new { message = "Tên loại hoặc ký hiệu đã tồn tại hoặc không tìm thấy loại." });

            return Ok(new { message = "Cập nhật loại thành công." });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _loaiServices.DeleteAsync(id);
            if (!result)
                return NotFound(new { message = "Không tìm thấy loại cần xoá." });

            return Ok(new { message = "Xoá loại thành công." });
        }
    }
}

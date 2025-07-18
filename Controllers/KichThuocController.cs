using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FashionApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KichThuocController : ControllerBase
    {
        private readonly IKichThuocServices _kichThuocService;

        public KichThuocController(IKichThuocServices kichThuocService)
        {
            _kichThuocService = kichThuocService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _kichThuocService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _kichThuocService.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Không tìm thấy kích thước." });

            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            var result = await _kichThuocService.SearchAsync(keyword);
            return Ok(result);
        }

        [HttpGet("{id}/sanphams")]
        public async Task<IActionResult> GetSanPhamsByKichThuocId(int id)
        {
            var result = await _kichThuocService.GetSanPhamsByKichThuocIdAsync(id);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] KichThuocCreate model, IFormFile? file)
        {
            var result = await _kichThuocService.CreateAsync(model, file);
            if (!result)
                return BadRequest(new { message = "Kích thước đã tồn tại. Vui lòng chọn tên khác." });

            return Ok(new { message = "Thêm kích thước thành công." });
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromForm] KichThuocEdit model, IFormFile? file)
        {
            var result = await _kichThuocService.UpdateAsync(model, file);
            if (!result)
                return BadRequest(new { message = "Không tìm thấy hoặc tên kích thước đã tồn tại." });

            return Ok(new { message = "Cập nhật kích thước thành công." });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _kichThuocService.DeleteAsync(id);
            if (!result)
                return NotFound(new { message = "Không tìm thấy kích thước cần xoá." });

            return Ok(new { message = "Xoá kích thước thành công." });
        }
    }
}

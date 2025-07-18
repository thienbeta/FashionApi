using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FashionApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MauController : ControllerBase
    {
        private readonly IMauServices _mauService;

        public MauController(IMauServices mauService)
        {
            _mauService = mauService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _mauService.GetAllAsync();
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _mauService.GetByIdAsync(id);
            if (item == null)
                return NotFound(new { message = "Không tìm thấy màu." });

            return Ok(item);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string keyword)
        {
            var result = await _mauService.SearchAsync(keyword);
            return Ok(result);
        }

        [HttpGet("getproducts/{id}")]
        public async Task<IActionResult> GetSanPhamsByMauId(int id)
        {
            var result = await _mauService.GetSanPhamsByMauIdAsync(id);
            return Ok(result);
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] MauCreate model, IFormFile? file)
        {
            var success = await _mauService.CreateAsync(model, file);
            if (!success)
            {
                return BadRequest(new { message = "Code màu hoặc tên màu đã tồn tại." });
            }

            return Ok(new { message = "Thêm màu thành công." });
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromForm] MauEdit model, IFormFile? file)
        {
            var success = await _mauService.UpdateAsync(model, file);
            if (!success)
            {
                return BadRequest(new { message = "Không thể cập nhật. Code màu hoặc tên màu đã tồn tại, hoặc màu không tồn tại." });
            }

            return Ok(new { message = "Cập nhật màu thành công." });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _mauService.DeleteAsync(id);
            if (!success)
                return NotFound(new { message = "Không tìm thấy màu để xóa." });

            return Ok(new { message = "Xóa màu thành công." });
        }
    }
}

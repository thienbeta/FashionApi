using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FashionApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThuongHieuController : ControllerBase
    {
        private readonly IThuongHieuServices _thuongHieuServices;

        public ThuongHieuController(IThuongHieuServices thuongHieuServices)
        {
            _thuongHieuServices = thuongHieuServices;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _thuongHieuServices.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _thuongHieuServices.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Không tìm thấy thương hiệu." });

            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            var result = await _thuongHieuServices.SearchAsync(keyword);
            return Ok(result);
        }

        [HttpGet("{id}/sanphams")]
        public async Task<IActionResult> GetSanPhamsByThuongHieuId(int id)
        {
            var result = await _thuongHieuServices.GetSanPhamsByThuongHieuIdAsync(id);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] ThuongHieuCreate model, IFormFile? file)
        {
            var result = await _thuongHieuServices.CreateAsync(model, file);
            if (!result)
                return BadRequest(new { message = "Thương hiệu đã tồn tại. Vui lòng chọn tên khác." });

            return Ok(new { message = "Thêm thương hiệu thành công." });
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromForm] ThuongHieuEdit model, IFormFile? file)
        {
            var result = await _thuongHieuServices.UpdateAsync(model, file);
            if (!result)
                return BadRequest(new { message = "Tên thương hiệu đã tồn tại hoặc không tìm thấy thương hiệu." });

            return Ok(new { message = "Cập nhật thương hiệu thành công." });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _thuongHieuServices.DeleteAsync(id);
            if (!result)
                return NotFound(new { message = "Không tìm thấy thương hiệu cần xoá." });

            return Ok(new { message = "Xoá thương hiệu thành công." });
        }
    }
}

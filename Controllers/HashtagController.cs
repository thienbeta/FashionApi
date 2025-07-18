using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FashionApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HashtagController : ControllerBase
    {
        private readonly IHashtagServices _hashtagServices;

        public HashtagController(IHashtagServices hashtagServices)
        {
            _hashtagServices = hashtagServices;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _hashtagServices.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _hashtagServices.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { message = "Không tìm thấy hashtag." });

            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            var result = await _hashtagServices.SearchAsync(keyword);
            return Ok(result);
        }

        [HttpGet("{id}/sanphams")]
        public async Task<IActionResult> GetSanPhamsByHashtagId(int id)
        {
            var result = await _hashtagServices.GetSanPhamsByHashtagIdAsync(id);
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] HashtagCreate model, IFormFile? file)
        {
            var result = await _hashtagServices.CreateAsync(model, file);
            if (!result)
                return BadRequest(new { message = "Hashtag đã tồn tại. Vui lòng chọn tên khác." });

            return Ok(new { message = "Thêm hashtag thành công." });
        }


        [HttpPut("update")]
        public async Task<IActionResult> Update([FromForm] HashtagEdit model, IFormFile? file)
        {
            var result = await _hashtagServices.UpdateAsync(model, file);
            if (!result)
                return BadRequest(new { message = "Tên hashtag đã tồn tại hoặc không tìm thấy hashtag." });

            return Ok(new { message = "Cập nhật hashtag thành công." });
        }


        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _hashtagServices.DeleteAsync(id);
            if (!result)
                return NotFound(new { message = "Không tìm thấy hashtag cần xoá." });

            return Ok(new { message = "Xoá hashtag thành công." });
        }
    }
}

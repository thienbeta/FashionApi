using System.ComponentModel.DataAnnotations;
using FashionApi.DTO;
using FashionApi.Models.View;
using FashionApi.Repository;
using Microsoft.AspNetCore.Mvc;

namespace FashionApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GiaoDienController : ControllerBase
    {
        private readonly IGiaoDienServices _giaoDienServices;
        private readonly ILogger<GiaoDienController> _logger;

        public GiaoDienController(IGiaoDienServices giaoDienServices, ILogger<GiaoDienController> logger)
        {
            _giaoDienServices = giaoDienServices;
            _logger = logger;
        }

        // GET: api/GiaoDien
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GiaoDienView>>> GetAll()
        {
            try
            {
                var result = await _giaoDienServices.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all GiaoDien");
                return StatusCode(500, new { message = "Lỗi hệ thống khi lấy danh sách giao diện" });
            }
        }

        // GET: api/GiaoDien/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<GiaoDienView>> GetById(int id)
        {
            try
            {
                var result = await _giaoDienServices.GetByIdAsync(id);
                if (result == null)
                {
                    return NotFound(new { message = $"Không tìm thấy giao diện với ID {id}" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting GiaoDien with ID {Id}", id);
                return StatusCode(500, new { message = "Lỗi hệ thống khi lấy thông tin giao diện" });
            }
        }

        // GET: api/GiaoDien/type/{loaiGiaoDien}
        [HttpGet("type/{loaiGiaoDien}")]
        public async Task<ActionResult<IEnumerable<GiaoDienView>>> GetByType(int loaiGiaoDien)
        {
            try
            {
                var result = await _giaoDienServices.GetByTypeAsync(loaiGiaoDien);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting GiaoDien by type {Type}", loaiGiaoDien);
                return StatusCode(500, new { message = "Lỗi hệ thống khi lấy giao diện theo loại" });
            }
        }

        // GET: api/GiaoDien/active
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<GiaoDienView>>> GetActive()
        {
            try
            {
                var result = await _giaoDienServices.GetActiveAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active GiaoDien");
                return StatusCode(500, new { message = "Lỗi hệ thống khi lấy giao diện đang hoạt động" });
            }
        }

        // GET: api/GiaoDien/search/{keyword}
        [HttpGet("search/{keyword}")]
        public async Task<ActionResult<IEnumerable<GiaoDienView>>> Search([FromRoute] string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return BadRequest(new { message = "Từ khóa tìm kiếm không được để trống" });
                }

                var result = await _giaoDienServices.SearchAsync(keyword);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching GiaoDien with keyword {Keyword}", keyword);
                return StatusCode(500, new { message = "Lỗi hệ thống khi tìm kiếm giao diện" });
            }
        }

        // GET: api/GiaoDien/status/{trangThai}
        [HttpGet("status/{trangThai}")]
        public async Task<ActionResult<IEnumerable<GiaoDienView>>> FilterByStatus([FromRoute] int trangThai)
        {
            try
            {
                var result = await _giaoDienServices.FilterByStatusAsync(trangThai);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering GiaoDien by status {Status}", trangThai);
                return StatusCode(500, new { message = "Lỗi hệ thống khi lọc giao diện theo trạng thái" });
            }
        }

        // POST: api/GiaoDien
        [HttpPost]
        public async Task<ActionResult<GiaoDienView>> Create([FromBody] Models.Create.GiaoDienCreate giaoDienCreate)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _giaoDienServices.CreateAsync(giaoDienCreate);
                if (result == null)
                {
                    return BadRequest(new { message = "Không thể tạo giao diện mới" });
                }

                return CreatedAtAction(nameof(GetById), new { id = result.MaGiaoDien }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new GiaoDien");
                return StatusCode(500, new { message = "Lỗi hệ thống khi tạo giao diện mới" });
            }
        }

        // PUT: api/GiaoDien/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<GiaoDienView>> Update(int id, [FromBody] Models.Edit.GiaoDienEdit giaoDienEdit)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _giaoDienServices.UpdateAsync(id, giaoDienEdit);
                if (result == null)
                {
                    return NotFound(new { message = $"Không tìm thấy giao diện với ID {id}" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating GiaoDien with ID {Id}", id);
                return StatusCode(500, new { message = "Lỗi hệ thống khi cập nhật giao diện" });
            }
        }

        // DELETE: api/GiaoDien/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _giaoDienServices.DeleteAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"Không tìm thấy giao diện với ID {id}" });
                }

                return Ok(new { message = $"Đã xóa thành công giao diện với ID {id}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting GiaoDien with ID {Id}", id);
                return StatusCode(500, new { message = "Lỗi hệ thống khi xóa giao diện" });
            }
        }

        // POST: api/GiaoDien/{id}/media
        [HttpPost("{id}/media")]
        public async Task<IActionResult> AddMedia(int id, [FromBody] Models.Create.MediaCreate mediaCreate)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _giaoDienServices.AddMediaAsync(id, mediaCreate);
                if (!result)
                {
                    return NotFound(new { message = $"Không tìm thấy giao diện với ID {id}" });
                }

                return Ok(new { message = $"Đã thêm media thành công vào giao diện với ID {id}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding media to GiaoDien with ID {Id}", id);
                return StatusCode(500, new { message = "Lỗi hệ thống khi thêm media" });
            }
        }

        // DELETE: api/GiaoDien/{id}/media/{mediaId}
        [HttpDelete("{id}/media/{mediaId}")]
        public async Task<IActionResult> RemoveMedia(int id, int mediaId)
        {
            try
            {
                var result = await _giaoDienServices.RemoveMediaAsync(id, mediaId);
                if (!result)
                {
                    return NotFound(new { message = $"Không tìm thấy media với ID {mediaId} trong giao diện với ID {id}" });
                }

                return Ok(new { message = $"Đã xóa thành công media với ID {mediaId} khỏi giao diện với ID {id}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing media {MediaId} from GiaoDien {Id}", mediaId, id);
                return StatusCode(500, new { message = "Lỗi hệ thống khi xóa media" });
            }
        }

        // GET: api/GiaoDien/{id}/media
        [HttpGet("{id}/media")]
        public async Task<ActionResult<IEnumerable<MediaView>>> GetMedia(int id)
        {
            try
            {
                var result = await _giaoDienServices.GetMediaAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting media for GiaoDien with ID {Id}", id);
                return StatusCode(500, new { message = "Lỗi hệ thống khi lấy danh sách media" });
            }
        }
    }
}
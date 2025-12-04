using System.ComponentModel.DataAnnotations;
using FashionApi.DTO;
using FashionApi.Models.View;
using FashionApi.Repository;
using Microsoft.AspNetCore.Mvc;

namespace FashionApi.Controllers
{
    /// <summary>
    /// Controller quản lý giao diện website (Logo, Banner, Slider)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GiaoDienController : ControllerBase
    {
        private readonly IGiaoDienServices _giaoDienServices;
        private readonly ILogger<GiaoDienController> _logger;

        /// <summary>
        /// Khởi tạo GiaoDienController với dependency injection
        /// </summary>
        /// <param name="giaoDienServices">Service xử lý logic nghiệp vụ giao diện</param>
        /// <param name="logger">Logger để ghi nhật ký hoạt động</param>
        public GiaoDienController(IGiaoDienServices giaoDienServices, ILogger<GiaoDienController> logger)
        {
            _giaoDienServices = giaoDienServices;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả giao diện
        /// </summary>
        /// <returns>Danh sách tất cả giao diện bao gồm logo, banner, slider</returns>
        /// <response code="200">Lấy danh sách giao diện thành công</response>
        /// <response code="500">Lỗi hệ thống khi lấy danh sách giao diện</response>
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

        /// <summary>
        /// Lấy thông tin giao diện theo ID
        /// </summary>
        /// <param name="id">ID giao diện cần lấy</param>
        /// <returns>Thông tin chi tiết giao diện</returns>
        /// <response code="200">Lấy thông tin giao diện thành công</response>
        /// <response code="404">Không tìm thấy giao diện với ID tương ứng</response>
        /// <response code="500">Lỗi hệ thống khi lấy thông tin giao diện</response>
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

        /// <summary>
        /// Lấy giao diện theo loại
        /// </summary>
        /// <param name="loaiGiaoDien">Loại giao diện (1: Logo, 2: Banner, 3: Slider)</param>
        /// <returns>Danh sách giao diện theo loại cụ thể</returns>
        /// <response code="200">Lấy giao diện theo loại thành công</response>
        /// <response code="500">Lỗi hệ thống khi lấy giao diện theo loại</response>
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

        /// <summary>
        /// Lấy danh sách giao diện đang hoạt động
        /// </summary>
        /// <returns>Danh sách giao diện có trạng thái active (1)</returns>
        /// <response code="200">Lấy giao diện đang hoạt động thành công</response>
        /// <response code="500">Lỗi hệ thống khi lấy giao diện đang hoạt động</response>
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

        /// <summary>
        /// Tìm kiếm giao diện theo từ khóa
        /// </summary>
        /// <param name="keyword">Từ khóa tìm kiếm trong tên, mô tả hoặc meta title</param>
        /// <returns>Danh sách giao diện phù hợp với từ khóa tìm kiếm</returns>
        /// <response code="200">Tìm kiếm giao diện thành công</response>
        /// <response code="400">Từ khóa tìm kiếm không được để trống</response>
        /// <response code="500">Lỗi hệ thống khi tìm kiếm giao diện</response>
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

        /// <summary>
        /// Lọc giao diện theo trạng thái
        /// </summary>
        /// <param name="trangThai">Trạng thái giao diện (0: Không hoạt động, 1: Hoạt động)</param>
        /// <returns>Danh sách giao diện theo trạng thái</returns>
        /// <response code="200">Lọc giao diện theo trạng thái thành công</response>
        /// <response code="500">Lỗi hệ thống khi lọc giao diện theo trạng thái</response>
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

        /// <summary>
        /// Tạo giao diện mới
        /// </summary>
        /// <param name="giaoDienCreate">Thông tin tạo giao diện bao gồm tên, loại, mô tả và SEO</param>
        /// <returns>Thông tin giao diện vừa tạo</returns>
        /// <response code="201">Tạo giao diện thành công</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="500">Lỗi hệ thống khi tạo giao diện mới</response>
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

        /// <summary>
        /// Cập nhật thông tin giao diện
        /// </summary>
        /// <param name="id">ID giao diện cần cập nhật</param>
        /// <param name="giaoDienEdit">Thông tin cập nhật giao diện</param>
        /// <returns>Thông tin giao diện sau khi cập nhật</returns>
        /// <response code="200">Cập nhật giao diện thành công</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="404">Không tìm thấy giao diện với ID tương ứng</response>
        /// <response code="500">Lỗi hệ thống khi cập nhật giao diện</response>
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

        /// <summary>
        /// Xóa giao diện
        /// </summary>
        /// <param name="id">ID giao diện cần xóa</param>
        /// <returns>Kết quả xóa giao diện</returns>
        /// <response code="200">Xóa giao diện thành công</response>
        /// <response code="404">Không tìm thấy giao diện với ID tương ứng</response>
        /// <response code="500">Lỗi hệ thống khi xóa giao diện</response>
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

        /// <summary>
        /// Thêm media vào giao diện
        /// </summary>
        /// <param name="id">ID giao diện cần thêm media</param>
        /// <param name="mediaCreate">Thông tin media cần thêm</param>
        /// <returns>Kết quả thêm media</returns>
        /// <response code="200">Thêm media thành công</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="404">Không tìm thấy giao diện với ID tương ứng</response>
        /// <response code="500">Lỗi hệ thống khi thêm media</response>
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

        /// <summary>
        /// Xóa media khỏi giao diện
        /// </summary>
        /// <param name="id">ID giao diện</param>
        /// <param name="mediaId">ID media cần xóa</param>
        /// <returns>Kết quả xóa media</returns>
        /// <response code="200">Xóa media thành công</response>
        /// <response code="404">Không tìm thấy media trong giao diện tương ứng</response>
        /// <response code="500">Lỗi hệ thống khi xóa media</response>
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

        /// <summary>
        /// Lấy danh sách media của giao diện
        /// </summary>
        /// <param name="id">ID giao diện</param>
        /// <returns>Danh sách media của giao diện</returns>
        /// <response code="200">Lấy danh sách media thành công</response>
        /// <response code="500">Lỗi hệ thống khi lấy danh sách media</response>
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

        /// <summary>
        /// Upload file và tạo media cho giao diện
        /// </summary>
        /// <param name="id">ID giao diện</param>
        /// <param name="file">File ảnh cần upload</param>
        /// <param name="altText">Alt text cho hình ảnh (tùy chọn)</param>
        /// <param name="link">Link liên kết cho media (tùy chọn)</param>
        /// <returns>Thông tin media vừa tạo</returns>
        /// <response code="201">Upload và tạo media thành công</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="404">Không tìm thấy giao diện với ID tương ứng</response>
        /// <response code="500">Lỗi hệ thống khi upload media</response>
        [HttpPost("{id}/upload-media")]
        public async Task<IActionResult> UploadMedia(int id, IFormFile file, string? altText = null, string? link = null)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "File không được để trống" });
                }

                // Validate file type (only images)
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp", "image/jpg" };
                if (!allowedTypes.Contains(file.ContentType.ToLower()))
                {
                    return BadRequest(new { message = "Chỉ chấp nhận file hình ảnh (JPEG, PNG, GIF, WebP)" });
                }

                // Validate file size (max 10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new { message = "Kích thước file không được vượt quá 10MB" });
                }

                // Check if GiaoDien exists
                var giaoDien = await _giaoDienServices.GetByIdAsync(id);
                if (giaoDien == null)
                {
                    return NotFound(new { message = $"Không tìm thấy giao diện với ID {id}" });
                }

                // Determine subfolder based on GiaoDien type
                string subFolder;
                switch (giaoDien.LoaiGiaoDien)
                {
                    case 1: subFolder = "logo"; break;
                    case 2: subFolder = "banner"; break;
                    case 3: subFolder = "slider"; break;
                    default: subFolder = "giaodien"; break;
                }

                // Import MediaServices to save the file
                var mediaServices = HttpContext.RequestServices.GetRequiredService<IMediaServices>();
                var imagePath = await mediaServices.SaveOptimizedImageAsync(file, subFolder);

                // Create media record
                var mediaCreate = new Models.Create.MediaCreate
                {
                    LoaiMedia = "image",
                    DuongDan = imagePath,
                    AltMedia = altText,
                    LinkMedia = link,
                    MaGiaoDien = id,
                    TrangThai = 1
                };

                var success = await _giaoDienServices.AddMediaAsync(id, mediaCreate);
                if (!success)
                {
                    return StatusCode(500, new { message = "Không thể tạo bản ghi media" });
                }

                // Get the newly created media
                var medias = await _giaoDienServices.GetMediaAsync(id);
                var newMedia = medias.OrderByDescending(m => m.NgayTao).FirstOrDefault();

                return CreatedAtAction(nameof(GetMedia), new { id = id }, newMedia);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading media for GiaoDien {Id}", id);
                return StatusCode(500, new { message = "Lỗi hệ thống khi upload media" });
            }
        }
    }
}

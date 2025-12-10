using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Models.View;
using FashionApi.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FashionApi.Controllers
{
    /// <summary>
    /// Controller quản lý bình luận và đánh giá sản phẩm
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BinhLuanController : ControllerBase
    {
        private readonly IBinhLuanServices _binhLuanServices;
        private readonly ILogger<BinhLuanController> _logger;

        /// <summary>
        /// Khởi tạo BinhLuanController với dependency injection
        /// </summary>
        /// <param name="binhLuanServices">Service xử lý logic nghiệp vụ bình luận</param>
        /// <param name="logger">Logger để ghi nhật ký hoạt động</param>
        public BinhLuanController(IBinhLuanServices binhLuanServices, ILogger<BinhLuanController> logger)
        {
            _binhLuanServices = binhLuanServices ?? throw new ArgumentNullException(nameof(binhLuanServices));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Tạo mới bình luận cho sản phẩm
        /// </summary>
        /// <param name="model">Thông tin tạo bình luận bao gồm tiêu đề, nội dung, đánh giá và hình ảnh</param>
        /// <returns>Thông tin bình luận vừa tạo</returns>
        /// <response code="201">Tạo bình luận thành công</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="401">Không có quyền truy cập</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromForm] BinhLuanCreate model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState không hợp lệ khi tạo bình luận: {@ModelState}", ModelState);
                return BadRequest(new { Message = "Dữ liệu đầu vào không hợp lệ.", Errors = ModelState });
            }

            try
            {
                // Lấy MaNguoiDung từ JWT token
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    _logger.LogWarning("Không thể xác định người dùng từ JWT token");
                    return Unauthorized(new { Message = "Token không hợp lệ hoặc đã hết hạn." });
                }

                // Sử dụng MaNguoiDung từ token thay vì từ model
                model.MaNguoiDung = currentUserId;
                _logger.LogInformation("Tạo bình luận cho người dùng: MaNguoiDung={MaNguoiDung}", currentUserId);

                var binhLuan = await _binhLuanServices.CreateAsync(model);
                _logger.LogInformation("Tạo bình luận thành công: MaBinhLuan={MaBinhLuan}", binhLuan.MaBinhLuan);
                return CreatedAtAction(nameof(GetById), new { id = binhLuan.MaBinhLuan }, binhLuan);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Lỗi xác thực khi tạo bình luận: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo bình luận: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật thông tin bình luận
        /// </summary>
        /// <param name="id">ID bình luận cần cập nhật</param>
        /// <param name="model">Thông tin cập nhật bình luận</param>
        /// <param name="newImageFiles">Danh sách hình ảnh mới (tùy chọn)</param>
        /// <returns>Thông tin bình luận sau khi cập nhật</returns>
        /// <response code="200">Cập nhật bình luận thành công</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="401">Không có quyền truy cập</response>
        /// <response code="403">Không có quyền cập nhật bình luận của người khác</response>
        /// <response code="404">Không tìm thấy bình luận</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [Authorize]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromForm] BinhLuanEdit model, List<IFormFile>? newImageFiles = null)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState không hợp lệ khi cập nhật bình luận: Id={Id}, {@ModelState}", id, ModelState);
                return BadRequest(new { Message = "Dữ liệu đầu vào không hợp lệ.", Errors = ModelState });
            }

            if (id != model.MaBinhLuan)
            {
                _logger.LogWarning("ID không khớp với dữ liệu chỉnh sửa: Id={Id}, MaBinhLuan={MaBinhLuan}", id, model.MaBinhLuan);
                return BadRequest(new { Message = "ID không khớp với dữ liệu chỉnh sửa." });
            }

            try
            {
                // Lấy MaNguoiDung từ JWT token
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    _logger.LogWarning("Không thể xác định người dùng từ JWT token");
                    return Unauthorized(new { Message = "Token không hợp lệ hoặc đã hết hạn." });
                }

                // Kiểm tra quyền: người dùng chỉ có thể cập nhật bình luận của chính mình
                // Admin có thể cập nhật tất cả
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                if (userRole != "Admin")
                {
                    // Lấy thông tin bình luận để kiểm tra chủ sở hữu
                    var existingBinhLuan = await _binhLuanServices.GetByIdAsync(id);
                    if (existingBinhLuan == null)
                    {
                        return NotFound(new { Message = "Bình luận không tồn tại." });
                    }

                    if (existingBinhLuan.MaNguoiDung != currentUserId)
                    {
                        _logger.LogWarning("Người dùng {CurrentUserId} cố gắng cập nhật bình luận của người dùng khác {CommentOwnerId}",
                            currentUserId, existingBinhLuan.MaNguoiDung);
                        return Forbid("Bạn chỉ có thể cập nhật bình luận của chính mình.");
                    }
                }

                _logger.LogInformation("Cập nhật bình luận: MaBinhLuan={Id}, MaNguoiDung={MaNguoiDung}", id, currentUserId);

                var binhLuan = await _binhLuanServices.UpdateAsync(id, model, newImageFiles);
                _logger.LogInformation("Cập nhật bình luận thành công: MaBinhLuan={MaBinhLuan}", binhLuan.MaBinhLuan);
                return Ok(binhLuan);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Bình luận không tồn tại: Id={Id}, Message={Message}", id, ex.Message);
                return NotFound(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Lỗi xác thực khi cập nhật bình luận: Id={Id}, Message={Message}", id, ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật bình luận: Id={Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Xóa bình luận - Xóa mềm
        /// </summary>
        /// <param name="id">ID bình luận cần xóa</param>
        /// <returns>Kết quả xóa bình luận</returns>
        /// <response code="200">Xóa bình luận thành công</response>
        /// <response code="401">Không có quyền truy cập</response>
        /// <response code="403">Không có quyền xóa bình luận của người khác</response>
        /// <response code="404">Không tìm thấy bình luận</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [Authorize]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Lấy MaNguoiDung từ JWT token
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId))
                {
                    _logger.LogWarning("Không thể xác định người dùng từ JWT token");
                    return Unauthorized(new { Message = "Token không hợp lệ hoặc đã hết hạn." });
                }

                // Kiểm tra quyền: người dùng chỉ có thể xóa bình luận của chính mình
                // Admin có thể xóa tất cả
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                if (userRole != "Admin")
                {
                    // Lấy thông tin bình luận để kiểm tra chủ sở hữu
                    var existingBinhLuan = await _binhLuanServices.GetByIdAsync(id);
                    if (existingBinhLuan == null)
                    {
                        return NotFound(new { Message = "Bình luận không tồn tại." });
                    }

                    if (existingBinhLuan.MaNguoiDung != currentUserId)
                    {
                        _logger.LogWarning("Người dùng {CurrentUserId} cố gắng xóa bình luận của người dùng khác {CommentOwnerId}",
                            currentUserId, existingBinhLuan.MaNguoiDung);
                        return Forbid("Bạn chỉ có thể xóa bình luận của chính mình.");
                    }
                }

                var result = await _binhLuanServices.DeleteAsync(id);
                if (!result)
                {
                    _logger.LogWarning("Bình luận không tồn tại: Id={Id}", id);
                    return NotFound(new { Message = "Bình luận không tồn tại." });
                }

                _logger.LogInformation("Xóa bình luận thành công: Id={Id}, MaNguoiDung={MaNguoiDung}", id, currentUserId);
                return Ok(new { Message = "Xóa bình luận thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa bình luận: Id={Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thông tin bình luận theo ID
        /// </summary>
        /// <param name="id">ID bình luận</param>
        /// <returns>Thông tin chi tiết bình luận</returns>
        /// <response code="200">Lấy bình luận thành công</response>
        /// <response code="404">Không tìm thấy bình luận</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var binhLuan = await _binhLuanServices.GetByIdAsync(id);
                if (binhLuan == null)
                {
                    _logger.LogWarning("Không tìm thấy bình luận: Id={Id}", id);
                    return NotFound(new { Message = "Bình luận không tồn tại." });
                }

                _logger.LogInformation("Lấy bình luận thành công: Id={Id}", id);
                return Ok(binhLuan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy bình luận: Id={Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả bình luận
        /// </summary>
        /// <returns>Danh sách tất cả bình luận</returns>
        /// <response code="200">Lấy danh sách bình luận thành công</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var binhLuans = await _binhLuanServices.GetAllAsync();
                _logger.LogInformation("Lấy tất cả bình luận thành công, Số lượng: {Count}", binhLuans.Count);
                return Ok(binhLuans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tất cả bình luận");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Tìm kiếm bình luận theo các tiêu chí
        /// </summary>
        /// <param name="danhGia">Đánh giá (1-5 sao)</param>
        /// <param name="trangThai">Trạng thái bình luận (0: Không hoạt động, 1: Hoạt động)</param>
        /// <param name="maSanPham">Mã sản phẩm</param>
        /// <param name="maNguoiDung">Mã người dùng</param>
        /// <returns>Danh sách bình luận phù hợp với tiêu chí tìm kiếm</returns>
        /// <response code="200">Tìm kiếm bình luận thành công</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] int? danhGia, [FromQuery] int? trangThai, [FromQuery] int? maSanPham, [FromQuery] int? maNguoiDung)
        {
            try
            {
                var binhLuans = await _binhLuanServices.SearchAsync(danhGia, trangThai, maSanPham, maNguoiDung);
                _logger.LogInformation("Tìm kiếm bình luận thành công, Số lượng: {Count}, DanhGia={DanhGia}, TrangThai={TrangThai}, MaSanPham={MaSanPham}, MaNguoiDung={MaNguoiDung}",
                    binhLuans.Count, danhGia, trangThai, maSanPham, maNguoiDung);
                return Ok(binhLuans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm bình luận");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Lọc bình luận theo trạng thái
        /// </summary>
        /// <param name="trangThai">Trạng thái bình luận (0: Không hoạt động, 1: Hoạt động)</param>
        /// <returns>Danh sách bình luận theo trạng thái</returns>
        /// <response code="200">Lọc bình luận theo trạng thái thành công</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [HttpGet("filter/trang-thai/{trangThai}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FilterByTrangThai(int trangThai)
        {
            try
            {
                var binhLuans = await _binhLuanServices.FilterByTrangThaiAsync(trangThai);
                _logger.LogInformation("Lọc bình luận theo trạng thái thành công, Số lượng: {Count}, TrangThai={TrangThai}",
                    binhLuans.Count, trangThai);
                return Ok(binhLuans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lọc bình luận theo trạng thái: TrangThai={TrangThai}", trangThai);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Lọc bình luận theo đánh giá
        /// </summary>
        /// <param name="danhGia">Đánh giá (1-5 sao)</param>
        /// <returns>Danh sách bình luận theo đánh giá</returns>
        /// <response code="200">Lọc bình luận theo đánh giá thành công</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [HttpGet("filter/danh-gia/{danhGia}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FilterByDanhGia(int danhGia)
        {
            try
            {
                var binhLuans = await _binhLuanServices.FilterByDanhGiaAsync(danhGia);
                _logger.LogInformation("Lọc bình luận theo đánh giá thành công, Số lượng: {Count}, DanhGia={DanhGia}",
                    binhLuans.Count, danhGia);
                return Ok(binhLuans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lọc bình luận theo đánh giá: DanhGia={DanhGia}", danhGia);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }
    }
}
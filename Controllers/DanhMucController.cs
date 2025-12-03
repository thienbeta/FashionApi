using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Models.View;
using FashionApi.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FashionApi.Controllers
{
    /// <summary>
    /// Controller quản lý danh mục sản phẩm (Loại, Thương hiệu, Hashtag, Kích thước, Màu sắc)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DanhMucController : ControllerBase
    {
        private readonly IDanhMucServices _danhMucServices;
        private readonly ILogger<DanhMucController> _logger;

        /// <summary>
        /// Khởi tạo DanhMucController với dependency injection
        /// </summary>
        /// <param name="danhMucServices">Service xử lý logic nghiệp vụ danh mục</param>
        /// <param name="logger">Logger để ghi nhật ký hoạt động</param>
        public DanhMucController(IDanhMucServices danhMucServices, ILogger<DanhMucController> logger)
        {
            _danhMucServices = danhMucServices ?? throw new ArgumentNullException(nameof(danhMucServices));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Tạo mới danh mục (Loại, Thương hiệu, Hashtag, Kích thước, Màu sắc)
        /// </summary>
        /// <param name="model">Thông tin tạo danh mục bao gồm tên, loại và hình ảnh</param>
        /// <param name="imageFile">File hình ảnh đại diện cho danh mục</param>
        /// <returns>Thông tin danh mục vừa tạo</returns>
        /// <response code="200">Tạo danh mục thành công</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] DanhMucCreate model, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState không hợp lệ khi tạo danh mục: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            try
            {
                var danhMuc = await _danhMucServices.CreateAsync(model, imageFile);
                _logger.LogInformation("Tạo danh mục thành công: {@DanhMuc}", danhMuc);
                return Ok(danhMuc);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo danh mục: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                return StatusCode(500, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật thông tin danh mục
        /// </summary>
        /// <param name="id">ID danh mục cần cập nhật</param>
        /// <param name="model">Thông tin cập nhật danh mục</param>
        /// <param name="imageFile">File hình ảnh mới (tùy chọn)</param>
        /// <returns>Thông tin danh mục sau khi cập nhật</returns>
        /// <response code="200">Cập nhật danh mục thành công</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="404">Không tìm thấy danh mục</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] DanhMucEdit model, IFormFile imageFile = null)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState không hợp lệ khi cập nhật danh mục: Id={Id}, {@ModelState}", id, ModelState);
                return BadRequest(ModelState);
            }

            if (id != model.MaDanhMuc)
            {
                _logger.LogWarning("ID không khớp với dữ liệu chỉnh sửa: Id={Id}, MaDanhMuc={MaDanhMuc}", id, model.MaDanhMuc);
                return BadRequest(new { Message = "ID không khớp với dữ liệu chỉnh sửa." });
            }

            try
            {
                var danhMuc = await _danhMucServices.UpdateAsync(id, model, imageFile);
                _logger.LogInformation("Cập nhật danh mục thành công: {@DanhMuc}", danhMuc);
                return Ok(danhMuc);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Danh mục không tồn tại: Id={Id}, Message={Message}", id, ex.Message);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật danh mục: Id={Id}, StackTrace: {StackTrace}", id, ex.StackTrace);
                return StatusCode(500, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Xóa danh mục - Xóa mềm
        /// </summary>
        /// <param name="id">ID danh mục cần xóa</param>
        /// <returns>Kết quả xóa danh mục</returns>
        /// <response code="200">Xóa danh mục thành công</response>
        /// <response code="404">Không tìm thấy danh mục</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _danhMucServices.DeleteAsync(id);
                if (!result)
                {
                    _logger.LogWarning("Danh mục không tồn tại: Id={Id}", id);
                    return NotFound(new { Message = "Danh mục không tồn tại." });
                }

                _logger.LogInformation("Xóa danh mục thành công: Id={Id}", id);
                return Ok(new { Message = "Xóa danh mục thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa danh mục: Id={Id}, StackTrace: {StackTrace}", id, ex.StackTrace);
                return StatusCode(500, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thông tin danh mục theo ID
        /// </summary>
        /// <param name="id">ID danh mục</param>
        /// <returns>Thông tin chi tiết danh mục</returns>
        /// <response code="200">Lấy danh mục thành công</response>
        /// <response code="404">Không tìm thấy danh mục</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var danhMuc = await _danhMucServices.GetByIdAsync(id);
                if (danhMuc == null)
                {
                    _logger.LogWarning("Không tìm thấy danh mục: Id={Id}", id);
                    return NotFound(new { Message = "Danh mục không tồn tại." });
                }

                _logger.LogInformation("Lấy danh mục thành công: Id={Id}", id);
                return Ok(danhMuc);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh mục: Id={Id}, StackTrace: {StackTrace}", id, ex.StackTrace);
                return StatusCode(500, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả danh mục
        /// </summary>
        /// <returns>Danh sách tất cả danh mục</returns>
        /// <response code="200">Lấy danh sách danh mục thành công</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var danhMucs = await _danhMucServices.GetAllAsync();
                _logger.LogInformation("Lấy tất cả danh mục thành công, Số lượng: {Count}", danhMucs.Count());
                return Ok(danhMucs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tất cả danh mục, StackTrace: {StackTrace}", ex.StackTrace);
                return StatusCode(500, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Tìm kiếm danh mục theo từ khóa
        /// </summary>
        /// <param name="keyword">Từ khóa tìm kiếm trong tên danh mục</param>
        /// <returns>Danh sách danh mục phù hợp với từ khóa</returns>
        /// <response code="200">Tìm kiếm danh mục thành công</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            try
            {
                var danhMucs = await _danhMucServices.SearchAsync(keyword);
                _logger.LogInformation("Tìm kiếm danh mục thành công với từ khóa: {Keyword}, Số lượng: {Count}", keyword, danhMucs.Count());
                return Ok(danhMucs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm danh mục với từ khóa: {Keyword}, StackTrace: {StackTrace}", keyword, ex.StackTrace);
                return StatusCode(500, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Lọc danh mục theo trạng thái
        /// </summary>
        /// <param name="status">Trạng thái danh mục (0: Không hoạt động, 1: Hoạt động)</param>
        /// <returns>Danh sách danh mục theo trạng thái</returns>
        /// <response code="200">Lọc danh mục theo trạng thái thành công</response>
        /// <response code="400">Trạng thái không hợp lệ (phải là 0 hoặc 1)</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [HttpGet("filter/status/{status}")]
        public async Task<IActionResult> FilterByStatus(int status)
        {
            if (status != 0 && status != 1)
            {
                _logger.LogWarning("Trạng thái không hợp lệ: {Status}", status);
                return BadRequest(new { Message = "Trạng thái phải là 0 hoặc 1." });
            }

            try
            {
                var danhMucs = await _danhMucServices.FilterByStatusAsync(status);
                _logger.LogInformation("Lọc danh mục theo trạng thái thành công: {Status}, Số lượng: {Count}", status, danhMucs.Count());
                return Ok(danhMucs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lọc danh mục theo trạng thái: {Status}, StackTrace: {StackTrace}", status, ex.StackTrace);
                return StatusCode(500, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Lọc danh mục theo loại
        /// </summary>
        /// <param name="loaiDanhMuc">Loại danh mục (1: Loại sản phẩm, 2: Thương hiệu, 3: Hashtag, 4: Kích thước, 5: Màu sắc)</param>
        /// <returns>Danh sách danh mục theo loại</returns>
        /// <response code="200">Lọc danh mục theo loại thành công</response>
        /// <response code="400">Loại danh mục không hợp lệ (phải từ 1 đến 5)</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [HttpGet("filter/type/{loaiDanhMuc}")]
        public async Task<IActionResult> FilterByCategoryType(int loaiDanhMuc)
        {
            if (loaiDanhMuc < 1 || loaiDanhMuc > 5)
            {
                _logger.LogWarning("Loại danh mục không hợp lệ: {LoaiDanhMuc}", loaiDanhMuc);
                return BadRequest(new { Message = "Loại danh mục phải từ 1 đến 5." });
            }

            try
            {
                var danhMucs = await _danhMucServices.FilterByCategoryTypeAsync(loaiDanhMuc);
                _logger.LogInformation("Lọc danh mục theo loại thành công: {LoaiDanhMuc}, Số lượng: {Count}", loaiDanhMuc, danhMucs.Count());
                return Ok(danhMucs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lọc danh mục theo loại: {LoaiDanhMuc}, StackTrace: {StackTrace}", loaiDanhMuc, ex.StackTrace);
                return StatusCode(500, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }
    }
}
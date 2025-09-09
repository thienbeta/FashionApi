using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Models.View;
using FashionApi.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FashionApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DanhMucController : ControllerBase
    {
        private readonly IDanhMucServices _danhMucServices;
        private readonly ILogger<DanhMucController> _logger;

        public DanhMucController(IDanhMucServices danhMucServices, ILogger<DanhMucController> logger)
        {
            _danhMucServices = danhMucServices ?? throw new ArgumentNullException(nameof(danhMucServices));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

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
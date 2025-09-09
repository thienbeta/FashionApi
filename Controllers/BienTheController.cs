using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Models.View;
using FashionApi.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FashionApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BienTheController : ControllerBase
    {
        private readonly IBienTheServices _bienTheServices;
        private readonly ILogger<BienTheController> _logger;

        public BienTheController(IBienTheServices bienTheServices, ILogger<BienTheController> logger)
        {
            _bienTheServices = bienTheServices ?? throw new ArgumentNullException(nameof(bienTheServices));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromForm] BienTheCreate model, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState không hợp lệ khi tạo biến thể: {@ModelState}", ModelState);
                return BadRequest(new { Message = "Dữ liệu đầu vào không hợp lệ.", Errors = ModelState });
            }

            try
            {
                var bienThe = await _bienTheServices.CreateAsync(model, imageFile);
                _logger.LogInformation("Tạo biến thể thành công: MaBienThe={MaBienThe}", bienThe.MaBienThe);
                return CreatedAtAction(nameof(GetById), new { id = bienThe.MaBienThe }, bienThe);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Lỗi xác thực khi tạo biến thể: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo biến thể: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromForm] BienTheEdit model, IFormFile imageFile = null)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState không hợp lệ khi cập nhật biến thể: Id={Id}, {@ModelState}", id, ModelState);
                return BadRequest(new { Message = "Dữ liệu đầu vào không hợp lệ.", Errors = ModelState });
            }

            if (id != model.MaBienThe)
            {
                _logger.LogWarning("ID không khớp với dữ liệu chỉnh sửa: Id={Id}, MaBienThe={MaBienThe}", id, model.MaBienThe);
                return BadRequest(new { Message = "ID không khớp với dữ liệu chỉnh sửa." });
            }

            try
            {
                var bienThe = await _bienTheServices.UpdateAsync(id, model, imageFile);
                _logger.LogInformation("Cập nhật biến thể thành công: MaBienThe={MaBienThe}", bienThe.MaBienThe);
                return Ok(bienThe);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Biến thể không tồn tại: Id={Id}, Message={Message}", id, ex.Message);
                return NotFound(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Lỗi xác thực khi cập nhật biến thể: Id={Id}, Message={Message}", id, ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật biến thể: Id={Id}, StackTrace: {StackTrace}", id, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _bienTheServices.DeleteAsync(id);
                if (!result)
                {
                    _logger.LogWarning("Biến thể không tồn tại: Id={Id}", id);
                    return NotFound(new { Message = "Biến thể không tồn tại." });
                }

                _logger.LogInformation("Xóa biến thể thành công: Id={Id}", id);
                return Ok(new { Message = "Xóa biến thể thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa biến thể: Id={Id}, StackTrace: {StackTrace}", id, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var bienThe = await _bienTheServices.GetByIdAsync(id);
                if (bienThe == null)
                {
                    _logger.LogWarning("Không tìm thấy biến thể: Id={Id}", id);
                    return NotFound(new { Message = "Biến thể không tồn tại." });
                }

                _logger.LogInformation("Lấy biến thể thành công: Id={Id}", id);
                return Ok(bienThe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy biến thể: Id={Id}, StackTrace: {StackTrace}", id, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var bienThes = await _bienTheServices.GetAllAsync();
                _logger.LogInformation("Lấy tất cả biến thể thành công, Số lượng: {Count}", bienThes.Count);
                return Ok(bienThes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tất cả biến thể, StackTrace: {StackTrace}", ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] decimal? giaBan, [FromQuery] int? soLuongNhap, [FromQuery] int? trangThai)
        {
            try
            {
                var bienThes = await _bienTheServices.SearchAsync(giaBan, soLuongNhap, trangThai);
                _logger.LogInformation("Tìm kiếm biến thể thành công, Số lượng: {Count}, GiaBan={GiaBan}, SoLuongNhap={SoLuongNhap}, TrangThai={TrangThai}",
                    bienThes.Count, giaBan, soLuongNhap, trangThai);
                return Ok(bienThes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm biến thể, StackTrace: {StackTrace}", ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        [HttpGet("filter/danh-muc/{maDanhMuc}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FilterByDanhMuc(int maDanhMuc)
        {
            try
            {
                var bienThes = await _bienTheServices.FilterByDanhMucAsync(maDanhMuc);
                _logger.LogInformation("Lọc biến thể theo danh mục thành công, Số lượng: {Count}, MaDanhMuc={MaDanhMuc}",
                    bienThes.Count, maDanhMuc);
                return Ok(bienThes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lọc biến thể theo danh mục: MaDanhMuc={MaDanhMuc}, StackTrace: {StackTrace}",
                    maDanhMuc, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        [HttpGet("by-san-pham/{maSanPham}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBySanPham(int maSanPham)
        {
            try
            {
                var bienThes = await _bienTheServices.GetBySanPhamAsync(maSanPham);
                _logger.LogInformation("Lấy biến thể theo sản phẩm thành công, Số lượng: {Count}, MaSanPham={MaSanPham}",
                    bienThes.Count, maSanPham);
                return Ok(bienThes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy biến thể theo sản phẩm: MaSanPham={MaSanPham}, StackTrace: {StackTrace}",
                    maSanPham, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }
    }
}
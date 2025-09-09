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
    public class SanPhamController : ControllerBase
    {
        private readonly ISanPhamServices _sanPhamServices;
        private readonly ILogger<SanPhamController> _logger;

        public SanPhamController(ISanPhamServices sanPhamServices, ILogger<SanPhamController> logger)
        {
            _sanPhamServices = sanPhamServices ?? throw new ArgumentNullException(nameof(sanPhamServices));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromForm] SanPhamCreate model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState không hợp lệ khi tạo sản phẩm: {@ModelState}", ModelState);
                return BadRequest(new { Message = "Dữ liệu đầu vào không hợp lệ.", Errors = ModelState });
            }

            try
            {
                var sanPham = await _sanPhamServices.CreateAsync(model);
                _logger.LogInformation("Tạo sản phẩm thành công: MaSanPham={MaSanPham}, TenSanPham={TenSanPham}",
                    sanPham.MaSanPham, sanPham.TenSanPham);
                return CreatedAtAction(nameof(GetById), new { id = sanPham.MaSanPham }, sanPham);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Lỗi xác thực khi tạo sản phẩm: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo sản phẩm: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromForm] SanPhamEdit model, List<IFormFile>? newImageFiles = null)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState không hợp lệ khi cập nhật sản phẩm: Id={Id}, {@ModelState}", id, ModelState);
                return BadRequest(new { Message = "Dữ liệu đầu vào không hợp lệ.", Errors = ModelState });
            }

            if (id != model.MaSanPham)
            {
                _logger.LogWarning("ID không khớp với dữ liệu chỉnh sửa: Id={Id}, MaSanPham={MaSanPham}", id, model.MaSanPham);
                return BadRequest(new { Message = "ID không khớp với dữ liệu chỉnh sửa." });
            }

            try
            {
                var sanPham = await _sanPhamServices.UpdateAsync(id, model, newImageFiles);
                _logger.LogInformation("Cập nhật sản phẩm thành công: MaSanPham={MaSanPham}, TenSanPham={TenSanPham}",
                    sanPham.MaSanPham, sanPham.TenSanPham);
                return Ok(sanPham);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Sản phẩm không tồn tại: Id={Id}, Message={Message}", id, ex.Message);
                return NotFound(new { Message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Lỗi xác thực khi cập nhật sản phẩm: Id={Id}, Message={Message}", id, ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật sản phẩm: Id={Id}, StackTrace: {StackTrace}", id, ex.StackTrace);
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
                var result = await _sanPhamServices.DeleteAsync(id);
                if (!result)
                {
                    _logger.LogWarning("Sản phẩm không tồn tại: Id={Id}", id);
                    return NotFound(new { Message = "Sản phẩm không tồn tại." });
                }

                _logger.LogInformation("Xóa sản phẩm thành công: Id={Id}", id);
                return Ok(new { Message = "Xóa sản phẩm thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa sản phẩm: Id={Id}, StackTrace: {StackTrace}", id, ex.StackTrace);
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
                var sanPham = await _sanPhamServices.GetByIdAsync(id);
                if (sanPham == null)
                {
                    _logger.LogWarning("Không tìm thấy sản phẩm: Id={Id}", id);
                    return NotFound(new { Message = "Sản phẩm không tồn tại." });
                }

                _logger.LogInformation("Lấy sản phẩm thành công: Id={Id}, TenSanPham={TenSanPham}", id, sanPham.TenSanPham);
                return Ok(sanPham);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy sản phẩm: Id={Id}, StackTrace: {StackTrace}", id, ex.StackTrace);
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
                var sanPhams = await _sanPhamServices.GetAllAsync();
                _logger.LogInformation("Lấy tất cả sản phẩm thành công, Số lượng: {Count}", sanPhams.Count);
                return Ok(sanPhams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tất cả sản phẩm, StackTrace: {StackTrace}", ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search(
            [FromQuery] decimal? giaBan,
            [FromQuery] int? soLuongNhap,
            [FromQuery] int? trangThai,
            [FromQuery] string? maVach,
            [FromQuery] int? maSanPham,
            [FromQuery] string? tenSanPham)
        {
            try
            {
                var sanPhams = await _sanPhamServices.SearchAsync(giaBan, soLuongNhap, trangThai, maVach, maSanPham, tenSanPham);
                _logger.LogInformation(
                    "Tìm kiếm sản phẩm thành công, Số lượng: {Count}, GiaBan={GiaBan}, SoLuongNhap={SoLuongNhap}, TrangThai={TrangThai}, MaVach={MaVach}, MaSanPham={MaSanPham}, TenSanPham={TenSanPham}",
                    sanPhams.Count, giaBan, soLuongNhap, trangThai, maVach, maSanPham, tenSanPham);
                return Ok(sanPhams);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Lỗi xác thực khi tìm kiếm sản phẩm: {Message}", ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm sản phẩm, StackTrace: {StackTrace}", ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        [HttpGet("filter/loai-danh-muc/{maLoaiDanhMuc}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FilterByLoaiDanhMuc(int maLoaiDanhMuc)
        {
            try
            {
                var sanPhams = await _sanPhamServices.FilterByLoaiDanhMucAsync(maLoaiDanhMuc);
                _logger.LogInformation("Lọc sản phẩm theo loại danh mục thành công, Số lượng: {Count}, MaLoaiDanhMuc={MaLoaiDanhMuc}",
                    sanPhams.Count, maLoaiDanhMuc);
                return Ok(sanPhams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lọc sản phẩm theo loại danh mục: MaLoaiDanhMuc={MaLoaiDanhMuc}, StackTrace: {StackTrace}",
                    maLoaiDanhMuc, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        [HttpGet("filter/danh-muc/{maDanhMuc}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByDanhMuc(int maDanhMuc)
        {
            try
            {
                var sanPhams = await _sanPhamServices.GetByDanhMucAsync(maDanhMuc);
                _logger.LogInformation("Lọc sản phẩm theo danh mục thành công, Số lượng: {Count}, MaDanhMuc={MaDanhMuc}",
                    sanPhams.Count, maDanhMuc);
                return Ok(sanPhams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lọc sản phẩm theo danh mục: MaDanhMuc={MaDanhMuc}, StackTrace: {StackTrace}",
                    maDanhMuc, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        [HttpGet("best-selling")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBestSelling([FromQuery] int limit = 10)
        {
            if (limit <= 0)
            {
                _logger.LogWarning("Giới hạn không hợp lệ khi lấy sản phẩm bán chạy: Limit={Limit}", limit);
                return BadRequest(new { Message = "Giới hạn phải lớn hơn 0." });
            }

            try
            {
                var sanPhams = await _sanPhamServices.GetBestSellingAsync(limit);
                _logger.LogInformation("Lấy sản phẩm bán chạy thành công, Số lượng: {Count}, Limit={Limit}",
                    sanPhams.Count, limit);
                return Ok(sanPhams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy sản phẩm bán chạy: Limit={Limit}, StackTrace: {StackTrace}",
                    limit, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        [HttpGet("filter/loai-thuong-hieu")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByLoaiAndThuongHieu([FromQuery] int maLoai, [FromQuery] int maThuongHieu)
        {
            if (maLoai <= 0 || maThuongHieu <= 0)
            {
                _logger.LogWarning("Danh mục không hợp lệ: MaLoai={MaLoai}, MaThuongHieu={MaThuongHieu}", maLoai, maThuongHieu);
                return BadRequest(new { Message = "Mã loại sản phẩm và mã thương hiệu phải lớn hơn 0." });
            }

            try
            {
                var sanPhams = await _sanPhamServices.GetByLoaiAndThuongHieuAsync(maLoai, maThuongHieu);
                _logger.LogInformation("Lấy sản phẩm theo loại và thương hiệu thành công, Số lượng: {Count}, MaLoai={MaLoai}, MaThuongHieu={MaThuongHieu}",
                    sanPhams.Count, maLoai, maThuongHieu);
                return Ok(sanPhams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy sản phẩm theo loại và thương hiệu: MaLoai={MaLoai}, MaThuongHieu={MaThuongHieu}, StackTrace: {StackTrace}",
                    maLoai, maThuongHieu, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

    }
}
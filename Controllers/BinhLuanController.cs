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
    public class BinhLuanController : ControllerBase
    {
        private readonly IBinhLuanServices _binhLuanServices;
        private readonly ILogger<BinhLuanController> _logger;

        public BinhLuanController(IBinhLuanServices binhLuanServices, ILogger<BinhLuanController> logger)
        {
            _binhLuanServices = binhLuanServices ?? throw new ArgumentNullException(nameof(binhLuanServices));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _binhLuanServices.DeleteAsync(id);
                if (!result)
                {
                    _logger.LogWarning("Bình luận không tồn tại: Id={Id}", id);
                    return NotFound(new { Message = "Bình luận không tồn tại." });
                }

                _logger.LogInformation("Xóa bình luận thành công: Id={Id}", id);
                return Ok(new { Message = "Xóa bình luận thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa bình luận: Id={Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
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

        [HttpGet("filter/danh-gia/{danhGia}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FilterByDanhGia(int danhGia)
        {
            try
            {
                var binhLuans = await _binhLuanServices.FilterByDanhGiaAsync(danhGia);
                _logger.LogInformation("Lọc bình luận theo trạng thái thành công, Số lượng: {Count}, DanhGia={DanhGia}",
                    binhLuans.Count, danhGia);
                return Ok(binhLuans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lọc bình luận theo trạng thái: DanhGia={DanhGia}", danhGia);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }
    }
}
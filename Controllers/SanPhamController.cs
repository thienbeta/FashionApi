using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Models.View;
using FashionApi.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FashionApi.Controllers
{
    /// <summary>Controller quản lý sản phẩm thời trang với CRUD operations, phân trang, tìm kiếm nâng cao và upload hình ảnh</summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SanPhamController : ControllerBase
    {
        private readonly ISanPhamServices _sanPhamServices;
        private readonly ILogger<SanPhamController> _logger;

        /// <summary>Khởi tạo controller với dependency injection cho service và logger</summary>
        public SanPhamController(ISanPhamServices sanPhamServices, ILogger<SanPhamController> logger)
        {
            _sanPhamServices = sanPhamServices ?? throw new ArgumentNullException(nameof(sanPhamServices));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Tạo mới sản phẩm (Admin only)
        /// </summary>
        /// <param name="model">Thông tin tạo sản phẩm bao gồm tên, mô tả, loại, thương hiệu và hình ảnh</param>
        /// <returns>Thông tin sản phẩm vừa tạo</returns>
        /// <response code="201">Tạo sản phẩm thành công</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
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

        /// <summary>
        /// Cập nhật thông tin sản phẩm (Admin only)
        /// </summary>
        /// <param name="id">Mã sản phẩm cần cập nhật</param>
        /// <param name="model">Thông tin cập nhật sản phẩm</param>
        /// <param name="newImageFiles">Danh sách hình ảnh mới (tùy chọn)</param>
        /// <returns>Thông tin sản phẩm sau khi cập nhật</returns>
        /// <response code="200">Cập nhật sản phẩm thành công</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="404">Không tìm thấy sản phẩm</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
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

        /// <summary>
        /// Xóa sản phẩm (Admin only) - Xóa mềm
        /// </summary>
        /// <param name="id">Mã sản phẩm cần xóa</param>
        /// <param name="hardDeleteImages">True: xóa vĩnh viễn file hình ảnh, False: soft delete (mặc định)</param>
        /// <returns>Kết quả xóa sản phẩm</returns>
        /// <response code="200">Xóa sản phẩm thành công</response>
        /// <response code="404">Không tìm thấy sản phẩm</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id, [FromQuery] bool hardDeleteImages = false)
        {
            try
            {
                var result = await _sanPhamServices.DeleteAsync(id, hardDeleteImages);
                if (!result)
                {
                    _logger.LogWarning("Sản phẩm không tồn tại: Id={Id}", id);
                    return NotFound(new { Message = "Sản phẩm không tồn tại." });
                }

                _logger.LogInformation("Xóa sản phẩm thành công: Id={Id}, HardDeleteImages={HardDeleteImages}", 
                    id, hardDeleteImages);
                return Ok(new { Message = "Xóa sản phẩm thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa sản phẩm: Id={Id}, StackTrace: {StackTrace}", id, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Xóa hình ảnh của sản phẩm (Admin only)
        /// </summary>
        /// <param name="productId">Mã sản phẩm</param>
        /// <param name="mediaId">Mã hình ảnh cần xóa</param>
        /// <param name="hardDelete">True: xóa vĩnh viễn (bao gồm file vật lý), False: soft delete (mặc định)</param>
        /// <returns>Kết quả xóa hình ảnh</returns>
        /// <response code="200">Xóa hình ảnh thành công</response>
        /// <response code="404">Không tìm thấy hình ảnh</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [HttpDelete("{productId}/images/{mediaId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteProductImage(
            int productId,
            int mediaId,
            [FromQuery] bool hardDelete = false)
        {
            try
            {
                var result = await _sanPhamServices.DeleteProductImageAsync(productId, mediaId, hardDelete);

                if (!result)
                {
                    _logger.LogWarning("Không tìm thấy hình ảnh: ProductId={ProductId}, MediaId={MediaId}",
                        productId, mediaId);
                    return NotFound(new { Message = "Hình ảnh không tồn tại hoặc không thuộc về sản phẩm này." });
                }

                _logger.LogInformation("Xóa hình ảnh thành công: ProductId={ProductId}, MediaId={MediaId}, HardDelete={HardDelete}",
                    productId, mediaId, hardDelete);
                return Ok(new { Message = hardDelete ? "Xóa hình ảnh vĩnh viễn thành công." : "Xóa hình ảnh thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa hình ảnh: ProductId={ProductId}, MediaId={MediaId}",
                    productId, mediaId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Xóa nhiều hình ảnh của sản phẩm cùng lúc (Admin only)
        /// </summary>
        /// <param name="productId">Mã sản phẩm</param>
        /// <param name="request">Danh sách mã hình ảnh cần xóa</param>
        /// <returns>Kết quả xóa batch</returns>
        /// <response code="200">Xóa batch thành công</response>
        /// <response code="400">Dữ liệu không hợp lệ</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [HttpDelete("{productId}/images/batch")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteProductImagesBatch(
            int productId,
            [FromBody] BatchDeleteImageRequest request)
        {
            if (request == null || request.MediaIds == null || !request.MediaIds.Any())
            {
                return BadRequest(new { Message = "Danh sách hình ảnh không được trống." });
            }

            try
            {
                var result = await _sanPhamServices.DeleteProductImagesAsync(
                    productId, 
                    request.MediaIds, 
                    request.HardDelete);

                _logger.LogInformation(
                    "Batch delete hoàn thành: ProductId={ProductId}, Success={Success}, Failed={Failed}",
                    productId, result.SuccessCount, result.FailedCount);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa batch hình ảnh: ProductId={ProductId}", productId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thông tin sản phẩm theo mã sản phẩm
        /// </summary>
        /// <param name="id">Mã sản phẩm</param>
        /// <returns>Thông tin chi tiết sản phẩm</returns>
        /// <response code="200">Lấy sản phẩm thành công</response>
        /// <response code="404">Không tìm thấy sản phẩm</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
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

        /// <summary>
        /// Lấy danh sách tất cả sản phẩm (Admin only)
        /// </summary>
        /// <returns>Danh sách tất cả sản phẩm</returns>
        /// <response code="200">Lấy danh sách sản phẩm thành công</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
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

        /// <summary>
        /// Tìm kiếm sản phẩm theo các tiêu chí (Admin only)
        /// </summary>
        /// <param name="trangThai">Trạng thái sản phẩm (0: Không hoạt động, 1: Hoạt động)</param>
        /// <param name="maSanPham">Mã sản phẩm cụ thể</param>
        /// <param name="tenSanPham">Tên sản phẩm (tìm kiếm chứa)</param>
        /// <returns>Danh sách sản phẩm phù hợp với tiêu chí tìm kiếm</returns>
        /// <response code="200">Tìm kiếm thành công</response>
        /// <response code="400">Tham số tìm kiếm không hợp lệ</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search(
            [FromQuery] int? trangThai,
            [FromQuery] int? maSanPham,
            [FromQuery] string? tenSanPham)
        {
            try
            {
                var sanPhams = await _sanPhamServices.SearchAsync(trangThai, maSanPham, tenSanPham);
                _logger.LogInformation(
                    "Tìm kiếm sản phẩm thành công, Số lượng: {Count}, TrangThai={TrangThai}, MaSanPham={MaSanPham}, TenSanPham={TenSanPham}",
                    sanPhams.Count, trangThai, maSanPham, tenSanPham);
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

        /// <summary>
        /// Lọc sản phẩm theo loại danh mục
        /// </summary>
        /// <param name="maLoaiDanhMuc">Mã loại danh mục (1: Loại sản phẩm, 2: Thương hiệu, 3: Hashtag)</param>
        /// <returns>Danh sách sản phẩm thuộc loại danh mục</returns>
        /// <response code="200">Lọc sản phẩm thành công</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
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

        /// <summary>
        /// Lấy sản phẩm theo danh mục cụ thể
        /// </summary>
        /// <param name="maDanhMuc">Mã danh mục cụ thể</param>
        /// <returns>Danh sách sản phẩm thuộc danh mục</returns>
        /// <response code="200">Lấy sản phẩm theo danh mục thành công</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
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

        /// <summary>
        /// Lấy danh sách sản phẩm bán chạy
        /// </summary>
        /// <param name="limit">Số lượng sản phẩm tối đa cần lấy (mặc định 10)</param>
        /// <returns>Danh sách sản phẩm bán chạy nhất</returns>
        /// <response code="200">Lấy sản phẩm bán chạy thành công</response>
        /// <response code="400">Tham số limit không hợp lệ</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
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

        /// <summary>
        /// Lấy sản phẩm theo loại và thương hiệu kết hợp
        /// </summary>
        /// <param name="maLoai">Mã loại sản phẩm</param>
        /// <param name="maThuongHieu">Mã thương hiệu</param>
        /// <returns>Danh sách sản phẩm thuộc cả loại và thương hiệu</returns>
        /// <response code="200">Lấy sản phẩm thành công</response>
        /// <response code="400">Tham số không hợp lệ</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
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

        /// <summary>
        /// Lấy sản phẩm mới nhất với phân trang - cho User
        /// </summary>
        /// <param name="page">Số trang (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số sản phẩm mỗi trang (mặc định 12)</param>
        /// <returns>Danh sách sản phẩm với thông tin phân trang</returns>
        [HttpGet("newest")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetNewestProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12)
        {
            if (page <= 0 || pageSize <= 0 || pageSize > 100)
            {
                _logger.LogWarning("Tham số phân trang không hợp lệ: Page={Page}, PageSize={PageSize}", page, pageSize);
                return BadRequest(new { Message = "Số trang phải lớn hơn 0 và kích thước trang từ 1-100." });
            }

            try
            {
                // Lấy tất cả sản phẩm đang hoạt động
                var allProducts = await _sanPhamServices.GetAllAsync();
                var activeProducts = allProducts.Where(sp => sp.TrangThai == 1).ToList();

                // Sắp xếp theo ngày tạo mới nhất
                var sortedProducts = activeProducts
                    .OrderByDescending(sp => sp.NgayTao)
                    .ThenBy(sp => sp.TenSanPham)
                    .ToList();

                // Tính toán phân trang
                var totalCount = sortedProducts.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var skip = (page - 1) * pageSize;

                var pagedProducts = sortedProducts
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();

                var result = new
                {
                    Data = pagedProducts,
                    Pagination = new
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages,
                        HasNextPage = page < totalPages,
                        HasPreviousPage = page > 1
                    }
                };

                _logger.LogInformation("Lấy sản phẩm mới nhất với phân trang thành công: Page={Page}, PageSize={PageSize}, TotalCount={TotalCount}",
                    page, pageSize, totalCount);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy sản phẩm mới nhất với phân trang: Page={Page}, PageSize={PageSize}, StackTrace: {StackTrace}",
                    page, pageSize, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Lấy sản phẩm bán chạy với phân trang - cho User
        /// </summary>
        /// <param name="page">Số trang (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số sản phẩm mỗi trang (mặc định 12)</param>
        /// <returns>Danh sách sản phẩm bán chạy với thông tin phân trang</returns>
        [HttpGet("hot-sale")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetHotSaleProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12)
        {
            if (page <= 0 || pageSize <= 0 || pageSize > 100)
            {
                _logger.LogWarning("Tham số phân trang không hợp lệ: Page={Page}, PageSize={PageSize}", page, pageSize);
                return BadRequest(new { Message = "Số trang phải lớn hơn 0 và kích thước trang từ 1-100." });
            }

            try
            {
                // Lấy top 100 sản phẩm bán chạy nhất
                var hotProducts = await _sanPhamServices.GetBestSellingAsync(100);
                var activeHotProducts = hotProducts.Where(sp => sp.TrangThai == 1).ToList();

                // Tính toán phân trang
                var totalCount = activeHotProducts.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var skip = (page - 1) * pageSize;

                var pagedProducts = activeHotProducts
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();

                var result = new
                {
                    Data = pagedProducts,
                    Pagination = new
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages,
                        HasNextPage = page < totalPages,
                        HasPreviousPage = page > 1
                    }
                };

                _logger.LogInformation("Lấy sản phẩm hot sale với phân trang thành công: Page={Page}, PageSize={PageSize}, TotalCount={TotalCount}",
                    page, pageSize, totalCount);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy sản phẩm hot sale với phân trang: Page={Page}, PageSize={PageSize}, StackTrace: {StackTrace}",
                    page, pageSize, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Tìm kiếm và lọc sản phẩm với phân trang - cho User và Admin
        /// </summary>
        /// <param name="page">Số trang (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số sản phẩm mỗi trang (mặc định 12)</param>
        /// <param name="keyword">Từ khóa tìm kiếm</param>
        /// <param name="maLoai">Mã loại sản phẩm</param>
        /// <param name="maThuongHieu">Mã thương hiệu</param>
        /// <param name="minPrice">Giá tối thiểu</param>
        /// <param name="maxPrice">Giá tối đa</param>
        /// <param name="gioiTinh">Giới tính (0: Tất cả, 1: Nam, 2: Nữ, 3: Khác)</param>
        /// <param name="sortBy">Sắp xếp theo (newest, price_asc, price_desc, rating)</param>
        /// <returns>Danh sách sản phẩm đã lọc với thông tin phân trang</returns>
        [HttpGet("filter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FilterProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] string? keyword = null,
            [FromQuery] int? maLoai = null,
            [FromQuery] int? maThuongHieu = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] int? gioiTinh = null,
            [FromQuery] string? sortBy = "newest")
        {
            if (page <= 0 || pageSize <= 0 || pageSize > 100)
            {
                _logger.LogWarning("Tham số phân trang không hợp lệ: Page={Page}, PageSize={PageSize}", page, pageSize);
                return BadRequest(new { Message = "Số trang phải lớn hơn 0 và kích thước trang từ 1-100." });
            }

            try
            {
                // Lấy tất cả sản phẩm
                var allProducts = await _sanPhamServices.GetAllAsync();
                var filteredProducts = allProducts.Where(sp => sp.TrangThai == 1).ToList();

                // Lọc theo từ khóa
                if (!string.IsNullOrEmpty(keyword))
                {
                    filteredProducts = filteredProducts
                        .Where(sp => sp.TenSanPham.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                                   (sp.MoTa != null && sp.MoTa.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                        .ToList();
                }

                // Lọc theo loại
                if (maLoai.HasValue)
                {
                    filteredProducts = filteredProducts.Where(sp => sp.MaLoai == maLoai.Value).ToList();
                }

                // Lọc theo thương hiệu
                if (maThuongHieu.HasValue)
                {
                    filteredProducts = filteredProducts.Where(sp => sp.MaThuongHieu == maThuongHieu.Value).ToList();
                }

                // Lọc theo giới tính
                if (gioiTinh.HasValue)
                {
                    filteredProducts = filteredProducts.Where(sp => sp.GioiTinh == gioiTinh.Value).ToList();
                }

                // Sắp xếp
                filteredProducts = sortBy?.ToLower() switch
                {
                    "price_asc" => filteredProducts.OrderBy(sp => sp.DanhGiaTrungBinh).ToList(),
                    "price_desc" => filteredProducts.OrderByDescending(sp => sp.DanhGiaTrungBinh).ToList(),
                    "rating" => filteredProducts.OrderByDescending(sp => sp.DanhGiaTrungBinh ?? 0).ThenByDescending(sp => sp.SoLuongDanhGia ?? 0).ToList(),
                    _ => filteredProducts.OrderByDescending(sp => sp.NgayTao).ToList() // newest
                };

                // Tính toán phân trang
                var totalCount = filteredProducts.Count;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var skip = (page - 1) * pageSize;

                var pagedProducts = filteredProducts
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();

                var result = new
                {
                    Data = pagedProducts,
                    Filters = new
                    {
                        Keyword = keyword,
                        MaLoai = maLoai,
                        MaThuongHieu = maThuongHieu,
                        MinPrice = minPrice,
                        MaxPrice = maxPrice,
                        GioiTinh = gioiTinh,
                        SortBy = sortBy
                    },
                    Pagination = new
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = totalPages,
                        HasNextPage = page < totalPages,
                        HasPreviousPage = page > 1
                    }
                };

                _logger.LogInformation("Lọc sản phẩm với phân trang thành công: Page={Page}, PageSize={PageSize}, TotalCount={TotalCount}, Filters={@Filters}",
                    page, pageSize, totalCount, new { keyword, maLoai, maThuongHieu, gioiTinh, sortBy });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lọc sản phẩm với phân trang: Page={Page}, PageSize={PageSize}, StackTrace: {StackTrace}",
                    page, pageSize, ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }
    }
}

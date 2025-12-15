using System;
using System.Collections.Generic;
using System.Security.Claims;
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
    /// <summary>Controller quản lý người dùng và xác thực với đăng ký, đăng nhập, quên mật khẩu, profile và admin management</summary>
    [Route("api/[controller]")]
    [ApiController]
    public class NguoiDungController : ControllerBase
    {
        private readonly INguoiDungServices _nguoiDungServices;
        private readonly ILogger<NguoiDungController> _logger;

        /// <summary>Khởi tạo controller với dependency injection cho service và logger</summary>
        public NguoiDungController(INguoiDungServices nguoiDungServices, ILogger<NguoiDungController> logger)
        {
            _nguoiDungServices = nguoiDungServices ?? throw new ArgumentNullException(nameof(nguoiDungServices));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Tạo mới người dùng (Đăng ký tài khoản)
        /// </summary>
        /// <param name="model">Thông tin tạo người dùng bao gồm họ tên, email, tài khoản, mật khẩu và avatar</param>
        /// <returns>Thông tin người dùng vừa tạo</returns>
        /// <response code="200">Tạo người dùng thành công</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NguoiDungCreate model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState không hợp lệ khi tạo người dùng: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            try
            {
                var nguoiDung = await _nguoiDungServices.CreateAsync(model);
                _logger.LogInformation("Tạo người dùng thành công: {@NguoiDung}", nguoiDung);
                return Ok(nguoiDung);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Lỗi tạo người dùng (Trùng lặp/Logic): {@Model}, Message={Message}", model, ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo người dùng: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                return StatusCode(500, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }


        /// <summary>
        /// Cập nhật thông tin người dùng - user tự cập nhật hoặc admin cập nhật user khác
        /// </summary>
        /// <param name="id">ID người dùng cần cập nhật</param>
        /// <param name="model">Thông tin cập nhật người dùng</param>
        /// <param name="imageFile">File ảnh đại diện mới (tùy chọn)</param>
        /// <returns>Thông tin người dùng sau khi cập nhật</returns>
        /// <response code="200">Cập nhật người dùng thành công</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="401">Không có quyền truy cập</response>
        /// <response code="403">Không có quyền cập nhật thông tin người dùng khác</response>
        /// <response code="404">Không tìm thấy người dùng</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] NguoiDungEdit model, IFormFile imageFile = null)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState không hợp lệ khi cập nhật người dùng: Id={Id}, {@ModelState}", id, ModelState);
                return BadRequest(ModelState);
            }

            // Check file size (max 5MB)
            if (imageFile != null && imageFile.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { Message = "Kích thước ảnh không được vượt quá 5MB." });
            }


            if (id != model.MaNguoiDung)
            {
                _logger.LogWarning("ID không khớp với dữ liệu chỉnh sửa: Id={Id}, MaNguoiDung={MaNguoiDung}", id, model.MaNguoiDung);
                return BadRequest(new { Message = "ID không khớp với dữ liệu chỉnh sửa." });
            }

            // Kiểm tra quyền: admin hoặc chính user đó
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out var currentUserId))
            {
                return Unauthorized(new { Message = "Không thể xác định người dùng hiện tại." });
            }

            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (currentUserRole != "Admin" && currentUserId != id)
            {
                return Forbid("Bạn không có quyền cập nhật thông tin của người dùng khác.");
            }

            try
            {
                var currentUserEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;
                var nguoiDung = await _nguoiDungServices.UpdateAsync(id, model, imageFile, currentUserId, currentUserEmail);
                _logger.LogInformation("Cập nhật người dùng thành công: {@NguoiDung}", nguoiDung);
                return Ok(nguoiDung);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Người dùng không tồn tại: Id={Id}, Message={Message}", id, ex.Message);
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật người dùng: Id={Id}, StackTrace: {StackTrace}", id, ex.StackTrace);
                return StatusCode(500, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Xóa người dùng - chỉ admin
        /// </summary>
        /// <param name="id">ID người dùng cần xóa</param>
        /// <returns>Kết quả xóa người dùng</returns>
        /// <response code="200">Xóa người dùng thành công</response>
        /// <response code="401">Không có quyền truy cập</response>
        /// <response code="403">Chỉ admin mới có quyền xóa</response>
        /// <response code="404">Không tìm thấy người dùng</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var currentUserEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;
                var result = await _nguoiDungServices.DeleteAsync(id, currentUserEmail);
                if (!result)
                {
                    _logger.LogWarning("Người dùng không tồn tại: Id={Id}", id);
                    return NotFound(new { Message = "Người dùng không tồn tại." });
                }

                _logger.LogInformation("Xóa người dùng thành công: Id={Id}", id);
                return Ok(new { Message = "Xóa người dùng thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa người dùng: Id={Id}, StackTrace: {StackTrace}", id, ex.StackTrace);
                return StatusCode(500, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Lấy thông tin người dùng theo ID - yêu cầu đăng nhập
        /// </summary>
        /// <param name="id">ID người dùng cần lấy thông tin</param>
        /// <returns>Thông tin chi tiết người dùng</returns>
        /// <response code="200">Lấy thông tin người dùng thành công</response>
        /// <response code="401">Không có quyền truy cập</response>
        /// <response code="404">Không tìm thấy người dùng</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var nguoiDung = await _nguoiDungServices.GetByIdAsync(id);
                if (nguoiDung == null)
                {
                    _logger.LogWarning("Không tìm thấy người dùng: Id={Id}", id);
                    return NotFound(new { Message = "Người dùng không tồn tại." });
                }

                _logger.LogInformation("Lấy người dùng thành công: Id={Id}", id);
                return Ok(nguoiDung);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy người dùng: Id={Id}, StackTrace: {StackTrace}", id, ex.StackTrace);
                return StatusCode(500, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả người dùng - chỉ admin
        /// </summary>
        /// <returns>Danh sách tất cả người dùng trong hệ thống</returns>
        /// <response code="200">Lấy danh sách người dùng thành công</response>
        /// <response code="401">Không có quyền truy cập</response>
        /// <response code="403">Chỉ admin mới có quyền xem danh sách</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()

        {
            try
            {
                var nguoiDungs = await _nguoiDungServices.GetAllAsync();
                _logger.LogInformation("Lấy tất cả người dùng thành công, Số lượng: {Count}", nguoiDungs.Count);
                return Ok(nguoiDungs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tất cả người dùng, StackTrace: {StackTrace}", ex.StackTrace);
                return StatusCode(500, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Tìm kiếm người dùng theo từ khóa - chỉ admin
        /// </summary>
        /// <param name="keyword">Từ khóa tìm kiếm trong họ tên, email, tài khoản</param>
        /// <returns>Danh sách người dùng phù hợp với từ khóa tìm kiếm</returns>
        /// <response code="200">Tìm kiếm người dùng thành công</response>
        /// <response code="401">Không có quyền truy cập</response>
        /// <response code="403">Chỉ admin mới có quyền tìm kiếm</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [Authorize(Roles = "Admin")]
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            try
            {
                var nguoiDungs = await _nguoiDungServices.SearchAsync(keyword);
                _logger.LogInformation("Tìm kiếm người dùng thành công, Số lượng: {Count}", nguoiDungs.Count);
                return Ok(nguoiDungs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm người dùng, StackTrace: {StackTrace}", ex.StackTrace);
                return StatusCode(500, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Lọc người dùng theo vai trò - chỉ admin
        /// </summary>
        /// <param name="role">Vai trò cần lọc (1=Admin, 2=User)</param>
        /// <returns>Danh sách người dùng theo vai trò cụ thể</returns>
        /// <response code="200">Lọc người dùng theo vai trò thành công</response>
        /// <response code="401">Không có quyền truy cập</response>
        /// <response code="403">Chỉ admin mới có quyền lọc</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [Authorize(Roles = "Admin")]
        [HttpGet("filter/role/{role}")]
        public async Task<IActionResult> FilterByRole(int role)
        {
            try
            {
                var nguoiDungs = await _nguoiDungServices.FilterByRoleAsync(role);
                _logger.LogInformation("Lọc người dùng theo vai trò thành công, Số lượng: {Count}", nguoiDungs.Count);
                return Ok(nguoiDungs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lọc người dùng theo vai trò: {Role}, StackTrace: {StackTrace}", role, ex.StackTrace);
                return StatusCode(500, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Đăng nhập người dùng
        /// </summary>
        /// <param name="model">Thông tin đăng nhập bao gồm tài khoản và mật khẩu</param>
        /// <returns>Token JWT và thông tin người dùng đã đăng nhập</returns>
        /// <response code="200">Đăng nhập thành công</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="401">Thông tin đăng nhập không hợp lệ</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] DangNhap model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState không hợp lệ khi đăng nhập: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            try
            {
                var nguoiDung = await _nguoiDungServices.LoginAsync(model.TaiKhoan, model.MatKhau);
                if (nguoiDung == null)
                {
                    _logger.LogWarning("Thông tin đăng nhập không hợp lệ: {TaiKhoan}", model.TaiKhoan);
                    return Unauthorized(new { Message = "Thông tin đăng nhập không hợp lệ." });
                }

                _logger.LogInformation("Đăng nhập thành công: {TaiKhoan}", model.TaiKhoan);
                return Ok(nguoiDung);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Lỗi đăng nhập (Tài khoản bị khóa/Lỗi logic): {TaiKhoan}, Message={Message}", model.TaiKhoan, ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng nhập: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                return StatusCode(500, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Gửi OTP để đặt lại mật khẩu
        /// </summary>
        /// <param name="email">Email của người dùng cần reset mật khẩu</param>
        /// <returns>Kết quả gửi OTP qua email</returns>
        /// <response code="200">Gửi OTP thành công</response>
        /// <response code="404">Email không tồn tại trong hệ thống</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            try
            {
                await _nguoiDungServices.SendForgotPasswordOtpAsync(email);
                _logger.LogInformation("Gửi OTP quên mật khẩu thành công cho: {Email}", email);
                return Ok(new { Message = "OTP đã được gửi đến email của bạn." });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Email không tồn tại: {Email}, Message={Message}", email, ex.Message);
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Tài khoản bị khóa khi yêu cầu quên mật khẩu: {Email}, Message={Message}", email, ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi OTP quên mật khẩu: {Email}, StackTrace: {StackTrace}", email, ex.StackTrace);
                return StatusCode(500, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Đặt lại mật khẩu với OTP
        /// </summary>
        /// <param name="model">Thông tin đặt lại mật khẩu bao gồm email, OTP và mật khẩu mới</param>
        /// <returns>Kết quả đặt lại mật khẩu</returns>
        /// <response code="200">Đặt lại mật khẩu thành công</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ</response>
        /// <response code="404">Email không tồn tại</response>
        /// <response code="500">Lỗi máy chủ nội bộ</response>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] QuenMatKhau model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState không hợp lệ khi đặt lại mật khẩu: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            try
            {
                await _nguoiDungServices.ResetPasswordAsync(model);
                _logger.LogInformation("Đặt lại mật khẩu thành công cho: {Email}", model.Email);
                return Ok(new { Message = "Mật khẩu đã được đặt lại thành công." });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Email không tồn tại: {Email}, Message={Message}", model.Email, ex.Message);
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Lỗi xác thực khi đặt lại mật khẩu: {Email}, Message={Message}", model.Email, ex.Message);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đặt lại mật khẩu: {Email}, StackTrace: {StackTrace}", model.Email, ex.StackTrace);
                return StatusCode(500, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }
    }
}

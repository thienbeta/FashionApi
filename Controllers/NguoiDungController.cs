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
    public class NguoiDungController : ControllerBase
    {
        private readonly INguoiDungServices _nguoiDungServices;
        private readonly ILogger<NguoiDungController> _logger;

        public NguoiDungController(INguoiDungServices nguoiDungServices, ILogger<NguoiDungController> logger)
        {
            _nguoiDungServices = nguoiDungServices ?? throw new ArgumentNullException(nameof(nguoiDungServices));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] NguoiDungCreate model)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo người dùng: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                return StatusCode(500, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }


        // Cập nhật người dùng
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] NguoiDungEdit model, IFormFile imageFile = null)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState không hợp lệ khi cập nhật người dùng: Id={Id}, {@ModelState}", id, ModelState);
                return BadRequest(ModelState);
            }

            if (id != model.MaNguoiDung)
            {
                _logger.LogWarning("ID không khớp với dữ liệu chỉnh sửa: Id={Id}, MaNguoiDung={MaNguoiDung}", id, model.MaNguoiDung);
                return BadRequest(new { Message = "ID không khớp với dữ liệu chỉnh sửa." });
            }

            try
            {
                var nguoiDung = await _nguoiDungServices.UpdateAsync(id, model, imageFile);
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

        // Xóa người dùng
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _nguoiDungServices.DeleteAsync(id);
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

        // Lấy người dùng theo ID
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

        // Lấy tất cả người dùng
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

        // Tìm kiếm người dùng
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

        // Lọc người dùng theo vai trò
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

        // Đăng nhập người dùng
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng nhập: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                return StatusCode(500, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        // Gửi OTP quên mật khẩu
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi OTP quên mật khẩu: {Email}, StackTrace: {StackTrace}", email, ex.StackTrace);
                return StatusCode(500, new { Message = "Lỗi máy chủ nội bộ", Detail = ex.Message });
            }
        }

        // Đặt lại mật khẩu với OTP
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
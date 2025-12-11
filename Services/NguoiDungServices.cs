using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FashionApi.Data;
using FashionApi.DTO;
using FashionApi.Models.Create;
using FashionApi.Models.Edit;
using FashionApi.Models.View;
using FashionApi.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace FashionApi.Services
{
    public class NguoiDungServices : INguoiDungServices
    {
        private readonly ApplicationDbContext _context;
        private readonly IMediaServices _mediaServices;
        private readonly IMemoryCacheServices _cacheServices;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;
        private readonly ILogger<NguoiDungServices> _logger;

        public NguoiDungServices(
            ApplicationDbContext context,
            IMediaServices mediaServices,
            IMemoryCacheServices cacheServices,
            EmailService emailService,
            IConfiguration config,
            ILogger<NguoiDungServices> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mediaServices = mediaServices ?? throw new ArgumentNullException(nameof(mediaServices));
            _cacheServices = cacheServices ?? throw new ArgumentNullException(nameof(cacheServices));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Thêm người dùng mới với mã hóa mật khẩu
        public async Task<NguoiDungView> CreateAsync(NguoiDungCreate model)
        {
            _logger.LogInformation("Bắt đầu tạo người dùng mới: {TaiKhoan}", model.TaiKhoan);

            try
            {
                // Kiểm tra tài khoản và email đã tồn tại chưa
                if (await CheckAccountExistsAsync(model.TaiKhoan))
                    throw new InvalidOperationException("Tài khoản đã tồn tại.");

                if (await CheckEmailExistsAsync(model.Email))
                    throw new InvalidOperationException("Email đã tồn tại.");

                // Tạo đối tượng người dùng mới
                var nguoiDung = new NguoiDung
                {
                    HoTen = model.HoTen,
                    Email = model.Email,
                    TaiKhoan = model.TaiKhoan,
                    MatKhau = PasswordHasher.HashPassword(model.MatKhau), // Mã hóa mật khẩu
                    VaiTro = model.VaiTro,
                    NgayTao = DateTime.UtcNow,
                    TrangThai = 1, // Mặc định kích hoạt
                    GioiTinh = 0   // Mặc định
                };

                _context.NguoiDungs.Add(nguoiDung);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Người dùng đã được tạo thành công: MaNguoiDung={MaNguoiDung}", nguoiDung.MaNguoiDung);

                return MapToView(nguoiDung);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo người dùng: {Message}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }
        }

        // Cập nhật người dùng, hỗ trợ thay đổi mật khẩu
        public async Task<NguoiDungView> UpdateAsync(int id, NguoiDungEdit model, IFormFile imageFile = null)
        {
            _logger.LogInformation("Bắt đầu cập nhật người dùng: MaNguoiDung={Id}", id);

            try
            {
                var nguoiDung = await _context.NguoiDungs.FindAsync(id);
                if (nguoiDung == null)
                    throw new KeyNotFoundException("Người dùng không tồn tại.");

                // Kiểm tra tài khoản, email và số điện thoại có bị trùng không
                if (!string.IsNullOrEmpty(model.TaiKhoan) && model.TaiKhoan != nguoiDung.TaiKhoan && await CheckAccountExistsAsync(model.TaiKhoan))
                    throw new InvalidOperationException("Tài khoản đã tồn tại.");

                if (!string.IsNullOrEmpty(model.Email) && model.Email != nguoiDung.Email && await CheckEmailExistsAsync(model.Email))
                    throw new InvalidOperationException("Email đã tồn tại.");

                if (!string.IsNullOrEmpty(model.Sdt) && model.Sdt != nguoiDung.Sdt && await CheckPhoneExistsAsync(model.Sdt))
                    throw new InvalidOperationException("Số điện thoại đã tồn tại.");

                // Cập nhật thông tin cơ bản
                nguoiDung.HoTen = model.HoTen ?? nguoiDung.HoTen;
                nguoiDung.NgaySinh = model.NgaySinh ?? nguoiDung.NgaySinh;
                nguoiDung.Sdt = model.Sdt ?? nguoiDung.Sdt;
                nguoiDung.Email = model.Email ?? nguoiDung.Email;
                nguoiDung.TaiKhoan = model.TaiKhoan ?? nguoiDung.TaiKhoan;
                nguoiDung.VaiTro = model.VaiTro;
                nguoiDung.TrangThai = model.TrangThai;
                nguoiDung.TieuSu = model.TieuSu ?? nguoiDung.TieuSu;
                nguoiDung.TimeKhoa = model.TimeKhoa ?? nguoiDung.TimeKhoa;
                nguoiDung.GioiTinh = model.GioiTinh ?? nguoiDung.GioiTinh;

                // Xử lý thay đổi mật khẩu nếu có
                if (!string.IsNullOrEmpty(model.MatKhauMoi))
                {
                    if (!PasswordHasher.VerifyPassword(model.MatKhauCu, nguoiDung.MatKhau))
                        throw new InvalidOperationException("Mật khẩu cũ không đúng.");

                    if (model.MatKhauMoi != model.XacNhanMatKhau)
                        throw new InvalidOperationException("Mật khẩu mới và xác nhận không khớp.");

                    nguoiDung.MatKhau = PasswordHasher.HashPassword(model.MatKhauMoi);
                    _logger.LogInformation("Mật khẩu đã được cập nhật cho người dùng: MaNguoiDung={Id}", id);
                }

                // Xử lý ảnh đại diện nếu có
                if (imageFile != null)
                {
                    // Xóa ảnh đại diện cũ nếu tồn tại
                    if (!string.IsNullOrEmpty(nguoiDung.Avt))
                    {
                        var oldAvatarDeleted = await _mediaServices.DeleteImageAsync(nguoiDung.Avt);
                        if (oldAvatarDeleted)
                        {
                            _logger.LogInformation("Đã xóa ảnh đại diện cũ: {OldAvatarUrl}", nguoiDung.Avt);
                        }
                        else
                        {
                            _logger.LogWarning("Không thể xóa ảnh đại diện cũ: {OldAvatarUrl}", nguoiDung.Avt);
                        }
                    }

                    var avatarUrl = await _mediaServices.SaveOptimizedImageAsync(imageFile, "nguoidung");
                    nguoiDung.Avt = avatarUrl;
                    _logger.LogInformation("Hình ảnh đại diện đã được cập nhật: {AvatarUrl}", avatarUrl);
                }

                await _context.SaveChangesAsync();

                return MapToView(nguoiDung);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật người dùng: MaNguoiDung={Id}, StackTrace: {StackTrace}", id, ex.StackTrace);
                throw;
            }
        }

        // Xóa người dùng
        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Bắt đầu xóa người dùng: MaNguoiDung={Id}", id);

            try
            {
                var nguoiDung = await _context.NguoiDungs
                    .Include(nd => nd.BinhLuans)
                    .ThenInclude(bl => bl.Medias)
                    .FirstOrDefaultAsync(nd => nd.MaNguoiDung == id);

                if (nguoiDung == null)
                {
                    _logger.LogWarning("Người dùng không tồn tại: MaNguoiDung={Id}", id);
                    return false;
                }

                // Xóa avatar của người dùng nếu có
                if (!string.IsNullOrEmpty(nguoiDung.Avt))
                {
                    var avatarDeleted = await _mediaServices.DeleteImageAsync(nguoiDung.Avt);
                    if (avatarDeleted)
                    {
                        _logger.LogInformation("Đã xóa avatar của người dùng: {AvatarUrl}", nguoiDung.Avt);
                    }
                    else
                    {
                        _logger.LogWarning("Không thể xóa avatar của người dùng: {AvatarUrl}", nguoiDung.Avt);
                    }
                }

                // Xóa tất cả các bình luận của người dùng và hình ảnh liên kết
                if (nguoiDung.BinhLuans != null && nguoiDung.BinhLuans.Any())
                {
                    foreach (var binhLuan in nguoiDung.BinhLuans)
                    {
                        if (binhLuan.Medias != null && binhLuan.Medias.Any())
                        {
                            foreach (var media in binhLuan.Medias)
                            {
                                if (!string.IsNullOrEmpty(media.DuongDan))
                                {
                                    var mediaDeleted = await _mediaServices.DeleteImageAsync(media.DuongDan);
                                    if (mediaDeleted)
                                    {
                                        _logger.LogInformation("Đã xóa file hình ảnh của bình luận: {ImagePath}", media.DuongDan);
                                    }
                                    else
                                    {
                                        _logger.LogWarning("Không thể xóa file hình ảnh của bình luận: {ImagePath}", media.DuongDan);
                                    }
                                }
                            }
                        }
                    }
                }

                _context.NguoiDungs.Remove(nguoiDung);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Xóa người dùng thành công: MaNguoiDung={Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa người dùng: MaNguoiDung={Id}, StackTrace: {StackTrace}", id, ex.StackTrace);
                throw;
            }
        }

        // Lấy người dùng theo ID
        public async Task<NguoiDungView> GetByIdAsync(int id)
        {
            _logger.LogInformation("Truy vấn người dùng: MaNguoiDung={Id}", id);

            try
            {
                var nguoiDung = await _context.NguoiDungs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(nd => nd.MaNguoiDung == id);

                return nguoiDung == null ? null : MapToView(nguoiDung);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi truy vấn người dùng: MaNguoiDung={Id}, StackTrace: {StackTrace}", id, ex.StackTrace);
                throw;
            }
        }

        // Lấy tất cả người dùng
        public async Task<List<NguoiDungView>> GetAllAsync()
        {
            _logger.LogInformation("Truy vấn tất cả người dùng");

            try
            {
                var nguoiDungs = await _context.NguoiDungs
                    .AsNoTracking()
                    .OrderByDescending(nd => nd.NgayTao) // sắp xếp mới nhất lên đầu
                    .ToListAsync();

                return nguoiDungs.Select(MapToView).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi truy vấn tất cả người dùng, StackTrace: {StackTrace}", ex.StackTrace);
                throw;
            }
        }


        // Tìm kiếm người dùng theo từ khóa
        public async Task<List<NguoiDungView>> SearchAsync(string keyword)
        {
            _logger.LogInformation("Tìm kiếm người dùng với từ khóa: {Keyword}", keyword);

            try
            {
                var query = _context.NguoiDungs.AsQueryable();

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(nd => EF.Functions.Like(nd.TaiKhoan, $"%{keyword}%") ||
                                              EF.Functions.Like(nd.Email, $"%{keyword}%") ||
                                              EF.Functions.Like(nd.HoTen, $"%{keyword}%"));
                }

                var nguoiDungs = await query.AsNoTracking().ToListAsync();
                return nguoiDungs.Select(MapToView).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm người dùng: {Keyword}, StackTrace: {StackTrace}", keyword, ex.StackTrace);
                throw;
            }
        }

        // Lọc người dùng theo vai trò
        public async Task<List<NguoiDungView>> FilterByRoleAsync(int role)
        {
            _logger.LogInformation("Lọc người dùng theo vai trò: {Role}", role);

            try
            {
                var nguoiDungs = await _context.NguoiDungs
                    .AsNoTracking()
                    .Where(nd => nd.VaiTro == role)
                    .ToListAsync();

                return nguoiDungs.Select(MapToView).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lọc người dùng theo vai trò: {Role}, StackTrace: {StackTrace}", role, ex.StackTrace);
                throw;
            }
        }

        // Đăng nhập người dùng và tạo JWT token
        public async Task<LoginResponse> LoginAsync(string taiKhoan, string matKhau)
        {
            _logger.LogInformation("Đăng nhập người dùng: {TaiKhoan}", taiKhoan);

            try
            {
                var nguoiDung = await _context.NguoiDungs
                    .FirstOrDefaultAsync(nd => nd.TaiKhoan == taiKhoan || nd.Email == taiKhoan);

                if (nguoiDung == null || !PasswordHasher.VerifyPassword(matKhau, nguoiDung.MatKhau))
                {
                    _logger.LogWarning("Thông tin đăng nhập không hợp lệ: {TaiKhoan}", taiKhoan);
                    return null;
                }

                var userView = MapToView(nguoiDung);
                var token = GenerateJwtToken(userView);

                // Lưu user view vào cache tạm thời sau đăng nhập thành công
                await _cacheServices.GetOrCreateAsync($"User_{taiKhoan}", async () => userView, TimeSpan.FromMinutes(30));
                _logger.LogInformation("Đăng nhập thành công và tạo JWT token: {TaiKhoan}", taiKhoan);

                return new LoginResponse
                {
                    Token = token,
                    User = userView
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đăng nhập người dùng: {TaiKhoan}, StackTrace: {StackTrace}", taiKhoan, ex.StackTrace);
                throw;
            }
        }

        // Tạo JWT token
        private string GenerateJwtToken(NguoiDungView user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.MaNguoiDung.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.TaiKhoan),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.TaiKhoan),
                new Claim(ClaimTypes.Role, GetRoleName(user.VaiTro))
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiryMinutes = int.Parse(_config["Jwt:ExpiryInMinutes"] ?? "1440"); // 24 hours default

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Chuyển đổi VaiTro từ int thành string
        private string GetRoleName(int vaiTro)
        {
            return vaiTro switch
            {
                1 => "Admin",
                2 => "User",
                _ => "User" // Mặc định là User
            };
        }

        // Gửi OTP cho quên mật khẩu
        public async Task SendForgotPasswordOtpAsync(string email)
        {
            _logger.LogInformation("Gửi OTP quên mật khẩu cho email: {Email}", email);

            try
            {
                var nguoiDung = await _context.NguoiDungs.FirstOrDefaultAsync(nd => nd.Email == email);
                if (nguoiDung == null)
                    throw new KeyNotFoundException("Email không tồn tại.");

                // Tạo OTP ngẫu nhiên 6 chữ số
                var otp = new Random().Next(100000, 999999).ToString();

                // Lưu OTP vào cache với thời hạn 5 phút
                await _cacheServices.GetOrCreateAsync($"OTP_{email}", async () => otp, TimeSpan.FromMinutes(5));

                // Xây dựng nội dung HTML cho email
                string htmlBody = $@"
                <!DOCTYPE html>
                <html lang='vi'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            background-color: #f4f4f4;
                            margin: 0;
                            padding: 0;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 0 auto;
                            background-color: #ffffff;
                            padding: 20px;
                            border-radius: 8px;
                            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
                        }}
                        .header {{
                            text-align: center;
                            padding: 20px 0;
                        }}
                        .header img {{
                            max-width: 150px;
                            height: auto;
                        }}
                        .content {{
                            text-align: center;
                            padding: 20px;
                        }}
                        .otp {{
                            font-size: 36px;
                            font-weight: bold;
                            color: #e91e63;
                            margin: 20px 0;
                            letter-spacing: 5px;
                            background-color: #f9f9f9;
                            padding: 10px;
                            border-radius: 4px;
                        }}
                        .button {{
                            display: inline-block;
                            margin: 20px 0;
                            padding: 10px 20px;
                            background-color: #e91e63;
                            color: #ffffff;
                            text-decoration: none;
                            border-radius: 4px;
                            font-weight: bold;
                        }}
                        .footer {{
                            text-align: center;
                            padding: 20px;
                            font-size: 14px;
                            color: #777;
                            border-top: 1px solid #ddd;
                            margin-top: 20px;
                        }}
                        .footer a {{
                            color: #e91e63;
                            text-decoration: none;
                        }}
                        @media screen and (max-width: 600px) {{
                            .container {{
                                padding: 10px;
                            }}
                            .otp {{
                                font-size: 28px;
                            }}
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Xin chào từ HoaiThu.Vn!</h2>
                        </div>
                        <div class='content'>
                            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
                            <p>Mã OTP của bạn là:</p>
                            <div class='otp'>{otp}</div>
                            <p>Mã này sẽ hết hạn sau <strong>5 phút</strong>.</p>
                            <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này và liên hệ hỗ trợ nếu cần.</p>
                            <a href='https://hoaithu.vn/reset-password' class='button'>Đặt lại mật khẩu ngay</a>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2025 HoaThu. Tất cả quyền được bảo lưu.</p>
                            <p>Liên hệ với chúng tôi: <a href='mailto:contact.hoaithu.vn@gmail.com'>contact.hoaithu.vn@gmail.com</a></p>
                            <p>Theo dõi chúng tôi: 
                                <a href='https://facebook.com/hoaithu.vn'>Facebook</a> | 
                                <a href='https://instagram.com/hoaithu.vn'>Instagram</a>
                            </p>
                        </div>
                    </div>
                </body>
                </html>";

                // Nội dung plain text fallback
                string plainBody = $"Mã OTP của bạn là: {otp}. Mã này có hiệu lực trong 5 phút.";

                // Gửi email với nội dung HTML
                var subject = "Yêu cầu đặt lại mật khẩu";
                await _emailService.SendEmailAsync(email, subject, htmlBody);


                _logger.LogInformation("OTP đã được gửi thành công cho email: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi OTP quên mật khẩu: {Email}, StackTrace: {StackTrace}", email, ex.StackTrace);
                throw;
            }
        }
        // Đặt lại mật khẩu với OTP
        public async Task ResetPasswordAsync(QuenMatKhau model)
        {
            _logger.LogInformation("Đặt lại mật khẩu cho email: {Email}", model.Email);

            try
            {
                var nguoiDung = await _context.NguoiDungs.FirstOrDefaultAsync(nd => nd.Email == model.Email);
                if (nguoiDung == null)
                    throw new KeyNotFoundException("Email không tồn tại.");

                // Lấy OTP từ cache
                var cachedOtp = await _cacheServices.GetOrCreateAsync<string>($"OTP_{model.Email}", async () => null);
                if (cachedOtp == null || cachedOtp != model.Otp)
                    throw new InvalidOperationException("OTP không hợp lệ hoặc đã hết hạn.");

                if (model.MatKhauMoi != model.XacNhanMatKhau)
                    throw new InvalidOperationException("Mật khẩu mới và xác nhận không khớp.");

                // Cập nhật mật khẩu mới đã mã hóa
                nguoiDung.MatKhau = PasswordHasher.HashPassword(model.MatKhauMoi);
                await _context.SaveChangesAsync();

                // Xóa OTP khỏi cache
                _cacheServices.Remove($"OTP_{model.Email}");

                _logger.LogInformation("Đặt lại mật khẩu thành công cho email: {Email}", model.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đặt lại mật khẩu: {Email}, StackTrace: {StackTrace}", model.Email, ex.StackTrace);
                throw;
            }
        }

        // Kiểm tra tài khoản có tồn tại
        public async Task<bool> CheckAccountExistsAsync(string taiKhoan)
        {
            return await _context.NguoiDungs.AnyAsync(nd => nd.TaiKhoan == taiKhoan);
        }

        // Kiểm tra email có tồn tại
        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await _context.NguoiDungs.AnyAsync(nd => nd.Email == email);
        }

        // Kiểm tra số điện thoại có tồn tại
        public async Task<bool> CheckPhoneExistsAsync(string phone)
        {
            return await _context.NguoiDungs.AnyAsync(nd => nd.Sdt == phone);
        }

        // Chuyển đổi từ NguoiDung thành NguoiDungView
        private NguoiDungView MapToView(NguoiDung nguoiDung)
        {
            return new NguoiDungView
            {
                MaNguoiDung = nguoiDung.MaNguoiDung,
                HoTen = nguoiDung.HoTen,
                NgaySinh = nguoiDung.NgaySinh.HasValue ? DateOnly.FromDateTime(nguoiDung.NgaySinh.Value) : null,
                Sdt = nguoiDung.Sdt,
                Email = nguoiDung.Email,
                TaiKhoan = nguoiDung.TaiKhoan,
                VaiTro = nguoiDung.VaiTro,
                TrangThai = nguoiDung.TrangThai,
                Avt = nguoiDung.Avt,
                TieuSu = nguoiDung.TieuSu,
                NgayTao = nguoiDung.NgayTao,
                TimeKhoa = nguoiDung.TimeKhoa,
                GioiTinh = nguoiDung.GioiTinh
            };
        }
    }
}

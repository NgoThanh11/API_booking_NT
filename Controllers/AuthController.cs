using Booking_thanhnt.Data;
using Booking_thanhnt.DTOs.Auth;
using Booking_thanhnt.Models;
using Booking_thanhnt.Models.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Booking_thanhnt.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        #region LOGIN

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Vui lòng nhập tên đăng nhập"
                });
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Vui lòng nhập mật khẩu"
                });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(x =>
                    x.Username == request.Username);

            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "Tài khoản hoặc mật khẩu không đúng"
                });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Tài khoản đã bị khóa"
                });
            }

            // Kiểm tra password bằng BCrypt
            bool isPasswordValid =
                BCrypt.Net.BCrypt.Verify(
                    request.Password,
                    user.PasswordHash
                );

            if (!isPasswordValid)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Tài khoản hoặc mật khẩu không đúng"
                });
            }

            // Tạo JWT
            var token = GenerateToken(user);

            return Ok(new
            {
                success = true,
                message = "Đăng nhập thành công",

                data = new LoginResponse
                {
                    Token = token,
                    UserId = user.Id,
                    Username = user.Username,
                    FullName = user.FullName,
                    Role = user.Role
                }
            });
        }
        #endregion
        // =========================
        // GENERATE JWT
        // =========================
        private string GenerateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()
                ),

                new Claim(
                    ClaimTypes.Name,
                    user.Username
                ),

                new Claim(
                    ClaimTypes.Role,
                    user.Role
                ),

                new Claim(
                    "FullName",
                    user.FullName
                )
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"]!
                )
            );

            var credentials =
                new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256
                );

            var expireMinutes =
                int.Parse(
                    _configuration["Jwt:ExpireMinutes"]!
                );

            var token = new JwtSecurityToken(
                issuer:
                    _configuration["Jwt:Issuer"],

                audience:
                    _configuration["Jwt:Audience"],

                claims: claims,

                expires:
                    DateTime.UtcNow.AddMinutes(
                        expireMinutes
                    ),

                signingCredentials:
                    credentials
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }

        #region logout
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            return Ok(new
            {
                success = true,
                message = "Đăng xuất thành công"
            });
        }
        #endregion

        #region Đăng ký tài khoản
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vui lòng nhập tên đăng nhập"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.PasswordHash))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vui lòng nhập mật khẩu"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.FullName))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vui lòng nhập họ tên"
                    });
                }

                // Kiểm tra username đã tồn tại
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(x =>
                        x.Username == request.Username);

                if (existingUser != null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Tên đăng nhập đã tồn tại"
                    });
                }

                // Hash password
                var passwordHash =
                    BCrypt.Net.BCrypt.HashPassword(request.PasswordHash);

                var user = new User
                {
                    Username = request.Username.Trim(),
                    PasswordHash = passwordHash,
                    FullName = request.FullName.Trim(),

                    // Tài khoản đăng ký từ Client
                    // mặc định là Customer
                    Role = "Customer",

                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Đăng ký tài khoản thành công",
                    data = new
                    {
                        userId = user.Id,
                        username = user.Username,
                        fullName = user.FullName,
                        role = user.Role
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi đăng ký",
                    error = ex.Message
                });
            }
        }
        #endregion
    }
}
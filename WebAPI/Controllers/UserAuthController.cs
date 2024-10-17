using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using WebAPI.Content;
using WebAPI.DTOs;
using WebAPI.Helper;
using WebAPI.Models;
using WebAPI.Service;

namespace WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserAuthController : ControllerBase
    {
        private readonly QuanLyThuVienContext _context;
        private readonly IEmailService _emailService;
        private static int OTP_email;

        public UserAuthController(QuanLyThuVienContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }


        [HttpGet("{phoneNumber}/{password}")]
        public async Task<IActionResult> CheckUserLogin(string phoneNumber, string password)
        {
            var loginDg = await _context.LoginDgs.FindAsync(phoneNumber);

            if (loginDg == null || loginDg.PasswordDg != password)
            {
                return NotFound();
            }

            return Ok(loginDg);
        }

        // PUT: api/UserAuth/5
        [HttpPut]
        public async Task<IActionResult> UpdatePassword(string id, LoginDg loginDg)
        {
            if (id != loginDg.Sdt)
            {
                return BadRequest();
            }

            _context.Entry(loginDg).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoginDgExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> SendEMmail_Register([FromBody] JsonElement infoUser)
        {

            try
            {
                // Truy cập thông tin email và các thông tin khác
                string userEmail = infoUser.GetProperty("userEmail").GetString(); // Đảm bảo tên thuộc tính đúng
                string phoneNumber = infoUser.GetProperty("phoneNumber").GetString(); // Truy cập số điện thoại
                string username = infoUser.GetProperty("username").GetString(); // Truy cập tên người dùng

                if (LoginDgExists(phoneNumber) && _context.LoginDgs.Any(e => e.Email == userEmail))
                {
                    return BadRequest("Email hoặc số điện thoại đã tồn tại");
                }

                // tạo các đối tượng sendEmail
                var email = new SendEmailRegister();
                Random rd = new Random();
                int random = rd.Next(100000, 1000000);

                OTP_email = random;

                MailRequest mailRequest = new MailRequest();
                mailRequest.ToEmail = userEmail;
                mailRequest.Subject = "Xác thực đăng ký tài khoản";
                mailRequest.Body = email.SendEmail_Register(random, username);
                await _emailService.SendEmailAsync(mailRequest);

                return Ok("Gửi mail thành công");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }

        [HttpGet("{otp}")]
        public async Task<IActionResult> CheckEMmail_Register(int otp)
        {
            try
            {
                var systemOTP = OTP_email;

                if (otp == systemOTP)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("OTP sai!!!");
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }

        // POST: api/UserAuth
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserAuthentication newRegister)
        {

            LoginDg register = new LoginDg()
            {
                Sdt = newRegister.Sdt,
                PasswordDg = newRegister.PasswordDg,
                Email = newRegister.Email,
                HoTen = newRegister.HoTen,
            };

            try
            {
                _context.LoginDgs.Add(register);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateException e)
            {   
                return BadRequest(e.InnerException);
            }

        }

        private bool LoginDgExists(string sdt)
        {
            return _context.LoginDgs.Any(e => e.Sdt == sdt);
        }

        [HttpGet("{email}")]
        public IActionResult CheckUserLoginByGoogle(string email)
        {
            if (_context.LoginDgs.Any(e => e.Email == email))
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }


        [HttpGet("{sdt}")]
        public IActionResult HistoryOfBorrowingBooks(string sdt)
        {
            var dkiMuonSach = _context.DkiMuonSaches.Where(s => s.Sdt == sdt).ToList();

            return Ok(dkiMuonSach);
        }


        [HttpGet("{userEmail}")]
        public IActionResult GetSdtByEmail(string userEmail)
        {
            LoginDg user = _context.LoginDgs.FirstOrDefault(s => s.Email == userEmail);

            return Ok(user.Sdt);
        }


        [HttpPut("{maDK}")]
        public async Task<IActionResult> CancelOrderBooks(int maDK)
        {
            try
            {
                // Tạo câu lệnh SQL để cập nhật dữ liệu
                var sql = "UPDATE DkiMuonSach SET Tinhtrang = @tinhTrang WHERE MaDk = @maDK";
                var parameters = new[]
                {
                    new SqlParameter("@tinhTrang", 3),  // Cập nhật trạng thái Tinhtrang thành 3
                    new SqlParameter("@maDK", maDK)
                };

                // Thực thi câu lệnh SQL
                await _context.Database.ExecuteSqlRawAsync(sql, parameters);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }



        [HttpGet("{maDK}")]
        public IActionResult DetailsOrderBooks(int maDK)
        {
            try
            {
                // lấy ra danh sách chi tiết đăng ký theo mã đăng ký nhập vào
                var details = _context.ChiTietDks.Where(d => d.MaDk == maDK).ToList();

                // tạo một danh sách chứa chi tiết đăng ký không có các liên kết khoá ngoại

                List<ChiTietDangKyDTO> chiTietDkList = new List<ChiTietDangKyDTO>();


                foreach (var d in details)
                {
                    ChiTietDangKyDTO chiTietDk = new ChiTietDangKyDTO();

                    var chiTietDki = new ChiTietDk()
                    {
                        MaDk = d.MaDk,
                        MaSach = d.MaSach,
                        Soluongmuon = d.Soluongmuon
                    };

                    chiTietDk.tenSach = _context.Saches.Find(d.MaSach).TenSach;
                    chiTietDk.chiTietDk = chiTietDki;

                    chiTietDkList.Add(chiTietDk);
                }

                return Ok(chiTietDkList);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }

        }

    }
}

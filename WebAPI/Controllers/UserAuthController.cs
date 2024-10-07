using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using WebAPI.DTOs;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserAuthController : ControllerBase
    {
        private readonly QuanLyThuVienContext _context;

        public UserAuthController(QuanLyThuVienContext context)
        {
            _context = context;
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
                if (LoginDgExists(register.Sdt))
                {
                    return BadRequest("Tài khoản đã tồn tại");
                }
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

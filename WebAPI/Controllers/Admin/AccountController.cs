using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Areas.Admin.Data;
using WebAPI.Models;
using WebAPI.Service_Admin;

namespace WebAPI.Controllers.Admin
{
    [Route("api/admin/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AccountService _accountService;

        public AccountController(AccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            try
            {
                List<NhanVien> nhanViens = _accountService.GetAll();

                if (nhanViens != null)
                {
                    return Ok(nhanViens);
                }
                else
                {
                    return NotFound();
                }

            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        public IActionResult GetAllNhanVien()
        {
            try
            {
                List<DTO_NhanVien_LoginNV> nhanViens = _accountService.GetAllNhanVien();

                if (nhanViens != null)
                {
                    return Ok(nhanViens);
                }
                else
                {
                    return NotFound();
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                DTO_NhanVien_LoginNV nhanViens = _accountService.GetById(id);

                if(nhanViens != null)
                {
                    return Ok(nhanViens);
                }
                else
                {
                    return NotFound();
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("{username}/{password}")]
        public IActionResult Login(string username, string password)
        {
            try
            {
                DTO_NhanVien_LoginNV nhanViens = _accountService.Login(username, password);

                if(nhanViens != null)
                {
                    return Ok(nhanViens);
                }
                else
                {
                    return NotFound("Tài khoản hoặc mật khẩu không chính xác");
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        public IActionResult GetBySDT(string sdt)
        {
            try
            {
                NhanVien nhanVien = _accountService.GetBySDT(sdt);

                if(nhanVien != null)
                {
                    return Ok(nhanVien);
                }
                else
                {
                    return NotFound();
                }

            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult Insert([FromBody] NhanVien obj)
        {
            try
            {
                if (_accountService.Insert(obj))
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("So dien thoai da trung");
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        public async Task<IActionResult> ThemNhanVien([FromBody] DTO_NhanVien_LoginNV obj)
        {
            try
            {
                if (await _accountService.ThemNhanVien(obj))
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Số điện thoại đã trùng");
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult UpdateThongTinNhanVien([FromBody] DTO_NhanVien_LoginNV obj)
        {
            try
            {
                if (_accountService.UpdateThongTinNhanVien(obj))
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Số điện thoại hoặc tên đã trùng!");
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Areas.Admin.Data;
using WebAPI.Models;
using WebAPI.Service_Admin;

namespace WebAPI.Controllers.Admin
{
    [Route("api/admin/[controller]/[action]")]
    [ApiController]
    public class TheDocGiaController : ControllerBase
    {
        private readonly TheDocGiaService _theDocGiaService;

        public TheDocGiaController(TheDocGiaService theDocGiaService)
        {
            _theDocGiaService = theDocGiaService;
        }

        [HttpGet]
        public IActionResult GetAllTheDocGia()
        {
            try
            {
                List<DTO_DocGia_TheDocGia> listTheDocGia = _theDocGiaService.GetAllTheDocGia();

                return Ok(listTheDocGia);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("{id}")]
        public ActionResult ThongTinTheDocGia(int id)
        {
            try
            {
                DTO_DocGia_TheDocGia thongTinDocGia = _theDocGiaService.GetById(id);
                return Ok(thongTinDocGia);

            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        public IActionResult Update([FromBody] DTO_DocGia_TheDocGia obj)
        {
            try
            {
                if (_theDocGiaService.Update(obj))
                    return Ok();
                else
                    return BadRequest();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        public IActionResult DangKyTheDocGia([FromBody] DTO_DocGia_TheDocGia obj)
        {
            try
            {
                if (_theDocGiaService.DangKyTheDocGia(obj))
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Số điện thoại đã tồn tại!");
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

    }
}

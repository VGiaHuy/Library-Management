using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Areas.Admin.Data;
using WebAPI.Models;
using WebAPI.Service_Admin;

namespace WebAPI.Controllers.Admin
{
    [Route("api/admin/[controller]/[action]")]
    [ApiController]
    public class ThongTinDocGiaController : ControllerBase
    {
        private readonly ThongTinDocGiaService _thongTinDocGiaService;
        private readonly TheDocGiaService _theDocGiaService;

        public ThongTinDocGiaController(ThongTinDocGiaService thongTinDocGiaService, TheDocGiaService theDocGiaService)
        {
            _thongTinDocGiaService = thongTinDocGiaService;
            _theDocGiaService = theDocGiaService;
        }


        [HttpGet("{id}")]
        public ActionResult ThongTinTheDocGia(int id)
        {
            try
            {
                DTO_DocGia_TheDocGia thongTinDocGia = _theDocGiaService.GetById(id);
                return Ok(thongTinDocGia);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult UpdateThongTinDocGia([FromBody] DocGium obj)
        {
            try
            {
               if(_thongTinDocGiaService.UpdateThongTinDocGia(obj))
               {
                    return Ok();
               }
               else
               {
                    return BadRequest("Số điện thoại đã tồn tại");
               }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}

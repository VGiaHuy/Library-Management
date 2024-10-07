using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Areas.Admin.Data;
using WebAPI.Service_Admin;

namespace WebAPI.Controllers.Admin
{
    [Route("api/admin/[controller]/[action]")]
    [ApiController]
    public class DangKyMuonSachController : ControllerBase
    {
        private readonly DangKyMuonSachService _dangKyMuonSachService;

        public DangKyMuonSachController(DangKyMuonSachService dangKyMuonSachService)
        {
            _dangKyMuonSachService = dangKyMuonSachService;
        }

        [HttpGet]
        public IActionResult GetAllDangKyMuonSach()
        {
            try
            {

                List<DTO_DangKyMuonSach_GroupSDT> listDangKyMuonSach = _dangKyMuonSachService.GetAllDangKyMuonSach();

                if (listDangKyMuonSach.Count > 0)
                {
                    return Ok(listDangKyMuonSach);
                }
                else 
                { 
                    return NotFound(); 
                }

            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("{maDK}/{tinhTrang}")]
        public IActionResult UpdateTinhTrang(int maDK, int tinhTrang)
        {
            try
            {
                if (_dangKyMuonSachService.UpdateTinhTrang(maDK, tinhTrang))
                {
                    return Ok();
                }
                else
                {
                    return NotFound("Khong tim thay don");
                }

            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("{maDK}")]
        public IActionResult Get_CTDK_ByMaDK(int maDK)
        {
            try
            {
                List<DTO_Sach_Muon> listSachDK = _dangKyMuonSachService.Get_CTDK_ByMaDK(maDK);

                if(listSachDK.Count > 0) { return Ok(listSachDK); }
                else { return NotFound(); }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("{sdt}")]
        public IActionResult GetMaTheBySDT(string sdt)
        {
            try
            {
                int maThe = _dangKyMuonSachService.GetMaTheBySDT(sdt);

                if(maThe > 0)
                {
                    return Ok(maThe);
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


        [HttpGet("{maThe}")]
        public IActionResult CheckHanTheDocGia(int maThe)
        {
            try
            {
                if (_dangKyMuonSachService.CheckHanTheDocGia(maThe))
                {
                    return Ok();
                }
                else
                {
                    return NotFound("Khong tim thay du lieu");
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        public async Task<IActionResult> Insert([FromBody] DTO_Tao_Phieu_Muon x)
        {
            try
            {
                if (_dangKyMuonSachService.Insert(x))
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Da xay ra loi");
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("{maDK}")]
        public IActionResult Get_DangKySachMuon_ByMaDK(int maDK)
        {
            try
            {
                DTO_DangKyMuonSach data = _dangKyMuonSachService.Get_DangKySachMuon_ByMaDK(maDK);

                if(data != null)
                {
                    return Ok(data);
                }
                else
                {
                    return NotFound();
                }
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message); 
            }
        }


        [HttpGet("{SDT}")]
        public IActionResult CheckDocGia(string SDT)
        {
            try
            {
                if (_dangKyMuonSachService.CheckDocGia(SDT))
                {
                    return Ok();
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



    }
}

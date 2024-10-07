using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Areas.Admin.Data;
using WebAPI.DTOs.Admin;
using WebAPI.DTOs.Admin_DTO;
using WebAPI.Models;
using WebAPI.Service_Admin;
namespace WebAPI.Controllers.Admin

{
    [Route("api/admin/[controller]/[action]")]
    [ApiController]
    public class PhieuMuonController : ControllerBase
    {
        private readonly QuanLyThuVienContext _context;
        private readonly IMapper _mapper;
        private readonly PhieuMuonService _phieuMuonService;


        public PhieuMuonController(IMapper mapper, QuanLyThuVienContext context, PhieuMuonService phieuMuonService)
        {
            _context = context;
            _mapper = mapper;
            _phieuMuonService = phieuMuonService;

        }


        [HttpGet]
        public IActionResult GetAllDataToStart()
        {
            try
            {
                DataStartPhieuMuonDTO data = _phieuMuonService.GetAllDataToStart();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet]
        public IActionResult GetAllThongTinDangKy()
        {
            try
            {
                List<DKiMuonSachDTO_PM> dangky = _phieuMuonService.GetAllThongTinDangKy();
                return Ok(dangky);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }


        [HttpGet("{MaDK}")]
        public IActionResult Get_CTDK_ByMaDK(int maDK)
        {
            try
            {
                List<DTO_Sach_Muon> chiTietPhieuMuon = _phieuMuonService.Get_CTDK_ByMaDK(maDK);
                return Ok(chiTietPhieuMuon);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        public IActionResult Insert([FromBody] DTO_Tao_Phieu_Muon x)
        {
            try
            {
                var insert = _phieuMuonService.Insert(x);
                if (insert)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{maDK}/{tinhTrang}")]
        public IActionResult UpdateTinhTrang(int maDK, int tinhTrang)
        {
            try
            {
                if (_phieuMuonService.UpdateTinhTrang(maDK, tinhTrang))
                {
                    return Ok();

                }
                else
                {
                    return BadRequest();
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}

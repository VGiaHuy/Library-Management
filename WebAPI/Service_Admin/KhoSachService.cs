using Microsoft.EntityFrameworkCore;
using WebAPI.DTOs;
using WebAPI.DTOs.Admin_DTO;
using WebAPI.Models;

namespace WebAPI.Service_Admin
{
    public class KhoSachService
    {
        private readonly QuanLyThuVienContext _context;

        public KhoSachService(QuanLyThuVienContext context)
        {
            _context = context;
        }
        public async Task<PagingResult<SachDTO>> GetAllSachPaging(GetListPhieuTraPaging req)
        {
            var query =
                (from SachDTO in _context.Saches
                 join TtSach in _context.TtSaches
                 on SachDTO.MaSach equals TtSach.Masach
                 where string.IsNullOrEmpty(req.Keyword) || SachDTO.TenSach.Contains(req.Keyword) || SachDTO.TheLoai.Contains(req.Keyword) ||
                  SachDTO.NgonNgu.Contains(req.Keyword) ||
                  SachDTO.TacGia.Contains(req.Keyword)
                 select new SachDTO
                 {
                     MaSach = SachDTO.MaSach,
                     TenSach = SachDTO.TenSach,
                     TacGia = SachDTO.TacGia,
                     TheLoai = SachDTO.TheLoai,
                     NgonNgu = SachDTO.NgonNgu,
                     NamXb = SachDTO.NamXb,
                     Nxb = SachDTO.Nxb,
                     SoLuongHientai = SachDTO.SoLuongHientai,
                     Mota = TtSach.Mota
                 });


            var totalRow = await query.CountAsync();

            var listsachs = await query.OrderBy(x => x.MaSach)
                                       .Skip((req.Page - 1) * req.PageSize)
                                       .Take(req.PageSize)
                                       .ToListAsync();

            return new PagingResult<SachDTO>
            {
                Results = listsachs,
                CurrentPage = req.Page,
                RowCount = totalRow,
                PageSize = req.PageSize
            };
        }

    }
    
}

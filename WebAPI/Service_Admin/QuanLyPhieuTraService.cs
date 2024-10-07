using Microsoft.EntityFrameworkCore;
using WebAPI.DTOs.Admin_DTO;
using WebAPI.Models;

namespace WebAPI.Service_Admin
{
    public class QuanLyPhieuTraService

    {

        private readonly QuanLyThuVienContext _context;

        public QuanLyPhieuTraService(QuanLyThuVienContext context)
        {
            _context = context;
        }

        

        public async Task<PagingResult<PhieuTra_GroupMaPM_DTO>> GetAllPhieuTraPaging(GetListPhieuTraPaging req)
        {
            var query =
                (from PhieuTra in _context.PhieuTras
                 join PhieuMuon in _context.PhieuMuons
                 on PhieuTra.MaPm equals PhieuMuon.MaPm
                 join DocGia in _context.DocGia
                    on PhieuTra.MaThe equals DocGia.MaDg
                 join NhanVien in _context.NhanViens
                 on PhieuTra.MaNv equals NhanVien.MaNv
                 where string.IsNullOrEmpty(req.Keyword) || DocGia.HoTenDg.Contains(req.Keyword) || DocGia.Sdt.Contains(req.Keyword)
                 select new PhieuTra_DTO1
                 {
                     MaPT = PhieuTra.MaPt,
                     NgayMuon = PhieuMuon.NgayMuon,
                     NgayTra = PhieuTra.NgayTra,
                     MaPM = PhieuTra.MaPm.Value,
                     MaThe = DocGia.MaDg,
                     HoTenDG = DocGia.HoTenDg,
                     SDT = DocGia.Sdt,
                     MaNV = NhanVien.MaNv
                 }).AsQueryable()
            .GroupBy(g => new { g.MaPM, g.HoTenDG, g.MaThe, g.SDT }, (key, g) => new PhieuTra_GroupMaPM_DTO
            {
                PhieuTra_GroupKey = new PhieuTra_GroupKey()
                {
                    HoTenDG = key.HoTenDG,
                    SDT = key.SDT,
                    MaThe = key.MaThe,
                    MaPM = key.MaPM,
                },
                DataPhieuTras = g.Select(phieutra => new PhieuTra_DTO1
                {
                    MaPT = phieutra.MaPT,
                    NgayMuon = phieutra.NgayMuon,
                    NgayTra = phieutra.NgayTra,
                    MaPM = phieutra.MaPM,
                    MaThe = phieutra.MaThe,
                    HoTenDG = phieutra.HoTenDG,
                    SDT = phieutra.SDT,
                    MaNV = phieutra.MaNV
                }).ToList(),
                //CountRow = g.Count()
            });
            var totalRow = await query.CountAsync();

            var listPhieutras = await query.OrderByDescending(x => x.PhieuTra_GroupKey.MaPM).Skip((req.Page - 1) * req.PageSize).Take(req.PageSize).ToListAsync();

            return new PagingResult<PhieuTra_GroupMaPM_DTO>()
            {
                Results = listPhieutras,
                CurrentPage = req.Page,
                RowCount = totalRow,
                PageSize = req.PageSize
            };
        }


       // private List<PhieuTra_GroupMaPM_DTO> listPhieutra_All;
       // public IEnumerable<DTO_Sach_Tra> Get_ChiTietPT_ByMaPM(int maPM)
        public List<DTO_Sach_Tra> Get_ChiTietPT_ByMaPM(int maPM)
        {

            // Using LINQ to query the data
            var listPhieutra_All =
                (from chiTietPT in _context.ChiTietPts
                 join sach in _context.Saches on chiTietPT.MaSach equals sach.MaSach
                 join phieuTra in _context.PhieuTras on chiTietPT.MaPt equals phieuTra.MaPt
                 join phieuMuon in _context.PhieuMuons on phieuTra.MaPm equals phieuMuon.MaPm
                // join ChiTietPm in _context.ChiTietPms on phieuMuon.MaPm equals ChiTietPm.MaPm

                 where phieuTra.MaPm == maPM
                 select new DTO_Sach_Tra
                 {
                     MaPT = phieuTra.MaPt,
                     MaSach = sach.MaSach,
                     TenSach = sach.TenSach,
                   //  SoLuongMuon = ChiTietPm.Soluongmuon ??0,
                     SoLuongTra = chiTietPT.Soluongtra ?? 0,
                     SoLuongLoi = chiTietPT.Soluongloi ?? 0,
                     SoLuongMat = chiTietPT.Soluongmat ?? 0,
                 }).ToList();

            // Returning the distinct list based on specified fields
            return listPhieutra_All
                .GroupBy(x => new { x.MaPT, x.MaSach, x.SoLuongTra, x.SoLuongLoi, x.SoLuongMat, x.TenSach })
                .Select(x => x.First())
                .ToList();
        }

        public List<DTO_Sach_Tra> Get_CTPT_ByMaPT(int maPT)
        {

            // Using LINQ to query the data
            var listPhieutra_All =
                (from chiTietPT in _context.ChiTietPts
                 join sach in _context.Saches on chiTietPT.MaSach equals sach.MaSach
                 join phieuTra in _context.PhieuTras on chiTietPT.MaPt equals phieuTra.MaPt
                 join phieuMuon in _context.PhieuMuons on phieuTra.MaPm equals phieuMuon.MaPm
                 join ChiTietPm in _context.ChiTietPms on phieuMuon.MaPm equals ChiTietPm.MaPm

                 where phieuTra.MaPt == maPT
                 select new DTO_Sach_Tra
                 {
                     MaPT = phieuTra.MaPt,
                     MaSach = sach.MaSach,
                     TenSach = sach.TenSach,
                    // SoLuongMuon = ChiTietPm.Soluongmuon ?? 0,
                     SoLuongTra = chiTietPT.Soluongtra ?? 0,
                     SoLuongLoi = chiTietPT.Soluongloi ?? 0,
                     SoLuongMat = chiTietPT.Soluongmat ?? 0,
                 }).ToList();

            // Returning the distinct list based on specified fields
            return listPhieutra_All
                .GroupBy(x => new { x.MaPT, x.MaSach, x.SoLuongTra, x.SoLuongLoi, x.SoLuongMat, x.TenSach})
                .Select(x => x.First())
                .ToList();
            
        }
    }
}


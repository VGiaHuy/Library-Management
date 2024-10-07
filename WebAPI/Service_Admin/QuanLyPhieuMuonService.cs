using Microsoft.EntityFrameworkCore;
using WebAPI.DTOs.Admin_DTO;
using WebAPI.Models;

namespace WebAPI.Service_Admin
{
    public class QuanLyPhieuMuonService
    {
        private readonly QuanLyThuVienContext _context;

        public QuanLyPhieuMuonService(QuanLyThuVienContext context)
        {
            _context = context;
        }

        public async Task<PagingResult<PhieuMuon_GroupMaDG_DTO>> GetAllDocGiaPaging(GetListPhieuTraPaging req)
        {
            
            var query =
                (from DocGia in _context.DocGia
                 join PhieuMuon in _context.PhieuMuons
                 on DocGia.MaDg equals PhieuMuon.MaThe
                 join NhanVien in _context.NhanViens
                 on PhieuMuon.MaNv equals NhanVien.MaNv
                 where string.IsNullOrEmpty(req.Keyword) || DocGia.HoTenDg.Contains(req.Keyword)
                 select new PhieuMuonDTO
                 {
                     MaPM = PhieuMuon.MaPm,
                     NgayMuon = PhieuMuon.NgayMuon,
                     HanTra = PhieuMuon.HanTra,
                     MaThe = DocGia.MaDg,
                     HoTenDG = DocGia.HoTenDg,
                     SDT = DocGia.Sdt,
                     MaNV = NhanVien.MaNv,
                     Tinhtrang = PhieuMuon.Tinhtrang

                 }).AsQueryable()
            .GroupBy(g => new { g.MaThe, g.HoTenDG, g.SDT }, (key, g) => new PhieuMuon_GroupMaDG_DTO
            {
                DocGia_GroupKey = new DocGia_GroupKey()
                {
                    MaThe = key.MaThe,
                    HoTenDG = key.HoTenDG,
                    SDT = key.SDT,
                   
                },
                DataPhieuMuons = g.Select(phieumuon => new PhieuMuonDTO
                {
                    MaPM = phieumuon.MaPM,
                    NgayMuon = phieumuon.NgayMuon,
                    HanTra = phieumuon.HanTra,
                    MaThe = phieumuon.MaThe,
                    HoTenDG = phieumuon.HoTenDG,
                    SDT = phieumuon.SDT,
                    MaNV = phieumuon.MaNV,
                    Tinhtrang = phieumuon.Tinhtrang,
                }).ToList(),
                //CountRow = g.Count()
            });
            var totalRow = await query.CountAsync();

            var listPhieumuons = await query.OrderByDescending(x => x.DocGia_GroupKey.MaThe).Skip((req.Page - 1) * req.PageSize).Take(req.PageSize).ToListAsync();

            return new PagingResult<PhieuMuon_GroupMaDG_DTO>()
            {
                Results = listPhieumuons,
                CurrentPage = req.Page,
                RowCount = totalRow,
                PageSize = req.PageSize
            };
        }


        private List<PhieuTra_GroupMaPM_DTO> listPhieumuon_All;
        // public IEnumerable<DTO_Sach_Tra> Get_ChiTietPT_ByMaPM(int maPM)
        public List<SachMuon_allPmDTO> Get_ChiTietPM_ByMaDG(int maThe)
        {

            // Using LINQ to query the data
            var listPhieumuon_All =
                (from chiTietPM in _context.ChiTietPms
                 join sach in _context.Saches on chiTietPM.MaSach equals sach.MaSach
                 join phieuMuon in _context.PhieuMuons on chiTietPM.MaPm equals phieuMuon.MaPm
              
                 where phieuMuon.MaThe == maThe
                 select new SachMuon_allPmDTO
                 {
                     MaPM = phieuMuon.MaPm,
                     MaSach = sach.MaSach,
                     TenSach = sach.TenSach,
                     SoLuongMuon = chiTietPM.Soluongmuon ?? 0,
                    
                 }).ToList();

            // Returning the distinct list based on specified fields
            return listPhieumuon_All
                .GroupBy(x => new { x.MaPM, x.MaSach,  x.TenSach, x.SoLuongMuon })
                .Select(x => x.First())
                .ToList();
        }

        public List<SachMuon_allPmDTO> Get_ChiTietPM_ByMaPM(int maPM)
        {

            // Using LINQ to query the data
            var listPhieumuon_All =
                (from chiTietPM in _context.ChiTietPms
                 join sach in _context.Saches on chiTietPM.MaSach equals sach.MaSach
                 join phieuMuon in _context.PhieuMuons on chiTietPM.MaPm equals phieuMuon.MaPm

                 where phieuMuon.MaPm == maPM
                 select new SachMuon_allPmDTO
                 {
                     MaPM = phieuMuon.MaPm,
                     MaSach = sach.MaSach,
                     TenSach = sach.TenSach,
                     SoLuongMuon = chiTietPM.Soluongmuon ?? 0,

                 }).ToList();

            // Returning the distinct list based on specified fields
            return listPhieumuon_All
                .GroupBy(x => new { x.MaPM, x.MaSach, x.TenSach, x.SoLuongMuon })
                .Select(x => x.First())
                .ToList();
        }
    }
}

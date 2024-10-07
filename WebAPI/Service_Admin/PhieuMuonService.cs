using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebAPI.Areas.Admin.Data;
using WebAPI.DTOs.Admin;
using WebAPI.DTOs.Admin_DTO;
using WebAPI.Models;

namespace WebAPI.Service_Admin
{
    public class PhieuMuonService
    {
        private readonly QuanLyThuVienContext _context;

        public PhieuMuonService(QuanLyThuVienContext context)
        {
            _context = context;
        }

        public DataStartPhieuMuonDTO GetAllDataToStart()
        {
            DateTime now = DateTime.Now;

            var sach = _context.Saches.ToList();

            var dangKyMuonSach =
                (from DkiMuonSach in _context.DkiMuonSaches
                 join LoginDgs in _context.LoginDgs
                    on DkiMuonSach.Sdt equals LoginDgs.Sdt
                 join DocGia in _context.DocGia
                    on DkiMuonSach.Sdt equals DocGia.Sdt
                 join TheDocGia in _context.TheDocGia
                    on DocGia.MaDg equals TheDocGia.MaDg
                 where DkiMuonSach.Tinhtrang == 1 && TheDocGia.NgayHh >= DateOnly.FromDateTime(now)
                 select new DKiMuonSachDTO_PM
                 {
                     MaThe = DocGia.MaDg,
                     MaDK = DkiMuonSach.MaDk,
                     SDT = LoginDgs.Sdt,
                     HoTen = LoginDgs.HoTen,
                     NgayDK = DkiMuonSach.NgayDkmuon,
                     NgayHen = DkiMuonSach.NgayHen,
                 }).ToList();

            var docGia =
                (from DocGia in _context.DocGia
                 join TheDocGia in _context.TheDocGia
                    on DocGia.MaDg equals TheDocGia.MaDg
                 where TheDocGia.NgayHh >= DateOnly.FromDateTime(now)
                 select new DTO_DocGia_TheDocGia
                 {
                     MaThe = TheDocGia.MaThe,
                     HoTenDG = DocGia.HoTenDg,
                     SDT = DocGia.Sdt,
                     DiaChi = DocGia.DiaChi,
                 }
                 ).ToList();

            DataStartPhieuMuonDTO data = new DataStartPhieuMuonDTO()
            {
                sach = sach,
                docGia = docGia,
                dKiMuonSach = dangKyMuonSach
            };

            return data;
        }


        public List<DKiMuonSachDTO_PM> GetAllThongTinDangKy()
        {
            DateTime now = DateTime.Now;

            var listDangKyMuonSach =
            (from DkiMuonSach in _context.DkiMuonSaches
             join LoginDgs in _context.LoginDgs
                on DkiMuonSach.Sdt equals LoginDgs.Sdt
             join DocGia in _context.DocGia
                on DkiMuonSach.Sdt equals DocGia.Sdt
             join TheDocGia in _context.TheDocGia
                on DocGia.MaDg equals TheDocGia.MaDg
             where DkiMuonSach.Tinhtrang == 1 && TheDocGia.NgayHh >= DateOnly.FromDateTime(now)
             select new DKiMuonSachDTO_PM
             {
                 MaThe = DocGia.MaDg,
                 MaDK = DkiMuonSach.MaDk,
                 SDT = LoginDgs.Sdt,
                 HoTen = LoginDgs.HoTen,
                 NgayDK = DkiMuonSach.NgayDkmuon,
                 NgayHen = DkiMuonSach.NgayHen,
             }).ToList();

            return listDangKyMuonSach;

        }


        public List<DTO_Sach_Muon> Get_CTDK_ByMaDK(int maDK)
        {
            var sach = _context.ChiTietDks.Where(ctdk => ctdk.MaDk == maDK).ToList();

            // Tạo danh sách để lưu kết quả
            List<DTO_Sach_Muon> listSachDK = new List<DTO_Sach_Muon>();

            foreach (var ctdk in sach)
            {
                // Lấy tên sách tương ứng
                var tenSach = _context.Saches.Find(ctdk.MaSach)?.TenSach;

                // Tạo đối tượng DTO_Sach_Muon và thêm vào danh sách
                var new_listSachDK = new DTO_Sach_Muon
                {
                    MaSach = ctdk.MaSach,
                    SoLuong = (int)ctdk.Soluongmuon,
                    TenSach = tenSach,
                };

                listSachDK.Add(new_listSachDK);
            }

            return listSachDK;
        }


        public bool Insert(DTO_Tao_Phieu_Muon x)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var newPhieuMuon = new PhieuMuon
                    {
                        NgayMuon = x.NgayMuon,
                        HanTra = x.NgayTra,
                        MaThe = x.MaTheDocGia,
                        MaNv = x.MaNhanVien,
                        MaDk = x.MaDK,
                        Tinhtrang = false
                    };

                    _context.PhieuMuons.Add(newPhieuMuon);
                    _context.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu

                    foreach (var sachMuon in x.listSachMuon)
                    {
                        var newChiTietPM = new ChiTietPm
                        {
                            MaPm = newPhieuMuon.MaPm,
                            MaSach = sachMuon.MaSach,
                            Soluongmuon = sachMuon.SoLuong
                        };

                        _context.ChiTietPms.Add(newChiTietPM);
                    }

                    _context.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu

                    transaction.Commit();

                    return true;
                }
                catch(Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine(ex);
                    return false;
                }
            }


        }


        public bool UpdateTinhTrang(int maDK, int tinhTrang)
        {
            try
            {
                var dkiMuonSach = _context.DkiMuonSaches.FirstOrDefault(dk => dk.MaDk == maDK);

                if (dkiMuonSach != null)
                {
                    //dkiMuonSach.Tinhtrang = tinhTrang;
                    //_context.SaveChanges();

                    // Tạo câu lệnh SQL để cập nhật dữ liệu
                    var sql = "UPDATE DkiMuonSach SET Tinhtrang = @tinhTrang WHERE MaDk = @maDK";
                    var parameters = new[]
                    {
                   new SqlParameter("@tinhTrang", tinhTrang),  // Cập nhật trạng thái Tinhtrang thành 2
                   new SqlParameter("@maDK", maDK)
               };

                    // Thực thi câu lệnh SQL
                    _context.Database.ExecuteSqlRawAsync(sql, parameters);
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                return false;
            }

        }






        public async Task<PagingResult<PhieuMuonDTO>> GetAllPhieuMuonPaging(GetListPhieuMuonPaging req)
        {
            var query =
                (from PhieuMuon in _context.PhieuMuons
                 join DocGia in _context.DocGia
                    on PhieuMuon.MaThe equals DocGia.MaDg
                 join CHITIETPM in _context.ChiTietPms
                    on PhieuMuon.MaPm equals CHITIETPM.MaPm
                 where PhieuMuon.Tinhtrang == false
                       && (string.IsNullOrEmpty(req.Keyword) || DocGia.HoTenDg.Contains(req.Keyword))
                 select new PhieuMuonDTO
                 {
                     MaPM = PhieuMuon.MaPm,
                     MaThe = DocGia.MaDg,
                     HoTenDG = DocGia.HoTenDg,
                     SDT = DocGia.Sdt,
                     NgayMuon = PhieuMuon.NgayMuon,
                     HanTra = PhieuMuon.HanTra
                 }
                ).Distinct();

            var totalRow = await query.CountAsync();

            var listPhieumuons = await query.OrderByDescending(x => x.MaPM)
                                             .Skip((req.Page - 1) * req.PageSize)
                                             .Take(req.PageSize)
                                             .ToListAsync();

            return new PagingResult<PhieuMuonDTO>
            {
                Results = listPhieumuons,
                CurrentPage = req.Page,
                RowCount = totalRow,
                PageSize = req.PageSize
            };
        }

        public IEnumerable<SachMuonDTO> getSachMuon(int MaPm)
        {
            // Lấy danh sách sách đã trả
            var listSachTra = 
                (from PhieuMuon in _context.PhieuMuons
                from phieuTra in _context.PhieuTras
                join chiTietPT in _context.ChiTietPts on phieuTra.MaPt equals chiTietPT.MaPt
                where phieuTra.MaPm == MaPm
                group chiTietPT by chiTietPT.MaSach into g
                select new SachDaTraDTO
                {
                    MaSach = g.Key,
                    SoLuongDaTra = g.Sum(a => a.Soluongtra + a.Soluongloi + a.Soluongmat)
                }
            ).ToList();

            // Lấy danh sách sách mượn
            var sachMuonList = (
                from chiTietPM in _context.ChiTietPms
                join sach in _context.Saches on chiTietPM.MaSach equals sach.MaSach
                join CHITIETPN in _context.Chitietpns on chiTietPM.MaSach equals CHITIETPN.MaSach
                where chiTietPM.MaPm == MaPm
                select new SachMuonDTO
                {
                    MaSach = sach.MaSach,
                    TenSach = sach.TenSach,
                    SoLuongMuon = chiTietPM.Soluongmuon,
                    giasach = CHITIETPN.GiaSach.Value
                })
                .GroupBy(group => new { group.MaSach, group.TenSach, group.SoLuongMuon })
                .AsEnumerable()
                .Select(x =>
                {
                    // Tìm kiếm thông tin sách đã trả tương ứng
                    var sachDaTra = listSachTra.FirstOrDefault(s => s.MaSach == x.Key.MaSach);

                    // Nếu không tìm thấy, sử dụng giá trị mặc định là 0
                    int soLuongDaTra = sachDaTra?.SoLuongDaTra ?? 0;

                    // Tính toán số lượng còn lại của sách mượn
                    int? soLuongMuonConLaiNullable = x.Key.SoLuongMuon - soLuongDaTra;

                    // Chuyển đổi kiểu dữ liệu từ int? sang int
                    int soLuongMuonConLai = soLuongMuonConLaiNullable ?? 0;


                    // Tạo đối tượng SachMuonDTO mới
                    return new SachMuonDTO
                    {
                        MaSach = x.Key.MaSach,
                        TenSach = x.Key.TenSach,
                        SoLuongMuon = soLuongMuonConLai,
                        giasach = x.OrderByDescending(item => item.giasach).First().giasach
                    };
                })
                .Where(x => x.SoLuongMuon > 0)
                .ToList();

            // Trả về danh sách sách mượn còn lại
            return sachMuonList;
        }
    }
}

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebAPI.Areas.Admin.Data;
using WebAPI.Models;

namespace WebAPI.Service_Admin
{
    public class DangKyMuonSachService
    {
        private readonly QuanLyThuVienContext _context;

        public DangKyMuonSachService(QuanLyThuVienContext context)
        {
            _context = context;
        }


        public List<DTO_DangKyMuonSach_GroupSDT> GetAllDangKyMuonSach()
        {
            var listDangKyMuonSach =
                (from DkiMuonSach in _context.DkiMuonSaches
                 join LOGIN_DG in _context.LoginDgs
                    on DkiMuonSach.Sdt equals LOGIN_DG.Sdt

                 select new DTO_DangKyMuonSach
                 {
                     MaDK = DkiMuonSach.MaDk,
                     SDT = LOGIN_DG.Sdt,
                     HoTen = LOGIN_DG.HoTen,
                     Email = LOGIN_DG.Email,
                     //Password = LOGIN_DG.PASSWORD_DG,
                     NgayDK = DkiMuonSach.NgayDkmuon,
                     NgayHen = DkiMuonSach.NgayHen,
                     TinhTrang = DkiMuonSach.Tinhtrang,

                 }).AsEnumerable()
                 .GroupBy(g => g.SDT, (key, g) => new DTO_DangKyMuonSach_GroupSDT
                 {
                     SDT = key,
                     List_DTO_DangKyMuonSach = g.Select(item => new DTO_DangKyMuonSach
                     {
                         MaDK = item.MaDK,
                         SDT = item.SDT,
                         HoTen = item.HoTen,
                         Email = item.Email,
                         //Password = item.Password,
                         NgayDK = item.NgayDK,
                         NgayHen = item.NgayHen,
                         TinhTrang = item.TinhTrang,
                     }).ToList(),
                     CountRow = g.Count()
                 }).ToList();

            return listDangKyMuonSach;
        }


        public bool UpdateTinhTrang(int maDK, int tinhTrang)
        {
            try
            {
                var sql = "UPDATE DkiMuonSach SET Tinhtrang = @tinhTrang WHERE MaDk = @maDK";
                var parameters = new[]
                {
                    new SqlParameter("@tinhTrang", tinhTrang),
                    new SqlParameter("@maDK", maDK)
                };

                _context.Database.ExecuteSqlRaw(sql, parameters);
                return true;
            }
            catch (Exception ex)
            {
                // Log exception here if needed
                return false;
            }
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


        public int GetMaTheBySDT(string sdt)
        {
            var data =
                (from DocGia in _context.DocGia
                 join TheDocGia in _context.TheDocGia
                    on DocGia.MaDg equals TheDocGia.MaDg
                 where DocGia.Sdt == sdt
                 select new
                 {
                     maThe = TheDocGia.MaThe

                 }).FirstOrDefault();

            return data.maThe;
        }


        public bool CheckHanTheDocGia(int maThe)
        {
            var data = _context.TheDocGia
                       .Where(tdg => tdg.MaThe == maThe && tdg.NgayHh >= DateOnly.FromDateTime(DateTime.Now)).FirstOrDefault();

            if(data != null)
            {
                return true;
            }
            else
            {
                return false;
            }

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
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine(ex);
                    return false;
                }
            }


        }


        public DTO_DangKyMuonSach Get_DangKySachMuon_ByMaDK(int maDK)
        {
            var data =
                (from DkiMuonSach in _context.DkiMuonSaches
                 join LOGIN_DG in _context.LoginDgs
                    on DkiMuonSach.Sdt equals LOGIN_DG.Sdt
                 where DkiMuonSach.MaDk == maDK
                 select new DTO_DangKyMuonSach
                 {
                     MaDK = DkiMuonSach.MaDk,
                     SDT = LOGIN_DG.Sdt,
                     HoTen = LOGIN_DG.HoTen,
                     Email = LOGIN_DG.Email,
                     //Password = LOGIN_DG.PASSWORD_DG,
                     NgayDK = DkiMuonSach.NgayDkmuon,
                     NgayHen = DkiMuonSach.NgayHen,
                     TinhTrang = DkiMuonSach.Tinhtrang,

                 }).FirstOrDefault();

            return data;
        }


        public bool CheckDocGia(string SDT)
        {
            var data = _context.DocGia
                        .Where(dg => dg.Sdt == SDT)
                        .FirstOrDefault();

            return data != null;
        }


    }
}

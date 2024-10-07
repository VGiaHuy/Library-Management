﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.DTOs.Admin_DTO;
using WebAPI.Models;

namespace WebAPI.Service_Admin
{
    public class PhieuTraService
    {
        private readonly QuanLyThuVienContext _context;

        public PhieuTraService(QuanLyThuVienContext context)
        {
            _context = context;
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

            var listPhieumuons = await query.OrderByDescending(x => x.MaPM).Skip((req.Page - 1) * req.PageSize).Take(req.PageSize).ToListAsync();   

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
            var listSachTra = (
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

        public bool Insert(DTO_Tao_Phieu_Tra x)
        {
            if (x.ListSachTra.Any(sach => sach.SoLuongLoi > 0 || sach.SoLuongTra > 0 || sach.SoLuongMat > 0) == false)
            {
                return false;
            }
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Kiểm tra xem MaNV có tồn tại trong bảng NhanVien không
                    var nhanVien = _context.NhanViens.Find(x.MaNhanVien);
                    if (nhanVien == null)
                    {
                        throw new Exception($"NhanVien with MaNV = {x.MaNhanVien} does not exist.");
                    }
                    var phieuMuon = _context.PhieuMuons.FirstOrDefault(p => p.MaPm == x.MaPhieuMuon);

                    if (phieuMuon == null)
                    {
                        return false;
                    }

                    // Tạo đối tượng PhieuTra từ DTO_Tao_Phieu_Tra
                    var newPhieuTra = new PhieuTra
                    {
                        NgayTra = x.NgayTra,
                        MaThe = phieuMuon.MaThe,
                        MaNv = x.MaNhanVien,
                        MaPm = x.MaPhieuMuon,
                    };

                    // Thêm PhieuTra vào Context
                    _context.PhieuTras.Add(newPhieuTra);
                    _context.SaveChanges(); // Lưu để có thể lấy MaPT của newPhieuTra

                    // Duyệt qua danh sách sách trả và tạo đối tượng ChiTietPT cho mỗi cuốn sách
                    foreach (var sachtra in x.ListSachTra)
                    {
                        if (sachtra.SoLuongTra == 0 && sachtra.SoLuongLoi == 0 && sachtra.SoLuongMat == 0)
                        {
                            continue;
                        }
                        var newChiTietPT = new ChiTietPt
                        {
                            MaPt = newPhieuTra.MaPt,
                            MaSach = sachtra.MaSach,
                            Soluongtra = sachtra.SoLuongTra,
                            Soluongloi = sachtra.SoLuongLoi,
                            Soluongmat = sachtra.SoLuongMat,
                            PhuThu = sachtra.PhuThu,
                        };

                        // Thêm ChiTietPT vào Context
                        _context.ChiTietPts.Add(newChiTietPT);
                    }

                    // Lưu thay đổi vào cơ sở dữ liệu khi mọi thứ đã thành công
                    _context.SaveChanges();

                    transaction.Commit();

                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // Rollback nếu có lỗi
                    Console.WriteLine($"Error: {ex.Message}");
                    // Xử lý lỗi và ghi log
                    return false;
                }
            }
        }
    }
}

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using WebAPI.Areas.Admin.Data;
using WebAPI.DTOs.Admin_DTO;
using WebAPI.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebAPI.Service_Admin
{
    public class ThongKeService
    {
        private readonly QuanLyThuVienContext _context;

        public ThongKeService(QuanLyThuVienContext context)
        {
            _context = context;
        }

        //sách được nhập về trong khoảng tgian đó
        public List<ThongKeSach> GetSachNhap(DateOnly tungay, DateOnly denngay)
        {

            var result = (
                from pn in _context.PhieuNhapSaches
                join ctpn in _context.Chitietpns on pn.MaPn equals ctpn.MaPn
                join s in _context.Saches on ctpn.MaSach equals s.MaSach
                where pn.NgayNhap >= tungay && pn.NgayNhap <= denngay
                group new { ctpn, s } by new { s.MaSach, s.TenSach } into g
                select new ThongKeSach
                {
                    MaSach = g.Key.MaSach,
                    TenSach = g.Key.TenSach,
                    SoLuong = g.Sum(x => x.ctpn.SoLuongNhap)
                })
                .OrderByDescending(ts => ts.SoLuong)    
                .ToList();

            return result;
        }

        //sách được mượn đọc top 5
        public List<ThongKeSach> GetSachMuon(DateOnly? tungay, DateOnly? denngay)
        {

            var result = (
                from pm in _context.PhieuMuons
                join ctpm in _context.ChiTietPms on pm.MaPm equals ctpm.MaPm
                join s in _context.Saches on ctpm.MaSach equals s.MaSach
                where pm.NgayMuon >= tungay && pm.NgayMuon <= denngay
                group new { ctpm, s } by new { s.MaSach, s.TenSach } into g
                select new ThongKeSach
                {
                    MaSach = g.Key.MaSach,
                    TenSach = g.Key.TenSach,
                    SoLuong = g.Sum(x => x.ctpm.Soluongmuon)
                })
               .OrderByDescending(ts => ts.SoLuong)
                .ToList();

            return result;
        }


        //tổng số phiếu mượn

        public ThongKePhieu GetPhieuMuon(DateOnly? tungay, DateOnly? denngay)
        {

            var result = (
                from pm in _context.PhieuMuons
                where pm.NgayMuon >= tungay && pm.NgayMuon <= denngay
            select pm);
            var count = result.Count();


            return new ThongKePhieu
            {
                SoLuong = count
            };
            
        }

        public List<ThongKePM> GetListPM(DateOnly? tungay, DateOnly? denngay)
        {

            var query =
                (from PhieuMuon in _context.PhieuMuons

                 where PhieuMuon.NgayMuon >= tungay && PhieuMuon.NgayMuon <= denngay

                 select new ThongKePM
                 {
                     MaPM = PhieuMuon.MaPm,
                     MaThe = PhieuMuon.MaThe.Value,
                     NgayMuon = PhieuMuon.NgayMuon,
                     Tinhtrang = PhieuMuon.Tinhtrang
                 }
                ).ToList();

            return query;
        }

        //tổng số lượng phiếu trả
        public ThongKePhieu GetPhieuTra(DateOnly? tungay, DateOnly? denngay)
        {

            var result = (
                from pt in _context.PhieuTras
                where pt.NgayTra >= tungay && pt.NgayTra <= denngay
                select pt);
            var count = result.Count();


            return new ThongKePhieu
            {
                SoLuong = count
            };

        }
        public List<ThongKePT> GetListPT(DateOnly? tungay, DateOnly? denngay)
        {

            var query =
                (from pt in _context.PhieuTras
                 where pt.NgayTra >= tungay && pt.NgayTra <= denngay

                 select new ThongKePT
                 {
                     MaPT = pt.MaPt,
                     MaThe = pt.MaThe.Value,
                     NgayTra = pt.NgayTra,
                    
                 }
                ).ToList();

            return query;
        }

        //lấy  độc giả mượn 
        public List<ThongKeDocGia_Muon> GetDocGiaMuon(DateOnly? tungay, DateOnly? denngay)
        {

            var result = (
                from DocGium in _context.DocGia
                join TheDocGium in _context.TheDocGia on DocGium.MaDg equals TheDocGium.MaDg
                join PhieuMuon in _context.PhieuMuons on TheDocGium.MaThe equals PhieuMuon.MaThe
                where PhieuMuon.NgayMuon >= tungay && PhieuMuon.NgayMuon <= denngay
                group new { TheDocGium, DocGium, PhieuMuon} by new { TheDocGium.MaThe, DocGium.HoTenDg } into g
                select new ThongKeDocGia_Muon
                {
                    MaThe = g.Key.MaThe,
                    HoTenDg = g.Key.HoTenDg,
                    SoLuong = g.Count()
                })
                .OrderByDescending(ts => ts.SoLuong)
                .ToList();

            return result;
        }

        //độc giả đăng ký thẻ  trong khoảng thời gian đó
        public List<ThongKeDocGia_Dki> GetDocGiaDki(DateOnly? tungay, DateOnly? denngay)
        {

            var result = (
                 from DocGium in _context.DocGia
                 join TheDocGium in _context.TheDocGia on DocGium.MaDg equals TheDocGium.MaDg
                where TheDocGium.NgayDk >= tungay && TheDocGium.NgayDk <= denngay
                 group new { TheDocGium, DocGium } by new { TheDocGium.MaThe, DocGium.HoTenDg , TheDocGium.NgayDk} into g
                 select new ThongKeDocGia_Dki
                {
                    MaThe = g.Key.MaThe,
                    HoTenDg = g.Key.HoTenDg,
                    NgayDki = g.Key.NgayDk,
                 })
                
                .ToList();

            return result;
        }


        public async Task<ThongKeDoanhThu> GetMoney(DateOnly? tungay, DateOnly? denngay)
        {
            //using (var context2 = new QuanLyThuVienContext())
            //{
                var totalMoneyData = new ThongKeDoanhThu();

                var totalPhuthu = await (
                    from ct in _context.ChiTietPts
                    join pt in _context.PhieuTras on ct.MaPt equals pt.MaPt
                    where pt.NgayTra >= tungay && pt.NgayTra <= denngay
                    select ct.PhuThu
                ).SumAsync();

                var totalThanhLy = await (
                    from ctl in _context.ChiTietPtls
                    join ptl in _context.PhieuThanhLies on ctl.MaPtl equals ptl.MaPtl
                    where ptl.NgayTl >= tungay && ptl.NgayTl <= denngay
                    select ctl.GiaTl * ctl.Soluongtl
                ).SumAsync();

                var totalTheDocGia = await _context.TheDocGia
                    .Where(x => x.NgayDk >= tungay && x.NgayDk <= denngay)
                    .SumAsync(x => x.TienThe);

                var totalNhapSach = await (
                    from ctpn in _context.Chitietpns
                    join pn in _context.PhieuNhapSaches on ctpn.MaPn equals pn.MaPn
                    where pn.NgayNhap >= tungay && pn.NgayNhap <= denngay
                    select ctpn.GiaSach * ctpn.SoLuongNhap
                ).SumAsync();

                //await Task.WhenAll(totalPhuthu, totalThanhLy, totalTheDocGia, totalNhapSach);


                totalMoneyData.TotalTraSach =  totalPhuthu ?? 0;
                totalMoneyData.TotalThanhLy =  totalThanhLy ?? 0;
                totalMoneyData.TotalTheDocGia =  totalTheDocGia ?? 0;
                totalMoneyData.TotalNhapSach =  totalNhapSach ?? 0;

                return totalMoneyData;
            //}
        }


    }
}

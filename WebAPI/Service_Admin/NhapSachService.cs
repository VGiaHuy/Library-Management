using Microsoft.EntityFrameworkCore;
using WebAPI.Areas.Admin.Data;
using WebAPI.DTOs;
using WebAPI.DTOs.Admin_DTO;
using WebAPI.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebAPI.Service_Admin
{
    public class NhapSachService
    {
        private readonly QuanLyThuVienContext _context;

        public NhapSachService(QuanLyThuVienContext context)
        {
            _context = context;
        }

        public async Task<PagingResult<NhaCungCap>> GetAllNCCPaging(GetListPhieuMuonPaging req)
        {
            var query =
                 (from NhaCungCap in _context.NhaCungCaps
                  where string.IsNullOrEmpty(req.Keyword) || NhaCungCap.TenNcc.Contains(req.Keyword)
                  select new NhaCungCap
                  {
                      MaNcc = NhaCungCap.MaNcc,
                      TenNcc = NhaCungCap.TenNcc,
                      DiaChiNcc = NhaCungCap.DiaChiNcc,
                      SdtNcc = NhaCungCap.SdtNcc,
                  }).ToList();

            var totalRow = query.Count();

            var listNCCs = query.OrderBy(x => x.MaNcc).Skip((req.Page - 1) * req.PageSize).Take(req.PageSize).ToList();

            return new PagingResult<NhaCungCap>()
            {
                Results = listNCCs,
                CurrentPage = req.Page,
                RowCount = totalRow,
                PageSize = req.PageSize
            };
        }

        public async Task<PagingResult<SachDTO>> GetAllSachPaging(GetListPhieuMuonPaging req)
        {
            var query =
                (from SACH in _context.Saches
                 join TtSach in _context.TtSaches
                 on SACH.MaSach equals TtSach.Masach
                 where string.IsNullOrEmpty(req.Keyword) || SACH.TenSach.Contains(req.Keyword)
                 select new SachDTO
                 {
                     MaSach = SACH.MaSach,
                     TenSach = SACH.TenSach,
                     TacGia = SACH.TacGia,
                      TheLoai = SACH.TheLoai,
                       NgonNgu= SACH.NgonNgu,
                       Nxb  = SACH.Nxb,
                       NamXb = SACH.NamXb,
                       UrlImage = TtSach.UrlImage,
                      // GiaSac = SACH.GiaSach, 
                     SoLuongHientai = SACH.SoLuongHientai

                 }
                 ).ToList();
            var totalRow = query.Count();

            var listSachs = query.OrderBy(x => x.MaSach).Skip((req.Page - 1) * req.PageSize).Take(req.PageSize).ToList();

            return new PagingResult<SachDTO>()
            {
                Results = listSachs,
                CurrentPage = req.Page,
                RowCount = totalRow,
                PageSize = req.PageSize
            };
        }
        public InsertRes InsertNCC(NhaCungCap obj)
        {
            var res = new InsertRes()
            {
                success = false,
                errorCode = -1,
                message = "Thêm đơn vị không thành công."
            };

            if (obj.TenNcc == "" || obj.SdtNcc == "" || obj.DiaChiNcc == "")
            {
                return res;
            }
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var existingDonVi = _context.NhaCungCaps.FirstOrDefault(dv => dv.SdtNcc == obj.SdtNcc);

                    if (existingDonVi != null)
                    {
                        //throw new Exception("existingDonVi");
                        res.errorCode = -2;
                        res.message = "Số điện thoại đã tồn tại.";
                        return res;
                    }
                    else
                    {
                        var newNCC = new NhaCungCap();
                        {

                            newNCC.MaNcc = obj.MaNcc;
                            newNCC.TenNcc = obj.TenNcc;
                            newNCC.DiaChiNcc = obj.DiaChiNcc;
                            newNCC.SdtNcc = obj.SdtNcc;
                        };

                        _context.NhaCungCaps.Add(newNCC);

                        _context.SaveChanges();
                        transaction.Commit();

                        res.success = true;
                        res.message = "Thêm Thành công";
                        res.errorCode = 0;
                        return res;
                    }

                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // Rollback nếu có lỗi
                    Console.WriteLine($"Error: {ex.Message}");
                    // Xử lý lỗi và ghi log
                    return res;
                }
            }

        }

        public bool InsertPhieuNhap(DTO_Tao_Phieu_Nhap obj, List<string> imageUrls)
        {
            if (obj.listSachNhap.Any(sach => sach.SoLuong > 0) == false)
            {
                return false;
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var newPhieuNhap = new PhieuNhapSach
                    {
                        NgayNhap = obj.NgayNhap,
                        MaNv = obj.MaNhanVien,
                        MaNcc = obj.MaNhaCungCap,
                    };

                    _context.PhieuNhapSaches.Add(newPhieuNhap);
                    _context.SaveChanges(); // Save to get the generated MaPn

                    for (int i = 0; i < obj.listSachNhap.Count; i++)
                    {
                        var sachNhap = obj.listSachNhap[i];

                        if (sachNhap.MaSach > 0)
                        {
                            var newChiTietPN = new Chitietpn
                            {
                                MaPn = newPhieuNhap.MaPn,
                                MaSach = sachNhap.MaSach,
                                GiaSach = sachNhap.GiaSach,
                                SoLuongNhap = sachNhap.SoLuong,
                            };

                            _context.Chitietpns.Add(newChiTietPN);
                        }
                        else
                        {
                            var newSach = new Sach
                            {
                                TenSach = sachNhap.TenSach,
                                TheLoai = sachNhap.TheLoai,
                                TacGia = sachNhap.TacGia,
                                NgonNgu = sachNhap.NgonNgu,
                                Nxb = sachNhap.NhaXB,
                                NamXb = sachNhap.NamXB,
                                SoLuongHientai = 0,
                            };

                            _context.Saches.Add(newSach);
                            _context.SaveChanges(); // Save to get the generated MaSach

                            // Use the URL from the list
                            string url = null;
                            if (i < imageUrls.Count)
                            {
                                url = imageUrls[i];
                            }

                            var thongTinSach = new TtSach
                            {
                                Masach = newSach.MaSach,
                                Mota = sachNhap.MoTa,
                                UrlImage = url,
                            };
                            _context.TtSaches.Add(thongTinSach);

                            var newChiTietPN = new Chitietpn
                            {
                                MaPn = newPhieuNhap.MaPn,
                                MaSach = newSach.MaSach,
                                GiaSach = sachNhap.GiaSach,
                                SoLuongNhap = sachNhap.SoLuong,
                            };

                            _context.Chitietpns.Add(newChiTietPN);
                        }
                    }

                    _context.SaveChanges(); // Save all changes

                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // Rollback if any error occurs
                    Console.WriteLine($"Error: {ex.Message}");
                    // Log error
                    return false;
                }
            }
        }


        

    }
}

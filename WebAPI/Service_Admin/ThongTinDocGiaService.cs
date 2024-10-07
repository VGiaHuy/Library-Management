using Microsoft.AspNetCore.Http.HttpResults;
using WebAPI.Areas.Admin.Data;
using WebAPI.Models;

namespace WebAPI.Service_Admin
{
    public class ThongTinDocGiaService
    {
        private readonly QuanLyThuVienContext _context;

        public ThongTinDocGiaService(QuanLyThuVienContext context)
        {
            _context = context;
        }


        public bool UpdateThongTinDocGia(DocGium obj)
        {
            try
            {
                // Kiểm tra trùng số điện thoại, nếu có độc giả nào trùng sdt thì return false
                var existingDocGia = _context.DocGia.FirstOrDefault(dg => dg.Sdt == obj.Sdt && dg.MaDg != obj.MaDg);

                if (existingDocGia != null)
                {
                    return false;
                }
                else
                {
                    var docGiaUpdate = _context.DocGia.FirstOrDefault(t => t.MaDg == obj.MaDg);

                    docGiaUpdate.HoTenDg = obj.HoTenDg;
                    docGiaUpdate.NgaySinh = obj.NgaySinh;
                    docGiaUpdate.GioiTinh = obj.GioiTinh;
                    docGiaUpdate.Sdt = obj.Sdt;
                    docGiaUpdate.DiaChi = obj.DiaChi;

                    // Lưu thay đổi vào cơ sở dữ liệu
                    _context.SaveChanges();
                    return true;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Update: {ex.Message}");
                throw;
            }
        }

    }
}

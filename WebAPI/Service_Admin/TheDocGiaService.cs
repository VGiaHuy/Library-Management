using WebAPI.Areas.Admin.Data;
using WebAPI.Models;

namespace WebAPI.Service_Admin
{
    public class TheDocGiaService
    {
        private readonly QuanLyThuVienContext _context;

        public TheDocGiaService(QuanLyThuVienContext context)
        {
            _context = context;
        }

        public List<DTO_DocGia_TheDocGia> GetAllTheDocGia()
        {
            var listTheDocGia =
                (from DocGia in _context.DocGia
                 join TheDocGia in _context.TheDocGia
                    on DocGia.MaDg equals TheDocGia.MaDg
                 select new DTO_DocGia_TheDocGia
                 {
                     MaThe = TheDocGia.MaThe,
                     HoTenDG = DocGia.HoTenDg,
                     GioiTinh = DocGia.GioiTinh,
                     NgaySinh = DocGia.NgaySinh,
                     SDT = DocGia.Sdt,
                     DiaChi = DocGia.DiaChi,
                     NgayDangKy = TheDocGia.NgayDk,
                     NgayHetHan = TheDocGia.NgayHh,
                     TienThe = (decimal)TheDocGia.TienThe,
                 }
                 ).ToList();
            return listTheDocGia;
        }

        public bool Update(DTO_DocGia_TheDocGia obj)
        {
            try
            {
                var theDocGiaToUpdate = _context.TheDocGia.FirstOrDefault(t => t.MaThe == obj.MaThe);

                theDocGiaToUpdate.NgayHh = obj.NgayHetHan;
                theDocGiaToUpdate.TienThe = (int?)obj.TienThe;

                // Lưu thay đổi vào cơ sở dữ liệu
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Update: {ex.Message}");
                return false;
            }
        }


        public bool DangKyTheDocGia(DTO_DocGia_TheDocGia obj)
        {
            try
            {
                var existingDocGia = _context.DocGia.FirstOrDefault(dg => dg.Sdt == obj.SDT);

                if (existingDocGia != null)
                {
                    return false;
                }
                else
                {
                    // Tạo một đối tượng DocGia mới
                    var newDocGia = new DocGium
                    {
                        HoTenDg = obj.HoTenDG,
                        GioiTinh = obj.GioiTinh,
                        NgaySinh = obj.NgaySinh,
                        Sdt = obj.SDT,
                        DiaChi = obj.DiaChi,
                    };

                    // Thêm đối tượng DocGia mới vào DbSet DocGias
                    _context.DocGia.Add(newDocGia);
                    _context.SaveChanges(); // Lưu các thay đổi vào cơ sở dữ liệu

                    // Tạo một đối tượng TheDocGia mới
                    var newTheDocGia = new TheDocGium
                    {
                        MaDg = newDocGia.MaDg, // Sử dụng MaDG từ đối tượng DocGia vừa thêm
                        NgayDk = obj.NgayDangKy,
                        NgayHh = obj.NgayHetHan,
                        TienThe = (int)obj.TienThe,
                        MaNv = obj.MaNhanVien,
                    };

                    // Thêm đối tượng TheDocGia mới vào DbSet TheDocGias
                    _context.TheDocGia.Add(newTheDocGia);
                    _context.SaveChanges(); // Lưu các thay đổi vào cơ sở dữ liệu

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong DangKyTheDocGia: {ex.Message}");
                throw new Exception();
            }
        }

        public DTO_DocGia_TheDocGia GetById(int id)
        {
            try
            {
                var DTO_DocGia_TheDocGia = (
                    from DocGia in _context.DocGia
                    join TheDocGia in _context.TheDocGia
                    on DocGia.MaDg equals TheDocGia.MaDg
                    where TheDocGia.MaThe == id
                    select new DTO_DocGia_TheDocGia
                    {
                        MaThe = TheDocGia.MaThe,
                        MaDocGia = DocGia.MaDg,
                        MaNhanVien = (int)TheDocGia.MaNv,
                        HoTenDG = DocGia.HoTenDg,
                        SDT = DocGia.Sdt,
                        DiaChi = DocGia.DiaChi,
                        GioiTinh = DocGia.GioiTinh,
                        NgaySinh = DocGia.NgaySinh,
                        NgayDangKy = TheDocGia.NgayDk,
                        NgayHetHan = TheDocGia.NgayHh,
                        TienThe = (int)TheDocGia.TienThe,
                    }).FirstOrDefault();

                return DTO_DocGia_TheDocGia;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetById: {ex.Message}");
                throw;
            }
        }

    }
}

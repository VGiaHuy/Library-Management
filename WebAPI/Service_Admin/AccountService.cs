using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using WebAPI.Areas.Admin.Data;
using WebAPI.Models;

namespace WebAPI.Service_Admin
{
    public class AccountService
    {
        private readonly QuanLyThuVienContext _context;

        public AccountService(QuanLyThuVienContext context)
        {
            _context = context;
        }


        public List<NhanVien> GetAll()
        {
            return _context.NhanViens.ToList();
        }

        public List<DTO_NhanVien_LoginNV> GetAllNhanVien()
        {
            var listNhanVien =
                (from NhanVien in _context.NhanViens
                 select new DTO_NhanVien_LoginNV
                 {
                     MaNV = NhanVien.MaNv,
                     HoTenNV = NhanVien.HoTenNv,
                     GioiTinh = NhanVien.GioiTinh,
                     DiaChi = NhanVien.DiaChi,
                     NgaySinh = NhanVien.Ngaysinh,
                     SDT = NhanVien.Sdt,
                     ChucVu = NhanVien.ChucVu,
                 }
                 ).ToList();
            return listNhanVien;
        }

        public DTO_NhanVien_LoginNV GetById(int id)
        {
            try
            {
                var DTO_NhanVien_LoginNV = (
                    from NhanVien in _context.NhanViens
                    join LOGIN_NV in _context.LoginDg
                    on NhanVien.MaNv equals LOGIN_NV.Manv
                    where NhanVien.MaNv == id
                    select new DTO_NhanVien_LoginNV
                    {
                        MaNV = NhanVien.MaNv,
                        HoTenNV = NhanVien.HoTenNv,
                        NgaySinh = NhanVien.Ngaysinh,
                        GioiTinh = NhanVien.GioiTinh,
                        DiaChi = NhanVien.DiaChi,
                        ChucVu = NhanVien.ChucVu,
                        SDT = NhanVien.Sdt,
                        Username = LOGIN_NV.UsernameNv,
                        Password = LOGIN_NV.PasswordNv,
                    }).FirstOrDefault();

                return DTO_NhanVien_LoginNV;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetById: {ex.Message}");
                throw;
            }
        }

        public DTO_NhanVien_LoginNV Login(string username, string password)
        {
            try
            {
                 var nhanVien =
                 (from nv in _context.NhanViens
                  join lg in _context.LoginDg
                     on nv.MaNv equals lg.Manv
                  where lg.UsernameNv == username && password == lg.PasswordNv
                  select new DTO_NhanVien_LoginNV
                  {
                      MaNV = nv.MaNv,
                      HoTenNV = nv.HoTenNv,
                      SDT = nv.Sdt,
                      ChucVu = nv.ChucVu,
                      DiaChi = nv.DiaChi
                  }).FirstOrDefault();

                  return nhanVien;
                

            }
            catch(Exception ex)
            {
                throw;
            }

        }


        public NhanVien GetBySDT(string sdt)
        {
            var data = _context.NhanViens.FirstOrDefault(nv => nv.Sdt == sdt);

            if(data != null)
            {
                return data;
            }
            else
            {
                return new NhanVien();
            }
        }

        public bool Insert(NhanVien obj)
        {
            // kiem tra sdt co trung hay khong
            var data = _context.NhanViens.FirstOrDefault(nv => nv.Sdt == obj.Sdt);

            if (data == null)
            {
                _context.NhanViens.Add(obj);
                _context.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Update(NhanVien nhanvien)
        {
            var data = _context.NhanViens.FirstOrDefault(nv => nv.Sdt == nhanvien.Sdt);
            if(data != null )
            {
                _context.NhanViens.Update(nhanvien);
                _context.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }


        }

        public async Task<bool> ThemNhanVien(DTO_NhanVien_LoginNV obj)
        {
            try
            {
                var existingSDT = _context.NhanViens.FirstOrDefault(nv => nv.Sdt == obj.SDT);

                if (existingSDT != null)
                {
                    return false;
                }
                else
                {
                    // Tạo một đối tượng nhan viên mới
                    var newNhanVien = new NhanVien
                    {
                        HoTenNv = obj.HoTenNV,
                        Sdt = obj.SDT,
                        GioiTinh = obj.GioiTinh,
                        Ngaysinh = obj.NgaySinh,
                        DiaChi = obj.DiaChi,
                        ChucVu = obj.ChucVu,
                    };

                    _context.NhanViens.Add(newNhanVien);
                    await _context.SaveChangesAsync();


                    // Mã hóa mật khẩu với bcrypt
                    //string hashedPassword = BCrypt.Net.BCrypt.HashPassword(obj.Password, 12);

                    //Tạo một đối tượng loginNV mới
                    var newLoginNV = new LoginNv
                    {
                        Manv = newNhanVien.MaNv,
                        UsernameNv = obj.Username,
                        PasswordNv = obj.Password,
                    };

                    _context.LoginDg.Add(newLoginNV);
                    await _context.SaveChangesAsync();

                    return true;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi trong ThemNhanVien: {ex.Message}");
                throw;
            }
        }

        public string Delete(int id)
        {
            var nvRemove = _context.NhanViens.Find(id);

            if (nvRemove != null)
            {
                _context.NhanViens.Remove(nvRemove);
                return "Xoá thành công";
            }
            else
            {
                return "Xoá không thành công";

            }
        }

        public bool UpdateThongTinNhanVien(DTO_NhanVien_LoginNV obj)
        {
            try
            {
                // Kiểm tra sdt và userName có trùng với nhân viên nào khác hay không
                var existingSDT = _context.NhanViens.FirstOrDefault(nv => nv.Sdt == obj.SDT && nv.MaNv  != obj.MaNV);
                var existingUsername = _context.LoginDg.FirstOrDefault(login => login.UsernameNv == obj.Username && login.Manv != obj.MaNV);

                if (existingSDT != null)
                {
                    //throw new Exception("Số điện thoại đã tồn tại.");
                    return false;
                }
                else
                {
                    if (existingUsername != null)
                    {
                        //throw new Exception("Username đã tồn tại.");
                        return false;
                    }
                    else
                    {
                        var nhanVien = _context.NhanViens.FirstOrDefault(t => t.MaNv == obj.MaNV);
                        nhanVien.HoTenNv = obj.HoTenNV;
                        nhanVien.Sdt = obj.SDT;
                        nhanVien.Ngaysinh = obj.NgaySinh;
                        nhanVien.GioiTinh = obj.GioiTinh;
                        nhanVien.DiaChi = obj.DiaChi;
                        nhanVien.ChucVu = obj.ChucVu;

                        var login = _context.LoginDg.FirstOrDefault(t => t.Manv == obj.MaNV);

                        if (obj.Password.Trim() == "")
                        {
                            login.UsernameNv = obj.Username;
                            // Lưu thay đổi vào cơ sở dữ liệu
                            _context.SaveChanges();

                            return true;
                        }
                        else
                        {

                            login.UsernameNv = obj.Username;
                            login.PasswordNv = obj.Password;

                            _context.SaveChanges();

                            return true;
                        }
                    }
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models;

public partial class LoginDg
{
    [DisplayName("Số điện thoại")]
    [Required(ErrorMessage ="Vui lòng điền đầy đủ thông tin")]
    public required string Sdt { get; set; }

    [DisplayName("Mật khẩu")]
    [Required(ErrorMessage = "Vui lòng điền đầy đủ thông tin")]
    public string? PasswordDg { get; set; }

    [DisplayName("Họ và tên")]
    [Required(ErrorMessage = "Vui lòng điền đầy đủ thông tin")]
    public string? HoTen { get; set; }

    [DisplayName("Email")]
    [Required(ErrorMessage = "Vui lòng điền đầy đủ thông tin")]
    [EmailAddress(ErrorMessage ="Vui lòng điền đúng định dạng Email")]
    public string? Email { get; set; }

    public virtual ICollection<DkiMuonSach> DkiMuonSaches { get; set; } = new List<DkiMuonSach>();
}


USE QuanLyThuVien;
SELECT * FROM NHANVIEN
SELECT * FROM DOCGIA
SELECT * FROM THEDOCGIA
SELECT * FROM LOGIN_DG 
SELECT * FROM LOGIN_NV
SELECT * FROM NHACUNGCAP --where masach =15
SELECT * FROM SACH
select * from PHIEUNHAPSACH
select * from CHITIETPN 
SELECT * FROM CUONSACH 
SELECT * FROM DONVITL
SELECT * FROM PHIEUMUON
select * from CHITIETPM
SELECT * FROM CHITIETSACHMUON
SELECT * FROM PHIEUTRA
SELECT * FROM ChiTietPT
SELECT * FROM CHITIETSACHTRA
SELECT * FROM KhoSachThanhLy
SELECT * FROM CHITIETKHOTHANHLY
SELECT * FROM PHIEUTHANHLY
SELECT * FROM CHITIETPTL-- WHERE TINHTRANG = 2
SELECT * FROM CHITIETSACHTHANHLY
SELECT * FROM DKIMUONSACH
SELECT * FROM CHITIETDK
SELECT * FROM ImportSachTemp
SELECT * FROM CUONSACH where maCUONsach in('26D1','26D2','27D1','27D2')
SELECT * FROM KhoSachThanhLy
SELECT * FROM CHITIETKHOTHANHLY


-------------------------------------------------------------------------------
-- lấy số lượng sách được nhập về từ ngày tới ngày 
select sach.masach, sach.tensach, sum(soluongnhap)
from chitietpn join phieunhapsach on phieunhapsach.mapn = chitietpn.mapn  
join sach  on sach.masach = chitietpn.masach 
where ngaynhap > = '2024-03-20'
group by sach.masach, sach.tensach	
order by  sum(soluongnhap) desc



-- lấy số lượng sách được mượn  từ ngày tới ngày 
select sach.masach, sach.tensach, sum(soluongmuon)
from chitietpm join phieumuon on phieumuon.mapm = chitietpm.mapm
join sach  on sach.masach = chitietpm.masach 
where ngaymuon > = '2024-03-20'
group by sach.masach, sach.tensach
order by  sum(soluongmuon) desc



-- lấy top 3 số lượng sách được mượn  nhieu nhất 
select top 3 sach.masach, sach.tensach, sum(soluongmuon)
from chitietpm join phieumuon on phieumuon.mapm = chitietpm.mapm
join sach  on sach.masach = chitietpm.masach 
group by sach.masach, sach.tensach
order by  sum(soluongmuon) desc


--tổng phiếu mượn - phiếu trả
select count(mapm)
from phieumuon 
where ngaymuon >= '2024-03-20'


select count(mapt)
from phieutra 
where ngaytra >= '2024-03-20'



-- lấy độc giả mượn sách nhieu nhất
select thedocgia.mathe, hotendg, count(thedocgia.mathe)
from thedocgia join phieumuon on thedocgia.mathe = phieumuon.mathe
join docgia on thedocgia.mathe = docgia.madg
where ngaymuon >= '2024-03-20'
group by thedocgia.mathe,hotendg


--lấy độc giả đăng ký thẻ từ ngày đến ngày 
select thedocgia.mathe, hotendg
from thedocgia 
join docgia on thedocgia.mathe = docgia.madg
where ngayDk >= '2024-03-20'
group by thedocgia.mathe,hotendg 

select sum(phuthu)
from chitietpt

select sum(giatl)
from chitietptl

select sum(tienthe)
from thedocgia

select sum(giasach* soluongnhap)
from chitietpn


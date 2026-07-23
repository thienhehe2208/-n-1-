SELECT COUNT(*) AS SoDauSach FROM Sach;
   SELECT COUNT(*) AS SoBanSao FROM BanSao;
   SELECT COUNT(*) AS SoVaiTro FROM AspNetRoles;
   SELECT COUNT(*) AS SoTaiKhoan FROM AspNetUsers;
   SELECT TOP (20) MaSach, TenSach, ISBN, GiaSach FROM Sach ORDER BY MaSach;
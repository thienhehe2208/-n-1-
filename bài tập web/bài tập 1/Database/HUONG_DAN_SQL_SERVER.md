# Chạy dự án với SQL Server

## Tạo database bằng script

1. Mở SQL Server Management Studio (SSMS).
2. Kết nối tới `(localdb)\MSSQLLocalDB`.
3. Mở file `quanlythuvien.sql` và chọn **Execute**.
4. Script sẽ tự tạo database `QuanLyThuVien` cùng toàn bộ bảng, khóa ngoại và chỉ mục.
5. Chạy dự án. Dữ liệu sách mẫu, các vai trò và tài khoản quản trị sẽ được ứng dụng tự thêm ở lần chạy đầu.

Tài khoản quản trị mặc định:

- Email: `admin@thuvien.com`
- Mật khẩu: `Admin@123`

Ứng dụng cũng có thể tự tạo database bằng migration khi chạy nếu tài khoản Windows có quyền tạo database.

## Nếu dùng SQL Server Express

Đổi connection string trong `appsettings.json` thành:

```json
"bài_tập_1Context": "Server=.\\SQLEXPRESS;Database=QuanLyThuVien;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

## Nếu dùng tài khoản SQL Server

```json
"bài_tập_1Context": "Server=localhost;Database=QuanLyThuVien;User Id=sa;Password=MAT_KHAU_CUA_BAN;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

Không commit mật khẩu thật lên Git.

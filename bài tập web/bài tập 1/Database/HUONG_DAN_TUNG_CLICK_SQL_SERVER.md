# HƯỚNG DẪN TỪNG CÚ CLICK: SQL SERVER + DỰ ÁN QUẢN LÝ THƯ VIỆN

Tài liệu này áp dụng đúng cho dự án ASP.NET Core MVC trong thư mục `bài tập 1`.

## Thông tin đã cấu hình sẵn

- SQL Server: `.\SQLEXPRESS`
- Database: `QuanLyThuVien`
- Kiểu đăng nhập SQL Server: Windows Authentication
- Tài khoản quản trị website: `admin@thuvien.com`
- Mật khẩu quản trị website: `Admin@123`
- File tạo database: `Database\quanlythuvien.sql`

> Không dùng tài khoản quản trị mẫu và mật khẩu mẫu khi đưa website lên Internet.

## Phần A — Mở và kết nối SQL Server Management Studio

1. Nhấn nút **Start** của Windows.
2. Gõ `SQL Server Management Studio`.
3. Bấm **SQL Server Management Studio** để mở.
4. Nếu cửa sổ **Connect to Server** chưa hiện:
   - Bấm menu **Object Explorer**.
   - Bấm **Connect**.
   - Bấm **Database Engine...**.
5. Tại **Server type**, chọn **Database Engine**.
6. Tại **Server name**, nhập chính xác:

   ```text
   .\SQLEXPRESS
   ```

7. Tại **Authentication**, chọn **Windows Authentication**.
8. Nếu có nút **Options >>**, không cần thay đổi mục nào.
9. Bấm **Connect**.
10. Nhìn bên trái, trong **Object Explorer**, phải thấy tên máy kèm `SQLEXPRESS`.

Nếu kết nối thất bại, xem phần xử lý lỗi ở cuối tài liệu.

## Phần B — Tạo database QuanLyThuVien bằng file SQL

1. Trong SSMS, bấm menu **File**.
2. Bấm **Open**.
3. Bấm **File...**.
4. Đi tới thư mục:

   ```text
   D:\Đồ Án 1\Đồ Án 1\-n-1-\bài tập web\bài tập 1\Database
   ```

5. Chọn file **quanlythuvien.sql**.
6. Bấm **Open**.
7. Kiểm tra ô chọn server trên thanh công cụ đang là `TVT-25\SQLEXPRESS` hoặc `.\SQLEXPRESS`.
8. Bấm nút **Execute** có biểu tượng tam giác xanh, hoặc nhấn **F5**.
9. Chờ đến khi tab **Messages** hiện thông báo hoàn tất, không có dòng lỗi màu đỏ.
10. Trong **Object Explorer**, bấm chuột phải vào **Databases**.
11. Bấm **Refresh**.
12. Bấm dấu `+` trước **Databases**.
13. Phải thấy database **QuanLyThuVien**.
14. Bấm dấu `+` trước **QuanLyThuVien**.
15. Bấm dấu `+` trước **Tables**.
16. Phải thấy các bảng nghiệp vụ như:
    - `dbo.Sach`
    - `dbo.BanSao`
    - `dbo.DocGia`
    - `dbo.NhanVien`
    - `dbo.PhieuMuon`
    - `dbo.ChiTietPhieuMuon`
    - `dbo.PhieuPhat`
    - `dbo.DatTruoc`
17. Đồng thời phải có các bảng tài khoản bắt đầu bằng `dbo.AspNet...`.

Script có thể chạy lại an toàn khi migration đã được ghi nhận. Không xóa database đang có dữ liệu thật chỉ để chạy lại hướng dẫn.

## Phần C — Kiểm tra chuỗi kết nối trong Visual Studio

1. Mở **File Explorer**.
2. Đi tới:

   ```text
   D:\Đồ Án 1\Đồ Án 1\-n-1-\bài tập web
   ```

3. Bấm đúp file **bài tập web.slnx**.
4. Nếu Windows hỏi chương trình mở file, chọn **Visual Studio 2022**.
5. Chờ Visual Studio tải xong solution và restore các package.
6. Trong **Solution Explorer**, bấm dấu `>` trước project **bài tập 1**.
7. Bấm đúp file **appsettings.json**.
8. Kiểm tra phần cấu hình có đúng nội dung sau:

   ```json
   "DatabaseProvider": "SqlServer",
   "ConnectionStrings": {
     "bài_tập_1Context": "Server=.\\SQLEXPRESS;Database=QuanLyThuVien;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   }
   ```

9. Nếu vừa sửa bằng tay, nhấn **Ctrl+S** để lưu.

Ý nghĩa:

- `Server=.\\SQLEXPRESS`: dùng SQL Server Express trên máy hiện tại.
- `Database=QuanLyThuVien`: kết nối database vừa tạo.
- `Trusted_Connection=True`: dùng tài khoản Windows, không cần ghi mật khẩu SQL vào code.
- `TrustServerCertificate=True`: tránh lỗi chứng chỉ khi chạy SQL Server cục bộ.

## Phần D — Build dự án

1. Trong Visual Studio, bấm menu **Build**.
2. Bấm **Build Solution**.
3. Hoặc nhấn **Ctrl+Shift+B**.
4. Chờ cửa sổ **Output** chạy xong.
5. Kết quả cần có dòng **Build succeeded** hoặc `0 Error(s)`.

Dự án hiện dùng .NET 6 nên Visual Studio có thể hiện cảnh báo .NET 6 đã hết vòng đời hỗ trợ và các cảnh báo nullable. Các cảnh báo này không ngăn dự án chạy; kết quả kiểm thử hiện tại là 0 lỗi build.

## Phần E — Chạy website lần đầu

1. Nhìn trên thanh công cụ Visual Studio.
2. Chọn profile chạy có tên project hoặc **https**.
3. Bấm nút tam giác xanh, hoặc nhấn **Ctrl+F5** để chạy không debug.
4. Nếu Windows Firewall hỏi quyền:
   - Chỉ chọn **Private networks**.
   - Bấm **Allow access**.
5. Chờ trình duyệt tự mở.
6. Trang chủ phải hiển thị danh sách sách.

Trong lần chạy đầu, `Program.cs` sẽ:

1. Kết nối tới `QuanLyThuVien`.
2. Áp dụng migration còn thiếu.
3. Thêm 12 đầu sách mẫu.
4. Thêm 36 bản sao, mỗi đầu sách có 3 bản.
5. Tạo ba vai trò `Admin`, `NhanVien`, `DocGia`.
6. Tạo tài khoản quản trị mẫu.

## Phần F — Đăng nhập và thử chức năng

1. Trên website, bấm **Đăng nhập**.
2. Trong ô email, nhập:

   ```text
   admin@thuvien.com
   ```

3. Trong ô mật khẩu, nhập:

   ```text
   Admin@123
   ```

4. Bấm nút **Đăng nhập**.
5. Mở mục quản lý sách hoặc dashboard.
6. Kiểm tra danh sách có 12 đầu sách.
7. Mở mục quản lý bản sao.
8. Kiểm tra có các mã như `TV-0001-01`.
9. Thử bấm **Thêm mới** một thể loại hoặc đầu sách.
10. Nhập dữ liệu thử rồi bấm **Lưu**.
11. Quay lại SSMS.
12. Bấm chuột phải bảng tương ứng, ví dụ `dbo.TheLoai`.
13. Bấm **Select Top 1000 Rows**.
14. Kiểm tra dòng vừa thêm đã xuất hiện. Đây là bằng chứng website đang ghi vào SQL Server.

## Phần G — Kiểm tra dữ liệu mẫu trực tiếp trong SSMS

1. Trong SSMS, bấm **New Query**.
2. Ở ô database trên thanh công cụ, chọn **QuanLyThuVien**.
3. Dán câu lệnh:

   ```sql
   SELECT COUNT(*) AS SoDauSach FROM Sach;
   SELECT COUNT(*) AS SoBanSao FROM BanSao;
   SELECT COUNT(*) AS SoVaiTro FROM AspNetRoles;
   SELECT COUNT(*) AS SoTaiKhoan FROM AspNetUsers;
   SELECT TOP (20) MaSach, TenSach, ISBN, GiaSach FROM Sach ORDER BY MaSach;
   ```

4. Bấm **Execute**.
5. Kết quả chuẩn sau lần chạy đầu:
   - `SoDauSach = 12`
   - `SoBanSao = 36`
   - `SoVaiTro = 3`
   - `SoTaiKhoan = 1`

## Phần H — Dừng website

1. Quay lại Visual Studio.
2. Nếu đang chạy debug, bấm nút hình vuông đỏ **Stop Debugging**.
3. Hoặc nhấn **Shift+F5**.
4. Nếu chạy bằng **Ctrl+F5**, đóng cửa sổ dòng lệnh của website.

## Xử lý lỗi thường gặp

### Lỗi “server was not found” hoặc “error 26”

1. Nhấn **Windows+R**.
2. Gõ `services.msc`.
3. Bấm **OK**.
4. Tìm **SQL Server (SQLEXPRESS)**.
5. Nếu cột Status chưa là **Running**, bấm chuột phải.
6. Bấm **Start**.
7. Mở lại SSMS và dùng server `.\SQLEXPRESS`.

### Máy bạn dùng SQLEXPRESS01 thay vì SQLEXPRESS

1. Trong SSMS thử kết nối `.\SQLEXPRESS01`.
2. Nếu kết nối được, mở `appsettings.json`.
3. Đổi `Server=.\\SQLEXPRESS` thành `Server=.\\SQLEXPRESS01`.
4. Nhấn **Ctrl+S**.
5. Build và chạy lại.

Database và ứng dụng bắt buộc phải trỏ cùng một instance. Nếu tạo database trên `SQLEXPRESS` nhưng ứng dụng trỏ `SQLEXPRESS01`, ứng dụng sẽ không nhìn thấy database đó.

### Lỗi chứng chỉ hoặc SSL

Kiểm tra cuối chuỗi kết nối phải có:

```text
TrustServerCertificate=True
```

### Lỗi đăng nhập database

Đảm bảo chuỗi kết nối có:

```text
Trusted_Connection=True
```

và trong SSMS đang chọn **Windows Authentication**.

### Không thấy dữ liệu mẫu

1. Chạy website ít nhất một lần và chờ trang chủ hiện đầy đủ.
2. Trong SSMS, bấm chuột phải **Databases** rồi bấm **Refresh**.
3. Chạy các câu `SELECT COUNT(*)` ở Phần G.
4. Kiểm tra Visual Studio và SSMS đều đang dùng `SQLEXPRESS`.

### Báo lỗi database đã tồn tại

Không cần tạo database thủ công thêm lần nữa. Mở database hiện có và chạy website. Script đã kiểm tra sự tồn tại của database và migration.

### Muốn làm lại database mẫu từ đầu

Việc xóa database sẽ mất toàn bộ dữ liệu. Chỉ thực hiện nếu chắc chắn không có dữ liệu cần giữ:

1. Dừng website.
2. Trong SSMS, bấm chuột phải **QuanLyThuVien**.
3. Bấm **Delete**.
4. Tích **Close existing connections**.
5. Bấm **OK**.
6. Chạy lại file `quanlythuvien.sql`.
7. Chạy lại website để thêm dữ liệu mẫu và tài khoản quản trị.


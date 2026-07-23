using bài_tập_1.Models;
using Microsoft.EntityFrameworkCore;

namespace bài_tập_1.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedLibraryDataAsync(bài_tập_1Context context)
        {
            if (await context.Sach.AnyAsync())
            {
                return;
            }

            var vanHoc = new TheLoai { TenTheLoai = "Văn học", MoTa = "Tiểu thuyết, truyện và các tác phẩm văn học." };
            var kyNang = new TheLoai { TenTheLoai = "Kỹ năng sống", MoTa = "Phát triển bản thân và kỹ năng giao tiếp." };
            var tamLy = new TheLoai { TenTheLoai = "Tâm lý học", MoTa = "Tư duy, hành vi và tâm lý con người." };
            var kinhTe = new TheLoai { TenTheLoai = "Kinh tế", MoTa = "Kinh doanh, tài chính và quản trị." };
            var lichSu = new TheLoai { TenTheLoai = "Lịch sử", MoTa = "Lịch sử Việt Nam và thế giới." };

            var nxbTre = Publisher("Nhà xuất bản Trẻ", "TP. Hồ Chí Minh", "nxbtre@example.com");
            var nxbTongHop = Publisher("NXB Tổng hợp TP.HCM", "TP. Hồ Chí Minh", "tonghop@example.com");
            var nxbTheGioi = Publisher("Nhà xuất bản Thế Giới", "Hà Nội", "thegioi@example.com");
            var nxbLaoDong = Publisher("Nhà xuất bản Lao Động", "Hà Nội", "laodong@example.com");

            var daleCarnegie = Author("Dale Carnegie", "Hoa Kỳ");
            var pauloCoelho = Author("Paulo Coelho", "Brazil");
            var jamesClear = Author("James Clear", "Hoa Kỳ");
            var danielKahneman = Author("Daniel Kahneman", "Hoa Kỳ");
            var yuvalHarari = Author("Yuval Noah Harari", "Israel");
            var georgeOrwell = Author("George Orwell", "Anh");
            var harperLee = Author("Harper Lee", "Hoa Kỳ");
            var robertKiyosaki = Author("Robert T. Kiyosaki", "Hoa Kỳ");
            var napoleonHill = Author("Napoleon Hill", "Hoa Kỳ");
            var nguyenNhatAnh = Author("Nguyễn Nhật Ánh", "Việt Nam");

            var books = new[]
            {
                Book("Đắc nhân tâm", "978604000001", 86000, 320, 2021, kyNang, nxbTongHop, daleCarnegie, "/images/books/dac-nhan-tam.jpg", "Nghệ thuật giao tiếp và ứng xử với mọi người."),
                Book("Nhà giả kim", "978604000002", 79000, 228, 2020, vanHoc, nxbTre, pauloCoelho, "/images/books/nha-gia-kim.jpg", "Hành trình theo đuổi ước mơ và vận mệnh của một chàng chăn cừu."),
                Book("Thói quen nguyên tử", "978604000003", 189000, 336, 2023, kyNang, nxbTheGioi, jamesClear, "/images/books/thoi-quen-nguyen-tu.jpg", "Phương pháp xây dựng thói quen tốt bằng những thay đổi nhỏ."),
                Book("Tư duy nhanh và chậm", "978604000004", 199000, 612, 2022, tamLy, nxbTheGioi, danielKahneman, "/images/books/tu-duy-nhanh-va-cham.jpg", "Khám phá hai hệ thống chi phối cách con người suy nghĩ."),
                Book("Sapiens: Lược sử loài người", "978604000005", 225000, 560, 2022, lichSu, nxbTheGioi, yuvalHarari, null, "Khái quát hành trình phát triển của loài người."),
                Book("1984", "978604000006", 120000, 368, 2021, vanHoc, nxbTre, georgeOrwell, null, "Tiểu thuyết phản địa đàng kinh điển về quyền lực và tự do."),
                Book("Giết con chim nhại", "978604000007", 135000, 420, 2020, vanHoc, nxbTre, harperLee, null, "Câu chuyện về công lý, lòng trắc ẩn và định kiến."),
                Book("Cha giàu cha nghèo", "978604000008", 145000, 336, 2022, kinhTe, nxbTre, robertKiyosaki, null, "Những góc nhìn nền tảng về tài chính cá nhân."),
                Book("Nghĩ giàu và làm giàu", "978604000009", 110000, 400, 2021, kinhTe, nxbLaoDong, napoleonHill, null, "Các nguyên tắc xây dựng tư duy hướng tới thành công."),
                Book("Cho tôi xin một vé đi tuổi thơ", "978604000010", 95000, 208, 2019, vanHoc, nxbTre, nguyenNhatAnh, null, "Một chuyến trở về thế giới tuổi thơ trong trẻo và hóm hỉnh."),
                Book("Mắt biếc", "978604000011", 125000, 300, 2022, vanHoc, nxbTre, nguyenNhatAnh, null, "Câu chuyện tình buồn gắn với làng quê Việt Nam."),
                Book("Homo Deus: Lược sử tương lai", "978604000012", 245000, 520, 2021, lichSu, nxbTheGioi, yuvalHarari, null, "Những dự báo và câu hỏi lớn về tương lai nhân loại.")
            };

            context.Sach.AddRange(books);
            await context.SaveChangesAsync();

            foreach (var book in books)
            {
                for (var copyNumber = 1; copyNumber <= 3; copyNumber++)
                {
                    context.BanSao.Add(new BanSao
                    {
                        MaSach = book.MaSach,
                        MaVach = $"TV-{book.MaSach:D4}-{copyNumber:D2}",
                        TinhTrang = TinhTrangBanSao.SanCo,
                        ViTriKe = $"K{((book.MaSach - 1) / 4) + 1}-N{((book.MaSach - 1) % 4) + 1}"
                    });
                }
            }

            await context.SaveChangesAsync();
        }

        private static Sach Book(
            string title,
            string isbn,
            decimal price,
            int pages,
            int year,
            TheLoai category,
            NhaXuatBan publisher,
            TacGia author,
            string? cover,
            string description)
        {
            var book = new Sach
            {
                TenSach = title,
                ISBN = isbn,
                GiaSach = price,
                SoTrang = pages,
                NamXuatBan = year,
                NgonNgu = "Tiếng Việt",
                TheLoai = category,
                NhaXuatBan = publisher,
                AnhBia = cover ?? string.Empty,
                MoTa = description
            };

            book.SachTacGias.Add(new SachTacGia { Sach = book, TacGia = author });
            return book;
        }

        private static NhaXuatBan Publisher(string name, string address, string email) => new()
        {
            TenNXB = name,
            DiaChi = address,
            Email = email,
            SoDienThoai = string.Empty
        };

        private static TacGia Author(string name, string nationality) => new()
        {
            HoTen = name,
            QuocTich = nationality,
            TieuSu = string.Empty
        };
    }
}

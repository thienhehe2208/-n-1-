using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bài_tập_1.Data;
using bài_tập_1.Models;
using bài_tập_1.Models.ViewModels;

namespace bài_tập_1.Controllers
{
    public class HomeController : Controller
    {
        private readonly bài_tập_1Context _context;

        public HomeController(bài_tập_1Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new TrangChuViewModel
            {
                SachMoi = await _context.Sach
                    .AsNoTracking()
                    .OrderByDescending(s => s.MaSach)
                    .Take(12)
                    .ToListAsync(),

                SachDangMuon = TaoDanhSachSachDangMuon(),

                TheLoaiPhoBien = TaoDanhSachTheLoai(),

                ThongBao = TaoDanhSachThongBao()
            };

            return View(model);
        }

        private static List<Sach> TaoDanhSachSachMoi()
        {
            return new List<Sach>
            {
                new Sach
                {
                    MaSach = 1,
                    TenSach = "Đắc nhân tâm",
                    ISBN = "978604000001",
                    GiaSach = 85000,
                    MaTheLoai = 1,
                    MaNXB = 1,
                    NamXuatBan = 2023,
                    SoTrang = 320,
                    NgonNgu = "Tiếng Việt",
                    MoTa = "Cuốn sách nổi tiếng về nghệ thuật giao tiếp.",
                    AnhBia = "/images/book1.jpg"
                },

                new Sach
                {
                    MaSach = 2,
                    TenSach = "Nhà giả kim",
                    ISBN = "978604000002",
                    GiaSach = 79000,
                    MaTheLoai = 1,
                    MaNXB = 1,
                    NamXuatBan = 2022,
                    SoTrang = 228,
                    NgonNgu = "Tiếng Việt",
                    MoTa = "Câu chuyện về hành trình theo đuổi ước mơ.",
                    AnhBia = "/images/book2.jpg"
                },

                new Sach
                {
                    MaSach = 3,
                    TenSach = "Thói quen nguyên tử",
                    ISBN = "978604000003",
                    GiaSach = 189000,
                    MaTheLoai = 2,
                    MaNXB = 1,
                    NamXuatBan = 2023,
                    SoTrang = 336,
                    NgonNgu = "Tiếng Việt",
                    MoTa = "Phương pháp xây dựng những thói quen tốt.",
                    AnhBia = "/images/book3.jpg"
                },

                new Sach
                {
                    MaSach = 4,
                    TenSach = "Tư duy nhanh và chậm",
                    ISBN = "978604000004",
                    GiaSach = 199000,
                    MaTheLoai = 2,
                    MaNXB = 1,
                    NamXuatBan = 2021,
                    SoTrang = 612,
                    NgonNgu = "Tiếng Việt",
                    MoTa = "Khám phá hai hệ thống tư duy của con người.",
                    AnhBia = "/images/book4.jpg"
                },

                new Sach
                {
                    MaSach = 5,
                    TenSach = "Sapiens - Lược sử loài người",
                    ISBN = "978604000005",
                    GiaSach = 215000,
                    MaTheLoai = 3,
                    MaNXB = 1,
                    NamXuatBan = 2022,
                    SoTrang = 560,
                    NgonNgu = "Tiếng Việt",
                    MoTa = "Cuốn sách khái quát lịch sử phát triển nhân loại.",
                    AnhBia = "/images/book5.jpg"
                }
            };
        }

        private static List<SachDangMuonItem> TaoDanhSachSachDangMuon()
        {
            return new List<SachDangMuonItem>
            {
                new SachDangMuonItem
                {
                    MaSach = 6,
                    TenSach = "Dám bị ghét",
                    AnhBia = "/images/book6.jpg",
                    NgayTra = DateTime.Today.AddDays(7),
                    SoNgayConLai = 7
                },

                new SachDangMuonItem
                {
                    MaSach = 7,
                    TenSach = "Người giàu có nhất thành Babylon",
                    AnhBia = "/images/book7.jpg",
                    NgayTra = DateTime.Today.AddDays(10),
                    SoNgayConLai = 10
                },

                new SachDangMuonItem
                {
                    MaSach = 8,
                    TenSach = "Chiến binh cầu vồng",
                    AnhBia = "/images/book8.jpg",
                    NgayTra = DateTime.Today.AddDays(14),
                    SoNgayConLai = 14
                }
            };
        }

        private static List<TheLoaiItem> TaoDanhSachTheLoai()
        {
            return new List<TheLoaiItem>
            {
                new TheLoaiItem
                {
                    MaTheLoai = 1,
                    TenTheLoai = "Văn học",
                    SoLuongSach = 1234,
                    Icon = "bi-book",
                    LopMau = "category-green"
                },

                new TheLoaiItem
                {
                    MaTheLoai = 2,
                    TenTheLoai = "Khoa học",
                    SoLuongSach = 987,
                    Icon = "bi-lightbulb",
                    LopMau = "category-purple"
                },

                new TheLoaiItem
                {
                    MaTheLoai = 3,
                    TenTheLoai = "Kinh tế",
                    SoLuongSach = 756,
                    Icon = "bi-graph-up-arrow",
                    LopMau = "category-orange"
                },

                new TheLoaiItem
                {
                    MaTheLoai = 4,
                    TenTheLoai = "Thiếu nhi",
                    SoLuongSach = 632,
                    Icon = "bi-balloon-heart",
                    LopMau = "category-pink"
                },

                new TheLoaiItem
                {
                    MaTheLoai = 5,
                    TenTheLoai = "Lịch sử",
                    SoLuongSach = 543,
                    Icon = "bi-bank",
                    LopMau = "category-blue"
                }
            };
        }

        private static List<ThongBaoItem> TaoDanhSachThongBao()
        {
            return new List<ThongBaoItem>
            {
                new ThongBaoItem
                {
                    NoiDung = "Sách “Nhà giả kim” của bạn sắp đến hạn trả.",
                    NgayThongBao = DateTime.Today,
                    Loai = "warning"
                },

                new ThongBaoItem
                {
                    NoiDung = "Bạn đã đặt chỗ sách “Đắc nhân tâm” thành công.",
                    NgayThongBao = DateTime.Today.AddDays(-1),
                    Loai = "success"
                },

                new ThongBaoItem
                {
                    NoiDung = "Thư viện thông báo lịch nghỉ cuối tuần.",
                    NgayThongBao = DateTime.Today.AddDays(-2),
                    Loai = "info"
                }
            };
        }
    }
}

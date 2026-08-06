using bài_tập_1.Data;
using bài_tập_1.Models;
using bài_tập_1.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace bài_tập_1.Controllers
{
    public class HomeController : Controller
    {
        private readonly bài_tập_1Context _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(
            bài_tập_1Context context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            // Admin/Nhân viên luôn sử dụng Dashboard làm trang chủ.
            // Guard này cũng xử lý trường hợp truy cập trực tiếp URL gốc "/".
            if (User.IsInRole("Admin") || User.IsInRole("NhanVien"))
                return RedirectToAction("Index", "Dashboard");

            const int homePageSize = 8;
            var bookQuery = _context.Sach
                .Include(s => s.BanSaos)
                .AsNoTracking();
            var pagination = Pagination.Create(
                page,
                await bookQuery.CountAsync(),
                homePageSize);
            ViewData["Pagination"] = pagination;
            ViewData["PaginationFragment"] = "sach-moi";

            var model = new TrangChuViewModel
            {
                SachMoi = await bookQuery
                    .OrderByDescending(s => s.MaSach)
                    .Skip((pagination.Page - 1) * pagination.PageSize)
                    .Take(pagination.PageSize)
                    .ToListAsync(),

                TheLoaiPhoBien = await _context.TheLoai
                    .AsNoTracking()
                    .OrderByDescending(t => t.DanhSachSach.Count)
                    .Take(5)
                    .Select(t => new TheLoaiItem
                    {
                        MaTheLoai = t.MaTheLoai,
                        TenTheLoai = t.TenTheLoai,
                        SoLuongSach = t.DanhSachSach.Count
                    })
                    .ToListAsync()
            };

            ApplyCategoryPresentation(model.TheLoaiPhoBien);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewData["FavoriteIds"] = string.IsNullOrWhiteSpace(userId)
                ? new HashSet<int>()
                : (await _context.YeuThich
                    .Where(y => y.DocGia.UserId == userId)
                    .Select(y => y.MaSach)
                    .ToListAsync())
                    .ToHashSet();

            if (User.IsInRole("DocGia"))
            {
                await LoadReaderDataAsync(model);
            }

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Terms()
        {
            return View();
        }

        private async Task LoadReaderDataAsync(TrangChuViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var docGia = await _context.DocGia
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (docGia == null)
                return;

            var borrowedItems = await _context.ChiTietPhieuMuon
                .Include(ct => ct.PhieuMuon)
                .Include(ct => ct.BanSao)
                    .ThenInclude(b => b.Sach)
                .AsNoTracking()
                .Where(ct =>
                    ct.PhieuMuon.MaDocGia == docGia.MaDocGia &&
                    ct.NgayTra == null)
                .OrderBy(ct => ct.PhieuMuon.NgayHenTra)
                .Take(6)
                .ToListAsync();

            model.SachDangMuon = borrowedItems.Select(ct =>
            {
                var dueDate = ct.PhieuMuon.NgayHenTra;
                return new SachDangMuonItem
                {
                    MaSach = ct.BanSao.MaSach,
                    TenSach = ct.BanSao.Sach.TenSach,
                    AnhBia = ct.BanSao.Sach.AnhBia,
                    NgayTra = dueDate,
                    SoNgayConLai = (dueDate.Date - DateTime.Today).Days
                };
            }).ToList();

            var dauNam = new DateTime(DateTime.Today.Year, 1, 1);
            var dauNamSau = dauNam.AddYears(1);
            var chiTietTrongNam = _context.ChiTietPhieuMuon
                .Where(c =>
                    c.PhieuMuon.MaDocGia == docGia.MaDocGia &&
                    c.PhieuMuon.NgayMuon >= dauNam &&
                    c.PhieuMuon.NgayMuon < dauNamSau);

            model.ThongKeCaNhan = new ThongKeCaNhanItem
            {
                Nam = dauNam.Year,
                SoSachDaMuonTrongNam = await chiTietTrongNam.CountAsync(),
                SoSachDangMuon = await _context.ChiTietPhieuMuon.CountAsync(c =>
                    c.PhieuMuon.MaDocGia == docGia.MaDocGia && c.NgayTra == null),
                SoSachTraDungHan = await chiTietTrongNam.CountAsync(c =>
                    c.NgayTra != null &&
                    c.NgayTra.Value.Date <= c.PhieuMuon.NgayHenTra.Date),
                SoSachYeuThich = await _context.YeuThich.CountAsync(y =>
                    y.MaDocGia == docGia.MaDocGia)
            };
        }

        private static void ApplyCategoryPresentation(
            IReadOnlyList<TheLoaiItem> categories)
        {
            var colors = new[]
            {
                "category-green",
                "category-purple",
                "category-orange",
                "category-pink",
                "category-blue"
            };
            var icons = new[]
            {
                "bi-book",
                "bi-lightbulb",
                "bi-graph-up-arrow",
                "bi-balloon-heart",
                "bi-bank"
            };

            for (var index = 0; index < categories.Count; index++)
            {
                categories[index].LopMau = colors[index % colors.Length];
                categories[index].Icon = icons[index % icons.Length];
                var tenTheLoai = categories[index].TenTheLoai.ToLowerInvariant();
                categories[index].AnhDaiDien = tenTheLoai switch
                {
                    var name when name.Contains("nước ngoài") => "/images/categories/van-hoc-nuoc-ngoai.webp",
                    var name when name.Contains("việt nam") => "/images/categories/van-hoc-viet-nam.webp",
                    var name when name.Contains("công nghệ") || name.Contains("tin học") => "/images/categories/cong-nghe-thong-tin.webp",
                    var name when name.Contains("khoa học") => "/images/categories/khoa-hoc-tu-nhien.webp",
                    var name when name.Contains("kinh tế") || name.Contains("quản trị") => "/images/categories/kinh-te-quan-tri.webp",
                    _ => "/images/categories/van-hoc-nuoc-ngoai.webp"
                };
            }
        }
    }
}

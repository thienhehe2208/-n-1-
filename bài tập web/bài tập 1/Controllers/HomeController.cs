using bài_tập_1.Data;
using bài_tập_1.Models;
using bài_tập_1.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using bài_tập_1.Services;

namespace bài_tập_1.Controllers
{
    public class HomeController : Controller
    {
        private readonly bài_tập_1Context _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ThongBaoService _thongBaoService;

        public HomeController(
            bài_tập_1Context context,
            UserManager<IdentityUser> userManager,
            ThongBaoService thongBaoService)
        {
            _context = context;
            _userManager = userManager;
            _thongBaoService = thongBaoService;
        }

        public async Task<IActionResult> Index()
        {
            // Admin/Nhân viên luôn sử dụng Dashboard làm trang chủ.
            // Guard này cũng xử lý trường hợp truy cập trực tiếp URL gốc "/".
            if (User.IsInRole("Admin") || User.IsInRole("NhanVien"))
                return RedirectToAction("Index", "Dashboard");

            var model = new TrangChuViewModel
            {
                SachMoi = await _context.Sach
                    .Include(s => s.BanSaos)
                    .AsNoTracking()
                    .OrderByDescending(s => s.MaSach)
                    .Take(12)
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

            await _thongBaoService.DongBoChoDocGiaAsync(docGia.MaDocGia);

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

            model.ThongBao = await _context.ThongBao
                .AsNoTracking()
                .Where(t => t.MaDocGia == docGia.MaDocGia)
                .OrderBy(t => t.DaDoc)
                .ThenByDescending(t => t.NgayTao)
                .Take(5)
                .Select(t => new ThongBaoItem
                {
                    NoiDung = t.NoiDung,
                    NgayThongBao = t.NgayTao,
                    Loai = t.Loai
                })
                .ToListAsync();
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
            }
        }
    }
}

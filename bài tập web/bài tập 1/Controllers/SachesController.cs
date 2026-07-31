using bài_tập_1.Data;
using bài_tập_1.Models;
using bài_tập_1.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace bài_tập_1.Controllers
{
    public class SachesController : Controller
    {
        private readonly bài_tập_1Context _context;

        public SachesController(bài_tập_1Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? q, int? maTheLoai, int page = 1)
        {
            var query = _context.Sach
                .Include(s => s.NhaXuatBan)
                .Include(s => s.TheLoai)
                .Include(s => s.BanSaos)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var keyword = q.Trim();
                query = query.Where(s =>
                    s.TenSach.Contains(keyword) ||
                    s.ISBN.Contains(keyword));
            }

            if (maTheLoai.HasValue)
                query = query.Where(s => s.MaTheLoai == maTheLoai.Value);

            ViewData["Search"] = q;
            ViewData["MaTheLoai"] = maTheLoai;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewData["FavoriteIds"] = string.IsNullOrWhiteSpace(userId)
                ? new HashSet<int>()
                : (await _context.YeuThich
                    .Where(y => y.DocGia.UserId == userId)
                    .Select(y => y.MaSach)
                    .ToListAsync())
                    .ToHashSet();
            var pagination = Pagination.Create(page, await query.CountAsync());
            ViewData["Pagination"] = pagination;
            return View(await query.OrderBy(s => s.TenSach)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var sach = await _context.Sach
                .Include(s => s.NhaXuatBan)
                .Include(s => s.TheLoai)
                .Include(s => s.SachTacGias)
                    .ThenInclude(st => st.TacGia)
                .Include(s => s.BanSaos)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.MaSach == id);

            return sach == null ? NotFound() : View(sach);
        }

        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Create()
        {
            await LoadSelectionsAsync();
            return View(new SachFormViewModel
            {
                NgonNgu = "Tiếng Việt"
            });
        }

        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SachFormViewModel model)
        {
            await ValidateAuthorsAsync(model);
            if (!ModelState.IsValid)
            {
                await LoadSelectionsAsync(
                    model.MaTheLoai,
                    model.MaNXB,
                    model.TacGiaIds);
                return View(model);
            }

            var sach = new Sach();
            ApplyForm(sach, model);
            foreach (var tacGiaId in model.TacGiaIds.Distinct())
            {
                sach.SachTacGias.Add(new SachTacGia
                {
                    MaTacGia = tacGiaId
                });
            }

            _context.Sach.Add(sach);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã thêm sách và danh sách tác giả.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var sach = await _context.Sach
                .Include(s => s.SachTacGias)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.MaSach == id);
            if (sach == null)
                return NotFound();

            var model = new SachFormViewModel
            {
                MaSach = sach.MaSach,
                TenSach = sach.TenSach,
                ISBN = sach.ISBN,
                GiaSach = sach.GiaSach,
                MaTheLoai = sach.MaTheLoai,
                MaNXB = sach.MaNXB,
                NamXuatBan = sach.NamXuatBan,
                SoTrang = sach.SoTrang,
                NgonNgu = sach.NgonNgu,
                MoTa = sach.MoTa,
                AnhBia = sach.AnhBia,
                TacGiaIds = sach.SachTacGias
                    .Select(st => st.MaTacGia)
                    .ToList()
            };

            await LoadSelectionsAsync(
                model.MaTheLoai,
                model.MaNXB,
                model.TacGiaIds);
            return View(model);
        }

        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            SachFormViewModel model)
        {
            if (id != model.MaSach)
                return NotFound();

            await ValidateAuthorsAsync(model);
            if (!ModelState.IsValid)
            {
                await LoadSelectionsAsync(
                    model.MaTheLoai,
                    model.MaNXB,
                    model.TacGiaIds);
                return View(model);
            }

            var sach = await _context.Sach
                .Include(s => s.SachTacGias)
                .FirstOrDefaultAsync(s => s.MaSach == id);
            if (sach == null)
                return NotFound();

            ApplyForm(sach, model);

            var selectedIds = model.TacGiaIds.Distinct().ToHashSet();
            var removed = sach.SachTacGias
                .Where(st => !selectedIds.Contains(st.MaTacGia))
                .ToList();
            _context.SachTacGias.RemoveRange(removed);

            var existingIds = sach.SachTacGias
                .Select(st => st.MaTacGia)
                .ToHashSet();
            foreach (var tacGiaId in selectedIds.Except(existingIds))
            {
                sach.SachTacGias.Add(new SachTacGia
                {
                    MaTacGia = tacGiaId
                });
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] =
                    "Đã cập nhật sách và danh sách tác giả.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Sach.AnyAsync(s => s.MaSach == id))
                    return NotFound();
                throw;
            }
        }

        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var sach = await _context.Sach
                .Include(s => s.NhaXuatBan)
                .Include(s => s.TheLoai)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.MaSach == id);

            return sach == null ? NotFound() : View(sach);
        }

        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sach = await _context.Sach.FindAsync(id);
            if (sach == null)
                return NotFound();

            try
            {
                _context.Sach.Remove(sach);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa sách.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Không thể xóa sách đang có bản sao, đặt trước hoặc lịch sử mượn.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadSelectionsAsync(
            int? maTheLoai = null,
            int? maNxb = null,
            IEnumerable<int>? tacGiaIds = null)
        {
            ViewData["MaTheLoai"] = new SelectList(
                await _context.TheLoai
                    .AsNoTracking()
                    .OrderBy(t => t.TenTheLoai)
                    .ToListAsync(),
                "MaTheLoai",
                "TenTheLoai",
                maTheLoai);

            ViewData["MaNXB"] = new SelectList(
                await _context.NhaXuatBan
                    .AsNoTracking()
                    .OrderBy(n => n.TenNXB)
                    .ToListAsync(),
                "MaNXB",
                "TenNXB",
                maNxb);

            ViewData["TacGiaIds"] = new MultiSelectList(
                await _context.TacGia
                    .AsNoTracking()
                    .OrderBy(t => t.HoTen)
                    .ToListAsync(),
                "MaTacGia",
                "HoTen",
                tacGiaIds);
        }

        private async Task ValidateAuthorsAsync(SachFormViewModel model)
        {
            var selectedIds = model.TacGiaIds.Distinct().ToList();
            if (selectedIds.Count == 0)
            {
                ModelState.AddModelError(
                    nameof(model.TacGiaIds),
                    "Vui lòng chọn ít nhất một tác giả.");
                return;
            }

            var existingCount = await _context.TacGia
                .CountAsync(t => selectedIds.Contains(t.MaTacGia));
            if (existingCount != selectedIds.Count)
            {
                ModelState.AddModelError(
                    nameof(model.TacGiaIds),
                    "Danh sách tác giả không hợp lệ.");
            }
        }

        private static void ApplyForm(Sach sach, SachFormViewModel model)
        {
            sach.TenSach = model.TenSach.Trim();
            sach.ISBN = model.ISBN.Trim();
            sach.GiaSach = model.GiaSach;
            sach.MaTheLoai = model.MaTheLoai;
            sach.MaNXB = model.MaNXB;
            sach.NamXuatBan = model.NamXuatBan;
            sach.SoTrang = model.SoTrang;
            sach.NgonNgu = model.NgonNgu.Trim();
            sach.MoTa = model.MoTa.Trim();
            sach.AnhBia = model.AnhBia.Trim();
        }
    }
}

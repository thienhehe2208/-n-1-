using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using bài_tập_1.Data;
using bài_tập_1.Models;
using bài_tập_1.Models.ViewModels;

namespace bài_tập_1.Controllers
{
    // Chỉ cần đăng nhập là được vào Controller (không khóa ở class),
    // vì Create cho phép cả DocGia, còn các action khác chỉ Admin/NhanVien
    [Authorize]
    public class DatTruocsController : Controller
    {
        private readonly bài_tập_1Context _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DatTruocsController(bài_tập_1Context context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Danh sách toàn bộ đặt trước - chỉ Admin/NhanVien duyệt
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Index()
        {
            var bài_tập_1Context = _context.DatTruoc.Include(d => d.DocGia).Include(d => d.Sach);
            return View(await bài_tập_1Context.ToListAsync());
        }

        // Nhân viên xem mọi yêu cầu; độc giả chỉ xem yêu cầu của chính mình.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.DatTruoc == null)
            {
                return NotFound();
            }

            var datTruoc = await _context.DatTruoc
                .Include(d => d.DocGia)
                .Include(d => d.Sach)
                    .ThenInclude(s => s.TheLoai)
                .Include(d => d.Sach)
                    .ThenInclude(s => s.NhaXuatBan)
                .FirstOrDefaultAsync(m => m.MaDatTruoc == id);
            if (datTruoc == null)
            {
                return NotFound();
            }

            if (!(User.IsInRole("Admin") || User.IsInRole("NhanVien")))
            {
                var userId = _userManager.GetUserId(User);
                if (datTruoc.DocGia.UserId != userId)
                    return Forbid();
            }

            return View(datTruoc);
        }

        // Trang xác nhận đặt trước. maSach được truyền từ nút Đặt trước của cuốn sách.
        [Authorize(Roles = "Admin,NhanVien,DocGia")]
        public async Task<IActionResult> Create(int? maSach)
        {
            if (maSach == null)
                return RedirectToAction("Index", "Saches");

            var model = await TaoDatTruocViewModelAsync(maSach.Value);
            if (model == null)
                return NotFound();

            return View(model);
        }

        // Lưu yêu cầu đặt trước sau khi người dùng xác nhận.
        [Authorize(Roles = "Admin,NhanVien,DocGia")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DatTruocViewModel input)
        {
            var isStaff = User.IsInRole("Admin") || User.IsInRole("NhanVien");
            DocGia? docGia;

            if (isStaff)
            {
                docGia = input.MaDocGia.HasValue
                    ? await _context.DocGia.FindAsync(input.MaDocGia.Value)
                    : null;
            }
            else
            {
                var userId = _userManager.GetUserId(User);
                docGia = await _context.DocGia.FirstOrDefaultAsync(d => d.UserId == userId);
            }

            if (docGia == null)
                ModelState.AddModelError(string.Empty, "Không tìm thấy hồ sơ độc giả hợp lệ.");
            else if (docGia.TrangThai != TrangThaiDocGia.HoatDong)
                ModelState.AddModelError(string.Empty, "Thẻ độc giả đang bị khóa.");
            else if (docGia.NgayHetHanThe < DateTime.Today)
                ModelState.AddModelError(string.Empty, "Thẻ độc giả đã hết hạn.");

            var sachTonTai = await _context.Sach.AnyAsync(s => s.MaSach == input.MaSach);
            if (!sachTonTai)
                ModelState.AddModelError(string.Empty, "Cuốn sách không tồn tại.");

            if (docGia != null)
            {
                var daDatTrung = await _context.DatTruoc.AnyAsync(d =>
                    d.MaDocGia == docGia.MaDocGia &&
                    d.MaSach == input.MaSach &&
                    (d.TrangThai == TrangThaiDatTruoc.DangCho ||
                     d.TrangThai == TrangThaiDatTruoc.DaCoSach));

                if (daDatTrung)
                    ModelState.AddModelError(string.Empty, "Bạn đã có một yêu cầu đang hoạt động cho cuốn sách này.");
            }

            if (!ModelState.IsValid)
            {
                var invalidModel = await TaoDatTruocViewModelAsync(input.MaSach, input.MaDocGia);
                if (invalidModel == null)
                    return NotFound();

                invalidModel.DongYQuyDinh = input.DongYQuyDinh;
                return View(invalidModel);
            }

            var datTruoc = new DatTruoc
            {
                MaDocGia = docGia!.MaDocGia,
                MaSach = input.MaSach,
                NgayDat = DateTime.Now,
                NgayHetHanDat = DateTime.Now.AddDays(7),
                TrangThai = TrangThaiDatTruoc.DangCho
            };

            _context.DatTruoc.Add(datTruoc);
            await _context.SaveChangesAsync();

            TempData["DatTruocSuccess"] = "Yêu cầu đặt trước đã được ghi nhận.";
            return RedirectToAction(nameof(Details), new { id = datTruoc.MaDatTruoc });
        }

        // Danh sách đặt trước của độc giả đang đăng nhập.
        [Authorize(Roles = "DocGia")]
        public async Task<IActionResult> CuaToi()
        {
            var userId = _userManager.GetUserId(User);
            var docGia = await _context.DocGia.FirstOrDefaultAsync(d => d.UserId == userId);
            if (docGia == null)
                return NotFound();

            var danhSach = await _context.DatTruoc
                .Include(d => d.Sach)
                .Where(d => d.MaDocGia == docGia.MaDocGia)
                .OrderByDescending(d => d.NgayDat)
                .AsNoTracking()
                .ToListAsync();

            return View(danhSach);
        }

        // Độc giả được hủy khi yêu cầu vẫn đang chờ; nhân viên có thể hủy giúp.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(int id)
        {
            var datTruoc = await _context.DatTruoc
                .Include(d => d.DocGia)
                .FirstOrDefaultAsync(d => d.MaDatTruoc == id);

            if (datTruoc == null)
                return NotFound();

            var isStaff = User.IsInRole("Admin") || User.IsInRole("NhanVien");
            if (!isStaff && datTruoc.DocGia.UserId != _userManager.GetUserId(User))
                return Forbid();

            if (datTruoc.TrangThai != TrangThaiDatTruoc.DangCho)
            {
                TempData["DatTruocError"] = "Chỉ yêu cầu đang chờ mới có thể hủy.";
                return RedirectToAction(nameof(Details), new { id });
            }

            datTruoc.TrangThai = TrangThaiDatTruoc.DaHuy;
            await _context.SaveChangesAsync();

            TempData["DatTruocSuccess"] = "Đã hủy yêu cầu đặt trước.";
            return isStaff
                ? RedirectToAction(nameof(Index))
                : RedirectToAction(nameof(CuaToi));
        }

        // Hiển thị form sửa đặt trước - chỉ Admin/NhanVien (cập nhật trạng thái)
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.DatTruoc == null)
            {
                return NotFound();
            }

            var datTruoc = await _context.DatTruoc.FindAsync(id);
            if (datTruoc == null)
            {
                return NotFound();
            }
            ViewData["MaDocGia"] = new SelectList(_context.DocGia, "MaDocGia", "HoTen", datTruoc.MaDocGia);
            ViewData["MaSach"] = new SelectList(_context.Sach, "MaSach", "TenSach", datTruoc.MaSach);
            return View(datTruoc);
        }

        // Xử lý cập nhật đặt trước - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDatTruoc,MaDocGia,MaSach,NgayDat,NgayHetHanDat,TrangThai")] DatTruoc datTruoc)
        {
            if (id != datTruoc.MaDatTruoc)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(datTruoc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DatTruocExists(datTruoc.MaDatTruoc))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaDocGia"] = new SelectList(_context.DocGia, "MaDocGia", "HoTen", datTruoc.MaDocGia);
            ViewData["MaSach"] = new SelectList(_context.Sach, "MaSach", "TenSach", datTruoc.MaSach);
            return View(datTruoc);
        }

        // Hiển thị xác nhận xóa đặt trước - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.DatTruoc == null)
            {
                return NotFound();
            }

            var datTruoc = await _context.DatTruoc
                .Include(d => d.DocGia)
                .Include(d => d.Sach)
                .FirstOrDefaultAsync(m => m.MaDatTruoc == id);
            if (datTruoc == null)
            {
                return NotFound();
            }

            return View(datTruoc);
        }

        // Xử lý xóa đặt trước - chỉ Admin/NhanVien
        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.DatTruoc == null)
            {
                return Problem("Entity set 'bài_tập_1Context.DatTruoc'  is null.");
            }
            var datTruoc = await _context.DatTruoc.FindAsync(id);
            if (datTruoc != null)
            {
                _context.DatTruoc.Remove(datTruoc);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<DatTruocViewModel?> TaoDatTruocViewModelAsync(
            int maSach,
            int? maDocGiaDuocChon = null)
        {
            var sach = await _context.Sach
                .Include(s => s.TheLoai)
                .Include(s => s.NhaXuatBan)
                .Include(s => s.SachTacGias)
                    .ThenInclude(st => st.TacGia)
                .Include(s => s.BanSaos)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.MaSach == maSach);

            if (sach == null)
                return null;

            var isStaff = User.IsInRole("Admin") || User.IsInRole("NhanVien");
            DocGia? docGia = null;

            if (isStaff)
            {
                var docGias = await _context.DocGia
                    .Where(d => d.TrangThai == TrangThaiDocGia.HoatDong)
                    .OrderBy(d => d.HoTen)
                    .AsNoTracking()
                    .ToListAsync();

                ViewData["MaDocGia"] = new SelectList(
                    docGias, "MaDocGia", "HoTen", maDocGiaDuocChon);

                if (maDocGiaDuocChon.HasValue)
                    docGia = docGias.FirstOrDefault(d => d.MaDocGia == maDocGiaDuocChon);
            }
            else
            {
                var userId = _userManager.GetUserId(User);
                docGia = await _context.DocGia
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.UserId == userId);
            }

            var soNguoiDangCho = await _context.DatTruoc.CountAsync(d =>
                d.MaSach == maSach &&
                (d.TrangThai == TrangThaiDatTruoc.DangCho ||
                 d.TrangThai == TrangThaiDatTruoc.DaCoSach));

            return new DatTruocViewModel
            {
                MaSach = sach.MaSach,
                MaDocGia = maDocGiaDuocChon ?? docGia?.MaDocGia,
                Sach = sach,
                DocGia = docGia,
                IsStaff = isStaff,
                TongBanSao = sach.BanSaos.Count,
                SoBanSanCo = sach.BanSaos.Count(b => b.TinhTrang == TinhTrangBanSao.SanCo),
                SoNguoiDangCho = soNguoiDangCho,
                NgayHetHanDuKien = DateTime.Now.AddDays(7)
            };
        }

        private bool DatTruocExists(int id)
        {
            return (_context.DatTruoc?.Any(e => e.MaDatTruoc == id)).GetValueOrDefault();
        }
    }
}

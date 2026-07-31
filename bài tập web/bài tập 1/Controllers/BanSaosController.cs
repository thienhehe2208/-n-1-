using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using bài_tập_1.Data;
using bài_tập_1.Models;
using bài_tập_1.Services;
using bài_tập_1.Models.ViewModels;

namespace bài_tập_1.Controllers
{
    // Quản lý bản sao (mã vạch, vị trí kệ, tình trạng) là dữ liệu vận hành nội bộ,
    // không phải thông tin cho độc giả tra cứu -> khóa toàn bộ Controller
    [Authorize(Roles = "Admin,NhanVien")]
    public class BanSaosController : Controller
    {
        private readonly bài_tập_1Context _context;
        private readonly DatTruocService _datTruocService;

        public BanSaosController(
            bài_tập_1Context context,
            DatTruocService datTruocService)
        {
            _context = context;
            _datTruocService = datTruocService;
        }

        // Danh sách bản sao
        public async Task<IActionResult> Index(string? q, string? tinhTrang, int page = 1)
        {
            var source = _context.BanSao.AsNoTracking();
            ViewData["TongBanSao"] = await source.CountAsync();
            ViewData["SanCo"] = await source.CountAsync(b => b.TinhTrang == TinhTrangBanSao.SanCo);
            ViewData["DangMuon"] = await source.CountAsync(b => b.TinhTrang == TinhTrangBanSao.DangMuon);
            ViewData["HuHong"] = await source.CountAsync(b => b.TinhTrang == TinhTrangBanSao.HuHong);
            ViewData["ThanhLy"] = await source.CountAsync(b => b.TinhTrang == TinhTrangBanSao.ThanhLy);
            ViewData["Mat"] = await source.CountAsync(b => b.TinhTrang == TinhTrangBanSao.Mat);
            ViewData["DaGiu"] = await source.CountAsync(b => b.TinhTrang == TinhTrangBanSao.DaGiu);

            var query = source.Include(b => b.Sach).AsQueryable();
            q = q?.Trim();
            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(b => b.MaVach.Contains(q) || b.Sach.TenSach.Contains(q) || b.ViTriKe.Contains(q));
            }

            query = tinhTrang switch
            {
                "available" => query.Where(b => b.TinhTrang == TinhTrangBanSao.SanCo),
                "borrowed" => query.Where(b => b.TinhTrang == TinhTrangBanSao.DangMuon),
                "damaged" => query.Where(b => b.TinhTrang == TinhTrangBanSao.HuHong),
                "liquidated" => query.Where(b => b.TinhTrang == TinhTrangBanSao.ThanhLy),
                "lost" => query.Where(b => b.TinhTrang == TinhTrangBanSao.Mat),
                "reserved" => query.Where(b => b.TinhTrang == TinhTrangBanSao.DaGiu),
                _ => query
            };

            ViewData["TuKhoa"] = q;
            ViewData["TinhTrang"] = tinhTrang;
            var pagination = Pagination.Create(page, await query.CountAsync());
            ViewData["Pagination"] = pagination;
            return View(await query.OrderBy(b => b.Sach.TenSach)
                .ThenBy(b => b.MaVach)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync());
        }

        // Xem chi tiết 1 bản sao
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.BanSao == null)
            {
                return NotFound();
            }

            var banSao = await _context.BanSao
                .Include(b => b.Sach)
                .FirstOrDefaultAsync(m => m.MaBanSao == id);
            if (banSao == null)
            {
                return NotFound();
            }

            return View(banSao);
        }

        // Hiển thị form thêm bản sao
        public IActionResult Create()
        {
            ViewData["MaSach"] = new SelectList(_context.Sach, "MaSach", "TenSach");
            return View();
        }

        // Xử lý lưu bản sao mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaSach,MaVach,ViTriKe")] BanSao banSao)
        {
            banSao.MaVach = banSao.MaVach?.Trim() ?? string.Empty;
            banSao.TinhTrang = TinhTrangBanSao.SanCo;
            if (await _context.BanSao.AnyAsync(
                    b => b.MaVach == banSao.MaVach))
            {
                ModelState.AddModelError(nameof(banSao.MaVach),
                    "Mã vạch này đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(banSao);
                await _context.SaveChangesAsync();
                await _datTruocService.PhanBoBanSaoAsync(banSao);
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaSach"] = new SelectList(_context.Sach, "MaSach", "TenSach", banSao.MaSach);
            return View(banSao);
        }

        // Hiển thị form sửa bản sao
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.BanSao == null)
            {
                return NotFound();
            }

            var banSao = await _context.BanSao.FindAsync(id);
            if (banSao == null)
            {
                return NotFound();
            }
            ViewData["MaSach"] = new SelectList(_context.Sach, "MaSach", "TenSach", banSao.MaSach);
            return View(banSao);
        }

        // Xử lý cập nhật bản sao
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaBanSao,MaSach,MaVach,TinhTrang,ViTriKe")] BanSao banSao)
        {
            if (id != banSao.MaBanSao)
            {
                return NotFound();
            }

            banSao.MaVach = banSao.MaVach?.Trim() ?? string.Empty;
            if (await _context.BanSao.AnyAsync(
                    b => b.MaVach == banSao.MaVach &&
                         b.MaBanSao != id))
            {
                ModelState.AddModelError(nameof(banSao.MaVach),
                    "Mã vạch này đã tồn tại.");
            }

            var banSaoHienTai = await _context.BanSao
                .FirstOrDefaultAsync(b => b.MaBanSao == id);
            if (banSaoHienTai == null)
                return NotFound();

            var dangCoLuotMuon = await _context.ChiTietPhieuMuon
                .AnyAsync(c =>
                    c.MaBanSao == id &&
                    c.NgayTra == null);
            var dangDuocGiu = await _context.DatTruoc
                .AnyAsync(d =>
                    d.MaBanSaoDuocGiu == id &&
                    d.TrangThai == TrangThaiDatTruoc.DaCoSach);

            if (dangCoLuotMuon)
            {
                if (banSao.TinhTrang != TinhTrangBanSao.DangMuon)
                {
                    ModelState.AddModelError(
                        nameof(banSao.TinhTrang),
                        "Bản sao đang được mượn nên không thể đổi trạng thái.");
                }

                if (banSao.MaSach != banSaoHienTai.MaSach)
                {
                    ModelState.AddModelError(
                        nameof(banSao.MaSach),
                        "Không thể đổi đầu sách của bản sao đang được mượn.");
                }
            }
            else if (dangDuocGiu)
            {
                if (banSao.TinhTrang != TinhTrangBanSao.DaGiu)
                {
                    ModelState.AddModelError(
                        nameof(banSao.TinhTrang),
                        "Bản sao đang được giữ cho một độc giả nên không thể đổi trạng thái.");
                }

                if (banSao.MaSach != banSaoHienTai.MaSach)
                {
                    ModelState.AddModelError(
                        nameof(banSao.MaSach),
                        "Không thể đổi đầu sách của bản sao đang được giữ.");
                }
            }
            else if (banSao.TinhTrang == TinhTrangBanSao.DangMuon ||
                     banSao.TinhTrang == TinhTrangBanSao.DaGiu)
            {
                ModelState.AddModelError(
                    nameof(banSao.TinhTrang),
                    "Trạng thái đang mượn/đang giữ chỉ được thiết lập bởi nghiệp vụ mượn và đặt trước.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    banSaoHienTai.MaSach = banSao.MaSach;
                    banSaoHienTai.MaVach = banSao.MaVach;
                    banSaoHienTai.TinhTrang = banSao.TinhTrang;
                    banSaoHienTai.ViTriKe =
                        banSao.ViTriKe?.Trim() ?? string.Empty;
                    await _context.SaveChangesAsync();

                    if (banSaoHienTai.TinhTrang ==
                        TinhTrangBanSao.SanCo)
                    {
                        await _datTruocService.PhanBoBanSaoAsync(
                            banSaoHienTai);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BanSaoExists(banSao.MaBanSao))
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
            ViewData["MaSach"] = new SelectList(_context.Sach, "MaSach", "TenSach", banSao.MaSach);
            return View(banSao);
        }

        // Hiển thị xác nhận xóa bản sao
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.BanSao == null)
            {
                return NotFound();
            }

            var banSao = await _context.BanSao
                .Include(b => b.Sach)
                .FirstOrDefaultAsync(m => m.MaBanSao == id);
            if (banSao == null)
            {
                return NotFound();
            }

            return View(banSao);
        }

        // Xử lý xóa bản sao
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var banSao = await _context.BanSao.FindAsync(id);
            if (banSao == null)
                return NotFound();

            var daCoLichSuMuon = await _context.ChiTietPhieuMuon
                .AnyAsync(c => c.MaBanSao == id);
            var daCoDatTruoc = await _context.DatTruoc
                .AnyAsync(d => d.MaBanSaoDuocGiu == id);

            if (daCoLichSuMuon || daCoDatTruoc)
            {
                TempData["Error"] =
                    "Không thể xóa bản sao đã có lịch sử mượn hoặc đặt trước. " +
                    "Hãy chuyển sang trạng thái thanh lý nếu bản sao không còn sử dụng.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.BanSao.Remove(banSao);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa bản sao chưa phát sinh giao dịch.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] =
                    "Không thể xóa bản sao vì đang có dữ liệu liên quan.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BanSaoExists(int id)
        {
            return (_context.BanSao?.Any(e => e.MaBanSao == id)).GetValueOrDefault();
        }
    }
}

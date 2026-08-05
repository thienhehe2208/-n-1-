using bài_tập_1.Data;
using bài_tập_1.Models;
using System.Data;
using bài_tập_1.Models.ViewModels;
using bài_tập_1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace bài_tập_1.Controllers
{
    [Authorize(Roles = "Admin,NhanVien")]
    public class PhieuMuonsController : Controller
    {
        private readonly bài_tập_1Context _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly PhieuMuonService _phieuMuonService;
        private readonly DocGiaEligibilityService _eligibilityService;
        private readonly DatTruocService _datTruocService;

        public PhieuMuonsController(
            bài_tập_1Context context,
            UserManager<IdentityUser> userManager,
            PhieuMuonService phieuMuonService,
            DocGiaEligibilityService eligibilityService,
            DatTruocService datTruocService)
        {
            _context = context;
            _userManager = userManager;
            _phieuMuonService = phieuMuonService;
            _eligibilityService = eligibilityService;
            _datTruocService = datTruocService;
        }

        public async Task<IActionResult> Index(
            string? q,
            string? trangThai,
            int page = 1)
        {
            await _phieuMuonService.CapNhatTrangThaiAsync();
            var query = _context.PhieuMuon
                .Include(p => p.DocGia)
                .Include(p => p.NhanVien)
                .Include(p => p.ChiTietPhieuMuons)
                .AsNoTracking()
                .AsQueryable();

            ViewData["TongPhieu"] = await query.CountAsync();
            ViewData["DangMuon"] = await query.CountAsync(p =>
                p.TrangThai == TrangThaiPhieuMuon.DangMuon);
            ViewData["DaTra"] = await query.CountAsync(p =>
                p.TrangThai == TrangThaiPhieuMuon.DaTra);
            ViewData["QuaHan"] = await query.CountAsync(p =>
                p.TrangThai == TrangThaiPhieuMuon.QuaHan);
            ViewData["Nhap"] = await query.CountAsync(p =>
                p.TrangThai == TrangThaiPhieuMuon.Nhap);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var keyword = q.Trim();
                var isId = int.TryParse(
                    keyword.TrimStart('#'),
                    out var maPhieu);

                query = query.Where(p =>
                    p.DocGia.HoTen.Contains(keyword) ||
                    p.NhanVien.HoTen.Contains(keyword) ||
                    (isId && p.MaPhieuMuon == maPhieu));
            }

            query = trangThai switch
            {
                "borrowing" => query.Where(p =>
                    p.TrangThai == TrangThaiPhieuMuon.DangMuon),
                "returned" => query.Where(p =>
                    p.TrangThai == TrangThaiPhieuMuon.DaTra),
                "overdue" => query.Where(p =>
                    p.TrangThai == TrangThaiPhieuMuon.QuaHan),
                "draft" => query.Where(p =>
                    p.TrangThai == TrangThaiPhieuMuon.Nhap),
                _ => query
            };

            ViewData["Search"] = q;
            ViewData["Status"] = trangThai;

            var pagination = Pagination.Create(page, await query.CountAsync());
            ViewData["Pagination"] = pagination;
            return View(await query
                .OrderByDescending(p => p.NgayMuon)
                .ThenByDescending(p => p.MaPhieuMuon)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            await _phieuMuonService.CapNhatTrangThaiAsync(id.Value);

            var phieuMuon = await _context.PhieuMuon
                .Include(p => p.DocGia)
                .Include(p => p.NhanVien)
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(ct => ct.BanSao)
                        .ThenInclude(b => b.Sach)
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(ct => ct.PhieuPhat)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.MaPhieuMuon == id);

            return phieuMuon == null
                ? NotFound()
                : View(phieuMuon);
        }

        public async Task<IActionResult> Create()
        {
            await LoadDocGiasAsync();
            return View(new LapPhieuMuonViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            LapPhieuMuonViewModel model)
        {
            var docGia = await _context.DocGia
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MaDocGia == model.MaDocGia);

            ValidateDocGia(docGia);

            if (docGia != null)
            {
                var errors = await _eligibilityService.KiemTraAsync(
                    docGia.MaDocGia);
                foreach (var error in errors)
                    ModelState.AddModelError(nameof(model.MaDocGia), error);
            }

            if (model.NgayHenTra.Date <= DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(model.NgayHenTra),
                    "Ngày hẹn trả phải sau ngày lập phiếu.");
            }

            if (model.NgayHenTra.Date >
                DateTime.Today.AddDays(LibraryRules.SoNgayMuonToiDa))
            {
                ModelState.AddModelError(
                    nameof(model.NgayHenTra),
                    $"Thời hạn mượn không được vượt quá {LibraryRules.SoNgayMuonToiDa} ngày.");
            }

            var userId = _userManager.GetUserId(User);
            var nhanVien = await _context.NhanVien
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.UserId == userId);

            if (nhanVien == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Tài khoản hiện tại chưa có hồ sơ nhân viên.");
            }

            if (!ModelState.IsValid)
            {
                await LoadDocGiasAsync(model.MaDocGia);
                return View(model);
            }

            var phieuMuon = new PhieuMuon
            {
                MaDocGia = docGia!.MaDocGia,
                MaNhanVien = nhanVien!.MaNhanVien,
                NgayMuon = DateTime.Now,
                NgayHenTra = model.NgayHenTra.Date,
                TrangThai = TrangThaiPhieuMuon.Nhap
            };

            _context.PhieuMuon.Add(phieuMuon);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Đã lập phiếu. Hãy thêm các bản sao cần cho mượn.";

            return RedirectToAction(
                "Create",
                "ChiTietPhieuMuons",
                new { maPhieuMuon = phieuMuon.MaPhieuMuon });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var phieuMuon = await _context.PhieuMuon
                .Include(p => p.DocGia)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.MaPhieuMuon == id);

            if (phieuMuon == null)
                return NotFound();

            if (phieuMuon.TrangThai != TrangThaiPhieuMuon.Nhap)
            {
                TempData["Error"] =
                    "Chỉ được sửa hạn trả khi phiếu còn là bản nháp. " +
                    "Phiếu đã có sách phải sử dụng chức năng gia hạn.";
                return RedirectToAction(
                    nameof(Details),
                    new { id = phieuMuon.MaPhieuMuon });
            }

            return View(new CapNhatHanTraViewModel
            {
                MaPhieuMuon = phieuMuon.MaPhieuMuon,
                HoTenDocGia = phieuMuon.DocGia.HoTen,
                NgayMuon = phieuMuon.NgayMuon,
                NgayHenTra = phieuMuon.NgayHenTra
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            CapNhatHanTraViewModel model)
        {
            if (id != model.MaPhieuMuon)
                return NotFound();

            var phieuMuon = await _context.PhieuMuon
                .Include(p => p.DocGia)
                .Include(p => p.ChiTietPhieuMuons)
                .FirstOrDefaultAsync(p => p.MaPhieuMuon == id);

            if (phieuMuon == null)
                return NotFound();

            if (phieuMuon.TrangThai != TrangThaiPhieuMuon.Nhap)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Chỉ được sửa hạn trả của phiếu nháp.");
            }

            if (model.NgayHenTra.Date <= phieuMuon.NgayMuon.Date)
            {
                ModelState.AddModelError(
                    nameof(model.NgayHenTra),
                    "Ngày hẹn trả phải sau ngày mượn.");
            }

            if (model.NgayHenTra.Date >
                phieuMuon.NgayMuon.Date.AddDays(LibraryRules.SoNgayMuonToiDa))
            {
                ModelState.AddModelError(
                    nameof(model.NgayHenTra),
                    $"Thời hạn mượn ban đầu không được vượt quá {LibraryRules.SoNgayMuonToiDa} ngày.");
            }

            if (!ModelState.IsValid)
            {
                model.HoTenDocGia = phieuMuon.DocGia.HoTen;
                model.NgayMuon = phieuMuon.NgayMuon;
                return View(model);
            }

            phieuMuon.NgayHenTra = model.NgayHenTra.Date;
            phieuMuon.TrangThai = phieuMuon.ChiTietPhieuMuons.Count == 0
                ? TrangThaiPhieuMuon.Nhap
                : phieuMuon.NgayHenTra < DateTime.Today
                    ? TrangThaiPhieuMuon.QuaHan
                    : TrangThaiPhieuMuon.DangMuon;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật hạn trả.";

            return RedirectToAction(
                nameof(Details),
                new { id = phieuMuon.MaPhieuMuon });
        }

        // Quy tắc nghiệp vụ: trả sách là thao tác nguyên tử theo cả phiếu.
        // Không cập nhật NgayTra cho một ChiTietPhieuMuon riêng lẻ.
        public async Task<IActionResult> TraSach(int? id)
        {
            if (id == null)
                return NotFound();

            var phieuMuon = await LoadPhieuDeTraAsync(id.Value);
            if (phieuMuon == null)
                return NotFound();

            var sachChuaTra = phieuMuon.ChiTietPhieuMuons
                .Where(c => !c.NgayTra.HasValue)
                .ToList();

            if (phieuMuon.TrangThai == TrangThaiPhieuMuon.Nhap ||
                sachChuaTra.Count == 0)
            {
                TempData["Error"] = sachChuaTra.Count == 0
                    ? "Phiếu mượn này đã được hoàn trả."
                    : "Phiếu nháp chưa thể tiếp nhận trả sách.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(ToTraPhieuMuonViewModel(phieuMuon));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TraSach(
            int id,
            TraPhieuMuonViewModel model)
        {
            if (id != model.MaPhieuMuon)
                return NotFound();

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var phieuMuon = await LoadPhieuDeTraAsync(id);
            if (phieuMuon == null)
                return NotFound();

            var sachChuaTra = phieuMuon.ChiTietPhieuMuons
                .Where(c => !c.NgayTra.HasValue)
                .OrderBy(c => c.MaChiTiet)
                .ToList();

            if (phieuMuon.TrangThai == TrangThaiPhieuMuon.Nhap)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Phiếu nháp chưa thể tiếp nhận trả sách.");
            }

            if (sachChuaTra.Count == 0)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Toàn bộ sách trong phiếu đã được trả trước đó.");
            }

            if (model.NgayTra.Date < phieuMuon.NgayMuon.Date)
            {
                ModelState.AddModelError(
                    nameof(model.NgayTra),
                    "Ngày trả không thể trước ngày mượn.");
            }

            if (model.NgayTra.Date > DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(model.NgayTra),
                    "Ngày trả không thể nằm trong tương lai.");
            }

            var submittedIds = model.Sach
                .Select(item => item.MaChiTiet)
                .ToList();
            var expectedIds = sachChuaTra
                .Select(item => item.MaChiTiet)
                .ToHashSet();
            if (submittedIds.Count != expectedIds.Count ||
                submittedIds.Distinct().Count() != submittedIds.Count ||
                !submittedIds.All(expectedIds.Contains))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Danh sách sách trả không khớp với phiếu mượn. Vui lòng tải lại trang.");
            }

            if (!ModelState.IsValid)
            {
                await transaction.RollbackAsync();
                CopyTraPhieuDisplayData(model, phieuMuon);
                return View(model);
            }

            foreach (var chiTiet in sachChuaTra)
            {
                var submitted = model.Sach.Single(item =>
                    item.MaChiTiet == chiTiet.MaChiTiet);
                var tinhTrang = submitted.TinhTrangKhiTra!.Value;

                chiTiet.NgayTra = model.NgayTra.Date;
                chiTiet.TinhTrangKhiTra = tinhTrang;
                chiTiet.GhiChu = (submitted.GhiChu ?? string.Empty).Trim();
                chiTiet.BanSao.TinhTrang = tinhTrang switch
                {
                    TinhTrangKhiTra.BinhThuong => TinhTrangBanSao.SanCo,
                    TinhTrangKhiTra.HuHong => TinhTrangBanSao.HuHong,
                    TinhTrangKhiTra.Mat => TinhTrangBanSao.Mat,
                    _ => chiTiet.BanSao.TinhTrang
                };
            }

            phieuMuon.TrangThai = TrangThaiPhieuMuon.DaTra;

            var soSachBinhThuong = sachChuaTra.Count(c =>
                c.TinhTrangKhiTra == TinhTrangKhiTra.BinhThuong);
            var soSachCanLapPhat = sachChuaTra.Count(c =>
                model.NgayTra.Date > phieuMuon.NgayHenTra.Date ||
                c.TinhTrangKhiTra is TinhTrangKhiTra.HuHong or TinhTrangKhiTra.Mat);

            _context.ThongBao.Add(new ThongBao
            {
                MaDocGia = phieuMuon.MaDocGia,
                MaSuKien = $"tra-phieu-{phieuMuon.MaPhieuMuon}",
                TieuDe = "Đã xác nhận trả phiếu mượn",
                NoiDung = $"Phiếu #PM-{phieuMuon.MaPhieuMuon:D5} gồm {sachChuaTra.Count} cuốn đã được xác nhận trả ngày {model.NgayTra:dd/MM/yyyy}. Sách bình thường: {soSachBinhThuong}; cần kiểm tra phạt: {soSachCanLapPhat}.",
                LienKet = string.Empty,
                Loai = soSachCanLapPhat == 0 ? "success" : "warning",
                NgayTao = DateTime.Now
            });

            var dueEvent = $"phieu-muon-{phieuMuon.MaPhieuMuon}-han-tra";
            var oldDueNotification = await _context.ThongBao
                .FirstOrDefaultAsync(t =>
                    t.MaDocGia == phieuMuon.MaDocGia &&
                    t.MaSuKien == dueEvent);
            if (oldDueNotification != null)
                _context.ThongBao.Remove(oldDueNotification);

            await _context.SaveChangesAsync();

            foreach (var chiTiet in sachChuaTra.Where(c =>
                         c.TinhTrangKhiTra == TinhTrangKhiTra.BinhThuong))
            {
                await _datTruocService.PhanBoBanSaoAsync(chiTiet.BanSao);
            }

            await transaction.CommitAsync();

            TempData["Success"] = soSachCanLapPhat == 0
                ? $"Đã xác nhận trả toàn bộ {sachChuaTra.Count} cuốn trong phiếu."
                : $"Đã xác nhận trả toàn bộ phiếu. Có {soSachCanLapPhat} cuốn cần lập phiếu phạt.";

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GiaHan(int id)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            await _phieuMuonService.CapNhatTrangThaiAsync(id);

            var phieuMuon = await _context.PhieuMuon
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(c => c.BanSao)
                .Include(p => p.DocGia)
                .FirstOrDefaultAsync(p => p.MaPhieuMuon == id);

            if (phieuMuon == null)
                return NotFound();

            if (phieuMuon.TrangThai != TrangThaiPhieuMuon.DangMuon ||
                phieuMuon.ChiTietPhieuMuons.All(c => c.NgayTra.HasValue))
            {
                TempData["Error"] = "Chỉ phiếu đang mượn và chưa quá hạn mới được gia hạn.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (phieuMuon.SoLanGiaHan >= LibraryRules.SoLanGiaHanToiDa)
            {
                TempData["Error"] =
                    $"Phiếu đã đạt giới hạn {LibraryRules.SoLanGiaHanToiDa} lần gia hạn.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var conNoPhat = await _context.PhieuPhat.AnyAsync(p =>
                p.TrangThai == TrangThaiPhieuPhat.ChuaDong &&
                p.ChiTietPhieuMuon.PhieuMuon.MaDocGia == phieuMuon.MaDocGia);

            var maSachDangMuon = phieuMuon.ChiTietPhieuMuons
                .Where(c => !c.NgayTra.HasValue)
                .Select(c => c.BanSao.MaSach)
                .Distinct()
                .ToList();

            var coNguoiDatTruoc = await _context.DatTruoc.AnyAsync(d =>
                maSachDangMuon.Contains(d.MaSach) &&
                d.MaDocGia != phieuMuon.MaDocGia &&
                (d.TrangThai == TrangThaiDatTruoc.DangCho ||
                 d.TrangThai == TrangThaiDatTruoc.DaCoSach));

            if (conNoPhat || coNguoiDatTruoc)
            {
                TempData["Error"] = conNoPhat
                    ? "Độc giả còn phiếu phạt chưa thanh toán."
                    : "Không thể gia hạn vì có độc giả khác đang đặt trước sách trong phiếu.";
                return RedirectToAction(nameof(Details), new { id });
            }

            phieuMuon.NgayHenTra = phieuMuon.NgayHenTra.Date
                .AddDays(LibraryRules.SoNgayMoiLanGiaHan);
            phieuMuon.SoLanGiaHan++;
            phieuMuon.NgayGiaHanGanNhat = DateTime.Now;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] =
                $"Đã gia hạn thêm {LibraryRules.SoNgayMoiLanGiaHan} ngày.";
            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var phieuMuon = await _context.PhieuMuon
                .Include(p => p.DocGia)
                .Include(p => p.NhanVien)
                .Include(p => p.ChiTietPhieuMuons)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.MaPhieuMuon == id);

            return phieuMuon == null
                ? NotFound()
                : View(phieuMuon);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phieuMuon = await _context.PhieuMuon
                .Include(p => p.ChiTietPhieuMuons)
                .FirstOrDefaultAsync(p => p.MaPhieuMuon == id);

            if (phieuMuon == null)
                return NotFound();

            if (phieuMuon.ChiTietPhieuMuons.Count != 0)
            {
                TempData["Error"] =
                    "Không thể xóa phiếu đã có sách. " +
                    "Hãy xử lý từng lượt mượn thay vì xóa lịch sử.";
                return RedirectToAction(nameof(Index));
            }

            _context.PhieuMuon.Remove(phieuMuon);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa phiếu trống.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDocGiasAsync(int? selectedId = null)
        {
            var docGias = await _context.DocGia
                .Where(d =>
                    d.TrangThai == TrangThaiDocGia.HoatDong &&
                    d.NgayHetHanThe >= DateTime.Today)
                .OrderBy(d => d.HoTen)
                .AsNoTracking()
                .ToListAsync();

            ViewData["MaDocGia"] = new SelectList(
                docGias,
                "MaDocGia",
                "HoTen",
                selectedId);
            ViewData["SoDocGiaHopLe"] = docGias.Count;
        }

        private void ValidateDocGia(DocGia? docGia)
        {
            if (docGia == null)
            {
                ModelState.AddModelError(
                    nameof(LapPhieuMuonViewModel.MaDocGia),
                    "Không tìm thấy độc giả.");
                return;
            }

            if (docGia.TrangThai != TrangThaiDocGia.HoatDong)
            {
                ModelState.AddModelError(
                    nameof(LapPhieuMuonViewModel.MaDocGia),
                    "Thẻ độc giả đang bị khóa.");
            }

            if (docGia.NgayHetHanThe < DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(LapPhieuMuonViewModel.MaDocGia),
                    "Thẻ độc giả đã hết hạn.");
            }
        }

        private Task<PhieuMuon?> LoadPhieuDeTraAsync(int id)
        {
            return _context.PhieuMuon
                .Include(p => p.DocGia)
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(c => c.BanSao)
                        .ThenInclude(b => b.Sach)
                .FirstOrDefaultAsync(p => p.MaPhieuMuon == id);
        }

        private static TraPhieuMuonViewModel ToTraPhieuMuonViewModel(
            PhieuMuon phieuMuon)
        {
            var model = new TraPhieuMuonViewModel
            {
                MaPhieuMuon = phieuMuon.MaPhieuMuon,
                HoTenDocGia = phieuMuon.DocGia.HoTen,
                NgayMuon = phieuMuon.NgayMuon,
                NgayHenTra = phieuMuon.NgayHenTra,
                NgayTra = DateTime.Today
            };
            CopyTraPhieuDisplayData(model, phieuMuon);
            return model;
        }

        private static void CopyTraPhieuDisplayData(
            TraPhieuMuonViewModel model,
            PhieuMuon phieuMuon)
        {
            model.HoTenDocGia = phieuMuon.DocGia.HoTen;
            model.NgayMuon = phieuMuon.NgayMuon;
            model.NgayHenTra = phieuMuon.NgayHenTra;

            var submitted = model.Sach
                .GroupBy(item => item.MaChiTiet)
                .ToDictionary(group => group.Key, group => group.First());
            model.Sach = phieuMuon.ChiTietPhieuMuons
                .Where(c => !c.NgayTra.HasValue)
                .OrderBy(c => c.MaChiTiet)
                .Select(c => new TraSachTrongPhieuViewModel
                {
                    MaChiTiet = c.MaChiTiet,
                    TenSach = c.BanSao.Sach.TenSach,
                    MaVach = c.BanSao.MaVach,
                    TinhTrangKhiTra = submitted.TryGetValue(c.MaChiTiet, out var old)
                        ? old.TinhTrangKhiTra
                        : null,
                    GhiChu = submitted.TryGetValue(c.MaChiTiet, out old)
                        ? old.GhiChu
                        : string.Empty
                })
                .ToList();
        }
    }
}

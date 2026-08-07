using System.Data;
using bài_tập_1.Data;
using bài_tập_1.Models;
using bài_tập_1.Models.ViewModels;
using bài_tập_1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bài_tập_1.Controllers
{
    [Authorize]
    public class MuonOnlineController : Controller
    {
        private readonly bài_tập_1Context _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly DocGiaEligibilityService _eligibilityService;
        private readonly MuonOnlineService _muonOnlineService;
        private readonly DatTruocService _datTruocService;

        public MuonOnlineController(
            bài_tập_1Context context,
            UserManager<IdentityUser> userManager,
            DocGiaEligibilityService eligibilityService,
            MuonOnlineService muonOnlineService,
            DatTruocService datTruocService)
        {
            _context = context;
            _userManager = userManager;
            _eligibilityService = eligibilityService;
            _muonOnlineService = muonOnlineService;
            _datTruocService = datTruocService;
        }

        [Authorize(Roles = "DocGia")]
        public async Task<IActionResult> Create(int? maSach)
        {
            await _muonOnlineService.XuLyHetHanAsync();
            var model = new TaoYeuCauMuonOnlineViewModel();
            if (maSach.HasValue)
                model.MaSachIds.Add(maSach.Value);

            await NapDanhSachSachAsync(model);
            if (maSach.HasValue &&
                model.DanhSachSach.All(s => s.MaSach != maSach.Value))
            {
                return NotFound();
            }

            return View(model);
        }

        [Authorize(Roles = "DocGia")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaoYeuCauMuonOnlineViewModel model)
        {
            await _muonOnlineService.XuLyHetHanAsync();
            model.MaSachIds = model.MaSachIds.Distinct().ToList();
            if (model.MaSachIds.Count is < 1 or > LibraryRules.SoSachMuonToiDa)
            {
                ModelState.AddModelError(nameof(model.MaSachIds),
                    $"Mỗi phiếu phải có từ 1 đến {LibraryRules.SoSachMuonToiDa} cuốn sách.");
            }

            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            var userId = _userManager.GetUserId(User);
            var docGia = await _context.DocGia
                .FirstOrDefaultAsync(d => d.UserId == userId);
            var saches = await _context.Sach
                .Include(s => s.BanSaos)
                .Where(s => model.MaSachIds.Contains(s.MaSach))
                .ToListAsync();

            if (docGia == null)
                ModelState.AddModelError(string.Empty, "Không tìm thấy hồ sơ độc giả.");
            if (saches.Count != model.MaSachIds.Count)
                ModelState.AddModelError(nameof(model.MaSachIds), "Có sách không tồn tại hoặc không còn khả dụng.");

            if (model.NgayHenNhan.Date < DateTime.Today ||
                model.NgayHenNhan.Date > DateTime.Today.AddDays(3))
            {
                ModelState.AddModelError(nameof(model.NgayHenNhan),
                    "Ngày nhận phải từ hôm nay đến 3 ngày tới.");
            }

            if (model.NgayHenTra.Date <= model.NgayHenNhan.Date ||
                model.NgayHenTra.Date > model.NgayHenNhan.Date.AddDays(LibraryRules.SoNgayMuonToiDa))
            {
                ModelState.AddModelError(nameof(model.NgayHenTra),
                    $"Hạn trả phải sau ngày nhận và không quá {LibraryRules.SoNgayMuonToiDa} ngày.");
            }

            var selectedCopies = new List<(Sach Sach, BanSao BanSao)>();
            foreach (var sach in saches)
            {
                var banSao = sach.BanSaos
                    .Where(b => b.TinhTrang == TinhTrangBanSao.SanCo)
                    .OrderBy(b => b.MaBanSao)
                    .FirstOrDefault();
                if (banSao == null)
                {
                    ModelState.AddModelError(string.Empty,
                        $"“{sach.TenSach}” vừa hết bản sẵn có. Vui lòng bỏ sách này hoặc đặt trước.");
                }
                else
                {
                    selectedCopies.Add((sach, banSao));
                }
            }

            if (docGia != null)
            {
                var generalErrors = await _eligibilityService.KiemTraAsync(
                    docGia.MaDocGia,
                    new KiemTraDocGiaOptions { KiemTraGioiHanSach = false });
                foreach (var error in generalErrors)
                    ModelState.AddModelError(string.Empty, error);

                var soSachDangMuon = await _context.ChiTietPhieuMuon.CountAsync(c =>
                    c.PhieuMuon.MaDocGia == docGia.MaDocGia && c.NgayTra == null);
                var soSachDangCho = await _context.YeuCauMuonOnline.CountAsync(y =>
                    y.MaDocGia == docGia.MaDocGia &&
                    (y.TrangThai == TrangThaiYeuCauMuonOnline.ChoNhan ||
                     y.TrangThai == TrangThaiYeuCauMuonOnline.DaDuyet));
                if (soSachDangMuon + soSachDangCho + model.MaSachIds.Count >
                    LibraryRules.SoSachMuonToiDa)
                {
                    ModelState.AddModelError(string.Empty,
                        $"Tổng số sách đang mượn và chờ nhận không được vượt quá {LibraryRules.SoSachMuonToiDa} cuốn.");
                }

                foreach (var maSach in model.MaSachIds)
                {
                    var bookErrors = await _eligibilityService.KiemTraAsync(
                        docGia.MaDocGia,
                        new KiemTraDocGiaOptions
                        {
                            MaSach = maSach,
                            KiemTraGioiHanSach = false
                        });
                    foreach (var error in bookErrors)
                        ModelState.AddModelError(string.Empty, error);
                }
            }

            if (!ModelState.IsValid)
            {
                await transaction.RollbackAsync();
                await NapDanhSachSachAsync(model);
                return View(model);
            }

            var confirmationCode = await TaoMaXacNhanAsync();
            var now = DateTime.Now;
            foreach (var item in selectedCopies)
            {
                item.BanSao.TinhTrang = TinhTrangBanSao.DaGiu;
                _context.YeuCauMuonOnline.Add(new YeuCauMuonOnline
                {
                    MaXacNhan = confirmationCode,
                    MaDocGia = docGia!.MaDocGia,
                    MaSach = item.Sach.MaSach,
                    MaBanSao = item.BanSao.MaBanSao,
                    NgayTao = now,
                    NgayHenNhan = model.NgayHenNhan.Date,
                    NgayHenTra = model.NgayHenTra.Date,
                    HanNhanSach = model.NgayHenNhan.Date.AddDays(1).AddTicks(-1),
                    GhiChu = model.GhiChu?.Trim() ?? string.Empty
                });
            }

            var nhanVienIds = await _context.NhanVien
                .Select(n => n.MaNhanVien)
                .ToListAsync();
            foreach (var maNhanVien in nhanVienIds)
            {
                _context.ThongBao.Add(new ThongBao
                {
                    MaNhanVien = maNhanVien,
                    MaSuKien = "muon-online:" + confirmationCode,
                    TieuDe = "Có yêu cầu mượn online mới",
                    NoiDung = $"{docGia!.HoTen} đề nghị mượn {selectedCopies.Count} cuốn. Hãy kiểm tra sách và duyệt yêu cầu.",
                    LienKet = Url.Action(nameof(XacNhan), "MuonOnline", new { ma = confirmationCode }) ?? string.Empty,
                    Loai = "info",
                    NgayTao = now
                });
            }

            try
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty,
                    "Một bản sao vừa được người khác giữ. Vui lòng chọn lại sách.");
                await NapDanhSachSachAsync(model);
                return View(model);
            }

            return RedirectToAction(nameof(Phieu), new { ma = confirmationCode });
        }

        [Authorize(Roles = "DocGia")]
        public async Task<IActionResult> CuaToi(string? trangThai, int page = 1)
        {
            await _muonOnlineService.XuLyHetHanAsync();
            var userId = _userManager.GetUserId(User);
            var rows = await RequestQuery()
                .Where(y => y.DocGia.UserId == userId)
                .OrderByDescending(y => y.NgayTao)
                .ToListAsync();
            var groups = rows.GroupBy(y => y.MaXacNhan)
                .Select(TaoPhieuViewModel)
                .Where(p => trangThai switch
                {
                    "waiting" => p.TrangThai == TrangThaiYeuCauMuonOnline.ChoNhan,
                    "approved" => p.TrangThai == TrangThaiYeuCauMuonOnline.DaDuyet,
                    "received" => p.TrangThai == TrangThaiYeuCauMuonOnline.DaNhan,
                    "closed" => p.TrangThai is TrangThaiYeuCauMuonOnline.DaHuy or TrangThaiYeuCauMuonOnline.HetHan or TrangThaiYeuCauMuonOnline.TuChoi,
                    _ => true
                })
                .OrderByDescending(p => p.NgayTao)
                .ToList();

            ViewData["Status"] = trangThai;
            ViewData["ChoNhan"] = groups.Count(p => p.TrangThai == TrangThaiYeuCauMuonOnline.ChoNhan);
            ViewData["DaDuyet"] = groups.Count(p => p.TrangThai == TrangThaiYeuCauMuonOnline.DaDuyet);
            ViewData["DaNhan"] = groups.Count(p => p.TrangThai == TrangThaiYeuCauMuonOnline.DaNhan);
            var pagination = Pagination.Create(page, groups.Count);
            ViewData["Pagination"] = pagination;
            return View(groups.Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize).ToList());
        }

        [Authorize(Roles = "DocGia")]
        public async Task<IActionResult> Phieu(string ma)
        {
            await _muonOnlineService.XuLyHetHanAsync();
            var userId = _userManager.GetUserId(User);
            var rows = await RequestQuery()
                .Where(y => y.MaXacNhan == ma && y.DocGia.UserId == userId)
                .ToListAsync();
            return rows.Count == 0 ? NotFound() : View(TaoPhieuViewModel(rows));
        }

        [Authorize(Roles = "DocGia")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(string ma)
        {
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);
            var userId = _userManager.GetUserId(User);
            var rows = await _context.YeuCauMuonOnline
                .Include(y => y.DocGia)
                .Include(y => y.BanSao)
                .Where(y => y.MaXacNhan == ma && y.DocGia.UserId == userId)
                .ToListAsync();
            if (rows.Count == 0)
                return NotFound();
            if (rows.Any(y => y.TrangThai is not
                    (TrangThaiYeuCauMuonOnline.ChoNhan or
                     TrangThaiYeuCauMuonOnline.DaDuyet)))
            {
                TempData["Error"] = "Phiếu này không còn có thể hủy.";
                return RedirectToAction(nameof(Phieu), new { ma });
            }

            foreach (var row in rows)
            {
                row.TrangThai = TrangThaiYeuCauMuonOnline.DaHuy;
                if (row.BanSao.TinhTrang == TinhTrangBanSao.DaGiu)
                    row.BanSao.TinhTrang = TinhTrangBanSao.SanCo;
            }
            await _context.SaveChangesAsync();
            foreach (var copy in rows.Select(y => y.BanSao))
                await _datTruocService.PhanBoBanSaoAsync(copy);
            await transaction.CommitAsync();

            TempData["Success"] = "Đã hủy toàn bộ phiếu mượn online.";
            return RedirectToAction(nameof(CuaToi));
        }

        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> XacNhan(string ma)
        {
            await _muonOnlineService.XuLyHetHanAsync();
            var rows = await RequestQuery()
                .Where(y => y.MaXacNhan == ma)
                .ToListAsync();
            return rows.Count == 0 ? NotFound() : View(TaoPhieuViewModel(rows));
        }

        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> TraCuu(string ma)
        {
            var code = ma?.Trim().Replace("#MO-", string.Empty,
                StringComparison.OrdinalIgnoreCase).ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(code))
            {
                TempData["Error"] = "Vui lòng nhập mã xác nhận của độc giả.";
                return RedirectToAction("Index", "DatTruocs");
            }

            var exists = await _context.YeuCauMuonOnline
                .AnyAsync(y => y.MaXacNhan == code);
            if (!exists)
            {
                TempData["Error"] = $"Không tìm thấy phiếu có mã {code}.";
                return RedirectToAction("Index", "DatTruocs");
            }

            return RedirectToAction(nameof(XacNhan), new { ma = code });
        }

        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuyetYeuCau(string ma)
        {
            await _muonOnlineService.XuLyHetHanAsync();
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);
            var rows = await _context.YeuCauMuonOnline
                .Include(y => y.DocGia)
                .Include(y => y.BanSao)
                .Where(y => y.MaXacNhan == ma)
                .ToListAsync();
            if (rows.Count == 0)
                return NotFound();
            if (rows.Any(y =>
                    y.TrangThai != TrangThaiYeuCauMuonOnline.ChoNhan ||
                    y.BanSao.TinhTrang != TinhTrangBanSao.DaGiu ||
                    y.HanNhanSach < DateTime.Now))
            {
                TempData["Error"] = "Yêu cầu không còn hiệu lực hoặc có sách không còn được giữ.";
                return RedirectToAction(nameof(XacNhan), new { ma });
            }

            foreach (var row in rows)
                row.TrangThai = TrangThaiYeuCauMuonOnline.DaDuyet;

            var eventCode = "muon-online-approved:" + ma;
            if (!await _context.ThongBao.AnyAsync(t =>
                    t.MaDocGia == rows[0].MaDocGia && t.MaSuKien == eventCode))
            {
                _context.ThongBao.Add(new ThongBao
                {
                    MaDocGia = rows[0].MaDocGia,
                    MaSuKien = eventCode,
                    TieuDe = "Yêu cầu mượn online đã được duyệt",
                    NoiDung = $"Thư viện đã giữ đủ {rows.Count} cuốn. Hãy đến quầy đúng ngày hẹn và đưa mã {ma} cho nhân viên.",
                    LienKet = Url.Action(nameof(Phieu), "MuonOnline", new { ma }) ?? string.Empty,
                    Loai = "success",
                    NgayTao = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["Success"] = "Đã duyệt yêu cầu. Độc giả đã nhận được thông báo và mã đến nhận sách.";
            return RedirectToAction("Index", "DatTruocs");
        }

        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TuChoiYeuCau(string ma, string lyDo)
        {
            var reason = lyDo?.Trim() ?? string.Empty;
            if (reason.Length < 5 || reason.Length > 500)
            {
                TempData["Error"] = "Vui lòng nhập lý do từ chối từ 5 đến 500 ký tự.";
                return RedirectToAction(nameof(XacNhan), new { ma });
            }

            await _muonOnlineService.XuLyHetHanAsync();
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);
            var rows = await _context.YeuCauMuonOnline
                .Include(y => y.DocGia)
                .Include(y => y.BanSao)
                .Where(y => y.MaXacNhan == ma)
                .ToListAsync();
            if (rows.Count == 0)
                return NotFound();
            if (rows.Any(y => y.TrangThai is not
                    (TrangThaiYeuCauMuonOnline.ChoNhan or
                     TrangThaiYeuCauMuonOnline.DaDuyet)))
            {
                TempData["Error"] = "Phiếu này đã được xử lý nên không thể từ chối.";
                return RedirectToAction(nameof(XacNhan), new { ma });
            }

            var copies = rows
                .Select(y => y.BanSao)
                .GroupBy(b => b.MaBanSao)
                .Select(group => group.First())
                .ToList();
            foreach (var row in rows)
            {
                row.TrangThai = TrangThaiYeuCauMuonOnline.TuChoi;
                row.LyDoTuChoi = reason;
                if (row.BanSao.TinhTrang == TinhTrangBanSao.DaGiu)
                    row.BanSao.TinhTrang = TinhTrangBanSao.SanCo;
            }

            var eventCode = "muon-online-rejected:" + ma;
            if (!await _context.ThongBao.AnyAsync(t =>
                    t.MaDocGia == rows[0].MaDocGia && t.MaSuKien == eventCode))
            {
                _context.ThongBao.Add(new ThongBao
                {
                    MaDocGia = rows[0].MaDocGia,
                    MaSuKien = eventCode,
                    TieuDe = "Yêu cầu mượn online không được duyệt",
                    NoiDung = $"Phiếu {ma} đã bị từ chối. Lý do: {reason}",
                    LienKet = Url.Action(nameof(Phieu), "MuonOnline", new { ma }) ?? string.Empty,
                    Loai = "danger",
                    NgayTao = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            foreach (var copy in copies)
                await _datTruocService.PhanBoBanSaoAsync(copy);
            await transaction.CommitAsync();

            TempData["Success"] = "Đã từ chối yêu cầu, thông báo lý do cho độc giả và giải phóng các bản sao.";
            return RedirectToAction("Index", "DatTruocs");
        }

        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanNhanSach(string ma)
        {
            await _muonOnlineService.XuLyHetHanAsync();
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);
            var rows = await _context.YeuCauMuonOnline
                .Include(y => y.DocGia)
                .Include(y => y.BanSao)
                .Where(y => y.MaXacNhan == ma)
                .ToListAsync();
            if (rows.Count == 0)
                return NotFound();
            if (rows.Any(y =>
                    y.TrangThai != TrangThaiYeuCauMuonOnline.DaDuyet ||
                    y.BanSao.TinhTrang != TinhTrangBanSao.DaGiu ||
                    y.HanNhanSach < DateTime.Now))
            {
                TempData["Error"] = "Mã chưa được duyệt, đã hết hạn hoặc có sách đã được xử lý.";
                return RedirectToAction(nameof(XacNhan), new { ma });
            }

            var eligibilityErrors = new List<string>();
            foreach (var row in rows)
            {
                eligibilityErrors.AddRange(await _eligibilityService.KiemTraAsync(
                    row.MaDocGia,
                    new KiemTraDocGiaOptions
                    {
                        MaSach = row.MaSach,
                        BoQuaMaYeuCauOnline = row.MaYeuCau
                    }));
            }
            if (eligibilityErrors.Count != 0)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = string.Join(" ", eligibilityErrors.Distinct());
                return RedirectToAction(nameof(XacNhan), new { ma });
            }

            var userId = _userManager.GetUserId(User);
            var nhanVien = await _context.NhanVien
                .FirstOrDefaultAsync(n => n.UserId == userId);
            if (nhanVien == null)
                return Forbid();

            var loan = new PhieuMuon
            {
                MaDocGia = rows[0].MaDocGia,
                MaNhanVien = nhanVien.MaNhanVien,
                NgayMuon = DateTime.Now,
                NgayHenTra = rows[0].NgayHenTra,
                TrangThai = TrangThaiPhieuMuon.DangMuon
            };
            foreach (var row in rows)
            {
                loan.ChiTietPhieuMuons.Add(new ChiTietPhieuMuon
                {
                    MaBanSao = row.MaBanSao,
                    PhiThue = LibraryRules.PhiThueMoiCuon,
                    GhiChu = "Tạo từ phiếu mượn online " + row.MaXacNhan
                });
                row.BanSao.TinhTrang = TinhTrangBanSao.DangMuon;
                row.TrangThai = TrangThaiYeuCauMuonOnline.DaNhan;
                row.PhieuMuon = loan;
            }

            _context.ThongBao.Add(new ThongBao
            {
                MaDocGia = rows[0].MaDocGia,
                MaSuKien = "muon-online-received:" + ma,
                TieuDe = "Đã lập phiếu mượn",
                NoiDung = $"Thư viện đã xác nhận bạn nhận {rows.Count} cuốn. Hạn trả là {loan.NgayHenTra:dd/MM/yyyy}.",
                LienKet = Url.Action("LichSuMuon", "Profile") ?? string.Empty,
                Loai = "success",
                NgayTao = DateTime.Now
            });
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] = $"Đã xác nhận độc giả nhận {rows.Count} cuốn và tạo một phiếu mượn.";
            return RedirectToAction("Details", "PhieuMuons", new { id = loan.MaPhieuMuon });
        }

        private async Task NapDanhSachSachAsync(TaoYeuCauMuonOnlineViewModel model)
        {
            var books = await _context.Sach
                .Include(s => s.BanSaos)
                .Include(s => s.SachTacGias)
                    .ThenInclude(st => st.TacGia)
                .Where(s => s.BanSaos.Any(b => b.TinhTrang == TinhTrangBanSao.SanCo))
                .OrderBy(s => s.TenSach)
                .AsNoTracking()
                .ToListAsync();
            model.DanhSachSach = books.Select(s => new ChonSachMuonOnlineItem
                {
                    MaSach = s.MaSach,
                    TenSach = s.TenSach,
                    AnhBia = s.AnhBia,
                    TacGia = string.Join(", ", s.SachTacGias.Select(st => st.TacGia.HoTen)),
                    SoBanSanCo = s.BanSaos.Count(b => b.TinhTrang == TinhTrangBanSao.SanCo)
                })
                .ToList();
        }

        private async Task<string> TaoMaXacNhanAsync()
        {
            string code;
            do
            {
                code = Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
            } while (await _context.YeuCauMuonOnline.AnyAsync(y => y.MaXacNhan == code));
            return code;
        }

        private static PhieuMuonOnlineViewModel TaoPhieuViewModel(
            IEnumerable<YeuCauMuonOnline> source)
        {
            var rows = source.OrderBy(y => y.MaYeuCau).ToList();
            var first = rows[0];
            return new PhieuMuonOnlineViewModel
            {
                MaXacNhan = first.MaXacNhan,
                MaDocGia = first.MaDocGia,
                DocGia = first.DocGia,
                NgayTao = first.NgayTao,
                NgayHenNhan = first.NgayHenNhan,
                NgayHenTra = first.NgayHenTra,
                HanNhanSach = first.HanNhanSach,
                GhiChu = first.GhiChu,
                LyDoTuChoi = first.LyDoTuChoi,
                TrangThai = first.TrangThai,
                MaPhieuMuon = first.MaPhieuMuon,
                ChiTiet = rows
            };
        }

        private IQueryable<YeuCauMuonOnline> RequestQuery() =>
            _context.YeuCauMuonOnline
                .Include(y => y.DocGia)
                .Include(y => y.Sach)
                .Include(y => y.BanSao)
                .Include(y => y.PhieuMuon)
                .AsNoTracking();
    }
}

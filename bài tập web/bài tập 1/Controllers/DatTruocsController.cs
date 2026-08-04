using System.Data;
using bài_tập_1.Data;
using bài_tập_1.Models;
using bài_tập_1.Models.ViewModels;
using bài_tập_1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace bài_tập_1.Controllers
{
    [Authorize]
    public class DatTruocsController : Controller
    {
        private readonly bài_tập_1Context _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly DatTruocService _datTruocService;
        private readonly DocGiaEligibilityService _eligibilityService;
        private readonly MuonOnlineService _muonOnlineService;

        public DatTruocsController(
            bài_tập_1Context context,
            UserManager<IdentityUser> userManager,
            DatTruocService datTruocService,
            DocGiaEligibilityService eligibilityService,
            MuonOnlineService muonOnlineService)
        {
            _context = context;
            _userManager = userManager;
            _datTruocService = datTruocService;
            _eligibilityService = eligibilityService;
            _muonOnlineService = muonOnlineService;
        }

        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Index(
            string? q,
            string? trangThai,
            int page = 1)
        {
            await _datTruocService.XuLyHetHanAsync();
            await _muonOnlineService.XuLyHetHanAsync();

            var query = _context.DatTruoc
                .Include(d => d.DocGia)
                .Include(d => d.Sach)
                .Include(d => d.BanSaoDuocGiu)
                .AsNoTracking()
                .AsQueryable();

            ViewData["DangCho"] = await query.CountAsync(d =>
                d.TrangThai == TrangThaiDatTruoc.DangCho);
            ViewData["DaCoSach"] = await query.CountAsync(d =>
                d.TrangThai == TrangThaiDatTruoc.DaCoSach);
            ViewData["HoanThanh"] = await query.CountAsync(d =>
                d.TrangThai == TrangThaiDatTruoc.HoanThanh);

            var onlineQuery = _context.YeuCauMuonOnline
                .Include(y => y.DocGia)
                .Include(y => y.Sach)
                .Include(y => y.BanSao)
                .Include(y => y.PhieuMuon)
                .AsNoTracking()
                .AsQueryable();

            ViewData["MuonOnlineChoNhan"] = await onlineQuery
                .Where(y => y.TrangThai == TrangThaiYeuCauMuonOnline.ChoNhan)
                .Select(y => y.MaXacNhan)
                .Distinct()
                .CountAsync();
            ViewData["MuonOnlineDaDuyet"] = await onlineQuery
                .Where(y => y.TrangThai == TrangThaiYeuCauMuonOnline.DaDuyet)
                .Select(y => y.MaXacNhan)
                .Distinct()
                .CountAsync();
            ViewData["MuonOnlineHomNay"] = await onlineQuery
                .Where(y => y.NgayTao.Date == DateTime.Today)
                .Select(y => y.MaXacNhan)
                .Distinct()
                .CountAsync();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var keyword = q.Trim();
                query = query.Where(d =>
                    d.DocGia.HoTen.Contains(keyword) ||
                    d.Sach.TenSach.Contains(keyword) ||
                    (d.BanSaoDuocGiu != null &&
                     d.BanSaoDuocGiu.MaVach.Contains(keyword)));
                onlineQuery = onlineQuery.Where(y =>
                    y.DocGia.HoTen.Contains(keyword) ||
                    y.Sach.TenSach.Contains(keyword) ||
                    y.MaXacNhan.Contains(keyword) ||
                    y.BanSao.MaVach.Contains(keyword));
            }

            query = trangThai switch
            {
                "waiting" => query.Where(d =>
                    d.TrangThai == TrangThaiDatTruoc.DangCho),
                "ready" => query.Where(d =>
                    d.TrangThai == TrangThaiDatTruoc.DaCoSach),
                "completed" => query.Where(d =>
                    d.TrangThai == TrangThaiDatTruoc.HoanThanh),
                "cancelled" => query.Where(d =>
                    d.TrangThai == TrangThaiDatTruoc.DaHuy ||
                    d.TrangThai == TrangThaiDatTruoc.HetHan),
                _ => query
            };

            ViewData["Search"] = q;
            ViewData["Status"] = trangThai;
            var onlineRows = await onlineQuery
                .OrderBy(y => y.TrangThai != TrangThaiYeuCauMuonOnline.ChoNhan)
                .ThenBy(y => y.TrangThai != TrangThaiYeuCauMuonOnline.DaDuyet)
                .ThenBy(y => y.HanNhanSach)
                .ThenByDescending(y => y.NgayTao)
                .ToListAsync();
            var displayedCodes = onlineRows
                .Select(y => y.MaXacNhan)
                .Distinct()
                .Take(20)
                .ToHashSet();
            ViewData["YeuCauMuonOnline"] = onlineRows
                .Where(y => displayedCodes.Contains(y.MaXacNhan))
                .ToList();

            var pagination = Pagination.Create(page, await query.CountAsync());
            ViewData["Pagination"] = pagination;
            return View(await query
                .OrderBy(d =>
                    d.TrangThai != TrangThaiDatTruoc.DaCoSach)
                .ThenBy(d =>
                    d.TrangThai != TrangThaiDatTruoc.DangCho)
                .ThenBy(d => d.NgayDat)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            await _datTruocService.XuLyHetHanAsync();

            var datTruoc = await LoadDatTruocAsync(id.Value);
            if (datTruoc == null)
                return NotFound();

            if (!IsStaff())
            {
                var userId = _userManager.GetUserId(User);
                if (datTruoc.DocGia.UserId != userId)
                    return Forbid();
            }

            if (datTruoc.TrangThai == TrangThaiDatTruoc.DangCho)
            {
                ViewData["ViTriHangDoi"] =
                    await _context.DatTruoc.CountAsync(d =>
                        d.MaSach == datTruoc.MaSach &&
                        d.TrangThai == TrangThaiDatTruoc.DangCho &&
                        (d.NgayDat < datTruoc.NgayDat ||
                         (d.NgayDat == datTruoc.NgayDat &&
                          d.MaDatTruoc <= datTruoc.MaDatTruoc)));
            }

            return View(datTruoc);
        }

        [Authorize(Roles = "Admin,NhanVien,DocGia")]
        public async Task<IActionResult> Create(int? maSach)
        {
            if (maSach == null)
                return RedirectToAction("Index", "Saches");

            await _datTruocService.XuLyHetHanAsync(maSach.Value);

            var model = await TaoDatTruocViewModelAsync(maSach.Value);
            return model == null ? NotFound() : View(model);
        }

        [Authorize(Roles = "Admin,NhanVien,DocGia")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DatTruocViewModel input)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            await _datTruocService.XuLyHetHanAsync(input.MaSach);

            var docGia = await ResolveDocGiaAsync(input.MaDocGia);
            ValidateDocGia(docGia);

            var sach = await _context.Sach
                .Include(s => s.BanSaos)
                .FirstOrDefaultAsync(s => s.MaSach == input.MaSach);

            if (sach == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Cuốn sách không tồn tại.");
            }
            else if (sach.BanSaos.Any(b =>
                         b.TinhTrang == TinhTrangBanSao.SanCo))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Sách đang có bản sao sẵn có. " +
                    "Vui lòng đến quầy để thực hiện mượn trực tiếp.");
            }

            if (docGia != null)
            {
                var errors = await _eligibilityService.KiemTraAsync(
                    docGia.MaDocGia,
                    new KiemTraDocGiaOptions
                    {
                        MaSach = input.MaSach,
                        KiemTraGioiHanSach = false
                    });
                foreach (var error in errors)
                    ModelState.AddModelError(string.Empty, error);
            }

            if (!ModelState.IsValid)
            {
                await transaction.RollbackAsync();
                var invalidModel = await TaoDatTruocViewModelAsync(
                    input.MaSach,
                    input.MaDocGia);
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
                NgayHetHanDat = DateTime.Now.AddDays(
                    DatTruocService.SoNgayToiDaTrongHangDoi),
                TrangThai = TrangThaiDatTruoc.DangCho
            };

            _context.DatTruoc.Add(datTruoc);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["DatTruocSuccess"] =
                "Yêu cầu đã được thêm vào hàng đợi.";
            return RedirectToAction(
                nameof(Details),
                new { id = datTruoc.MaDatTruoc });
        }

        [Authorize(Roles = "DocGia")]
        public async Task<IActionResult> CuaToi(int page = 1)
        {
            await _datTruocService.XuLyHetHanAsync();

            var userId = _userManager.GetUserId(User);
            var docGia = await _context.DocGia
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == userId);
            if (docGia == null)
                return NotFound();

            var query = _context.DatTruoc
                .Include(d => d.Sach)
                .Include(d => d.BanSaoDuocGiu)
                .Where(d => d.MaDocGia == docGia.MaDocGia)
                .AsNoTracking();
            ViewData["TongDatTruoc"] = await query.CountAsync();
            ViewData["DangCho"] = await query.CountAsync(d =>
                d.TrangThai == TrangThaiDatTruoc.DangCho);
            ViewData["SanSangNhan"] = await query.CountAsync(d =>
                d.TrangThai == TrangThaiDatTruoc.DaCoSach);
            ViewData["DaHoanThanh"] = await query.CountAsync(d =>
                d.TrangThai == TrangThaiDatTruoc.HoanThanh);
            var pagination = Pagination.Create(page, await query.CountAsync());
            ViewData["Pagination"] = pagination;
            var items = await query
                .OrderByDescending(d => d.NgayDat)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(int id)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            var datTruoc = await _context.DatTruoc
                .Include(d => d.DocGia)
                .Include(d => d.BanSaoDuocGiu)
                .FirstOrDefaultAsync(d => d.MaDatTruoc == id);

            if (datTruoc == null)
                return NotFound();

            var isStaff = IsStaff();
            if (!isStaff &&
                datTruoc.DocGia.UserId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            if (datTruoc.TrangThai is not
                (TrangThaiDatTruoc.DangCho or
                 TrangThaiDatTruoc.DaCoSach))
            {
                TempData["DatTruocError"] =
                    "Yêu cầu này không còn có thể hủy.";
                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            await _datTruocService.HuyVaChuyenLuotAsync(datTruoc);
            await transaction.CommitAsync();

            TempData["DatTruocSuccess"] = "Đã hủy yêu cầu đặt trước.";
            return isStaff
                ? RedirectToAction(nameof(Index))
                : RedirectToAction(nameof(CuaToi));
        }

        [Authorize(Roles = "Admin,NhanVien")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanNhanSach(int id)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            await _datTruocService.XuLyHetHanAsync();

            var datTruoc = await _context.DatTruoc
                .Include(d => d.DocGia)
                .Include(d => d.BanSaoDuocGiu)
                .FirstOrDefaultAsync(d => d.MaDatTruoc == id);

            if (datTruoc == null)
                return NotFound();

            if (datTruoc.TrangThai != TrangThaiDatTruoc.DaCoSach ||
                datTruoc.BanSaoDuocGiu == null ||
                datTruoc.HanNhanSach < DateTime.Now)
            {
                TempData["DatTruocError"] =
                    "Yêu cầu chưa sẵn sàng hoặc đã hết hạn nhận.";
                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            ValidateDocGia(datTruoc.DocGia);
            var eligibilityErrors = await _eligibilityService.KiemTraAsync(
                datTruoc.MaDocGia,
                new KiemTraDocGiaOptions
                {
                    MaSach = datTruoc.MaSach,
                    BoQuaMaDatTruoc = datTruoc.MaDatTruoc
                });
            foreach (var error in eligibilityErrors)
                ModelState.AddModelError(string.Empty, error);

            var userId = _userManager.GetUserId(User);
            var nhanVien = await _context.NhanVien
                .FirstOrDefaultAsync(n => n.UserId == userId);
            if (nhanVien == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Tài khoản hiện tại chưa có hồ sơ nhân viên.");
            }

            if (!ModelState.IsValid)
            {
                await transaction.RollbackAsync();
                TempData["DatTruocError"] = string.Join(
                    " ",
                    ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            var phieuMuon = new PhieuMuon
            {
                MaDocGia = datTruoc.MaDocGia,
                MaNhanVien = nhanVien!.MaNhanVien,
                NgayMuon = DateTime.Now,
                NgayHenTra = DateTime.Today.AddDays(14),
                TrangThai = TrangThaiPhieuMuon.DangMuon
            };

            phieuMuon.ChiTietPhieuMuons.Add(
                new ChiTietPhieuMuon
                {
                    MaBanSao = datTruoc.BanSaoDuocGiu.MaBanSao,
                    GhiChu = "Nhận từ yêu cầu đặt trước #" +
                             datTruoc.MaDatTruoc
                });

            datTruoc.BanSaoDuocGiu.TinhTrang =
                TinhTrangBanSao.DangMuon;
            datTruoc.TrangThai = TrangThaiDatTruoc.HoanThanh;
            _context.PhieuMuon.Add(phieuMuon);

            _context.ThongBao.Add(new ThongBao
            {
                MaDocGia = datTruoc.MaDocGia,
                MaSuKien = $"dat-truoc-received:{datTruoc.MaDatTruoc}",
                TieuDe = "Đã tạo phiếu mượn từ sách đặt trước",
                NoiDung = $"Thư viện đã xác nhận bạn nhận sách và tạo phiếu mượn. Hạn trả là {phieuMuon.NgayHenTra:dd/MM/yyyy}.",
                LienKet = "/Profile/LichSuMuon",
                Loai = "success",
                NgayTao = DateTime.Now
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] =
                "Đã xác nhận nhận sách và tạo phiếu mượn.";
            return RedirectToAction(
                "Details",
                "PhieuMuons",
                new { id = phieuMuon.MaPhieuMuon });
        }

        private async Task<DocGia?> ResolveDocGiaAsync(
            int? requestedId)
        {
            if (IsStaff())
            {
                return requestedId.HasValue
                    ? await _context.DocGia.FindAsync(requestedId.Value)
                    : null;
            }

            var userId = _userManager.GetUserId(User);
            return await _context.DocGia
                .FirstOrDefaultAsync(d => d.UserId == userId);
        }

        private void ValidateDocGia(DocGia? docGia)
        {
            if (docGia == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không tìm thấy hồ sơ độc giả.");
                return;
            }

            if (docGia.TrangThai != TrangThaiDocGia.HoatDong)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Thẻ độc giả đang bị khóa.");
            }

            if (docGia.NgayHetHanThe < DateTime.Today)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Thẻ độc giả đã hết hạn.");
            }
        }

        private async Task<DatTruoc?> LoadDatTruocAsync(int id)
        {
            return await _context.DatTruoc
                .Include(d => d.DocGia)
                .Include(d => d.Sach)
                    .ThenInclude(s => s.TheLoai)
                .Include(d => d.Sach)
                    .ThenInclude(s => s.NhaXuatBan)
                .Include(d => d.BanSaoDuocGiu)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MaDatTruoc == id);
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

            var isStaff = IsStaff();
            DocGia? docGia = null;

            if (isStaff)
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
                    maDocGiaDuocChon);

                if (maDocGiaDuocChon.HasValue)
                {
                    docGia = docGias.FirstOrDefault(d =>
                        d.MaDocGia == maDocGiaDuocChon);
                }
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
                d.TrangThai == TrangThaiDatTruoc.DangCho);

            return new DatTruocViewModel
            {
                MaSach = sach.MaSach,
                MaDocGia = maDocGiaDuocChon ?? docGia?.MaDocGia,
                Sach = sach,
                DocGia = docGia,
                IsStaff = isStaff,
                TongBanSao = sach.BanSaos.Count,
                SoBanSanCo = sach.BanSaos.Count(b =>
                    b.TinhTrang == TinhTrangBanSao.SanCo),
                SoNguoiDangCho = soNguoiDangCho,
                NgayHetHanDuKien = DateTime.Now.AddDays(
                    DatTruocService.SoNgayToiDaTrongHangDoi)
            };
        }

        private bool IsStaff()
        {
            return User.IsInRole("Admin") ||
                   User.IsInRole("NhanVien");
        }
    }
}

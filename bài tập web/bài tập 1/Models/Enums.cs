namespace bài_tập_1.Models
{
    public enum TinhTrangBanSao
    {
        SanCo,      // Sẵn có, có thể cho mượn
        DangMuon,   // Đang được mượn
        HuHong,     // Hư hỏng
        ThanhLy,    // Đã thanh lý, không còn sử dụng
        Mat,        // Bị mất trong quá trình mượn
        DaGiu       // Đang được giữ cho một yêu cầu đặt trước
    }

    public enum TrangThaiDocGia
    {
        HoatDong,
        Khoa
    }

    public enum TrangThaiPhieuMuon
    {
        DangMuon,
        DaTra,
        QuaHan,
        Nhap
    }

    public enum TinhTrangKhiTra
    {
        BinhThuong,
        HuHong,
        Mat
    }

    public enum TrangThaiDatTruoc
    {
        DangCho,        // Đang chờ sách về
        DaCoSach,       // Đã có sách, chờ độc giả đến lấy
        DaHuy,
        HoanThanh,
        HetHan
    }

    public enum LyDoPhat
    {
        TraTre,
        MatSach,
        HuHong
    }

    public enum TrangThaiPhieuPhat
    {
        ChuaDong,
        DaDong,
        DaHuy
    }

    public enum TrangThaiThanhToan
    {
        ChuaThanhToan,
        DaThanhToan
    }

    public enum PhuongThucThanhToan
    {
        KhongXacDinh = 0,
        TienMat = 1,
        ChuyenKhoan = 2
    }

    public enum TrangThaiYeuCauMuonOnline
    {
        ChoNhan = 0,
        DaNhan = 1,
        DaHuy = 2,
        HetHan = 3,
        DaDuyet = 4,
        TuChoi = 5
    }
}

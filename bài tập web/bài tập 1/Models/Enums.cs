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
        QuaHan
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

    public enum TrangThaiYeuCauMuonOnline
    {
        ChoNhan,
        DaNhan,
        DaHuy,
        HetHan
    }
}

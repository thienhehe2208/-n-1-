using bài_tập_1.Models;

namespace bài_tập_1.Models.ViewModels
{
    public class ReaderHeaderViewModel
    {
        public string DisplayName { get; init; } = "Tài khoản";
        public string Email { get; init; } = string.Empty;
        public string Initials { get; init; } = "DG";
        public int UnreadCount { get; init; }
        public IReadOnlyList<ThongBao> Notifications { get; init; } = Array.Empty<ThongBao>();
    }
}

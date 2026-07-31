namespace bài_tập_1.Models.ViewModels
{
    public class PaginationViewModel
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages => Math.Max(1,
            (int)Math.Ceiling(TotalItems / (double)PageSize));
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }

    public static class Pagination
    {
        public const int DefaultPageSize = 12;

        public static int NormalizePage(int page) => Math.Max(1, page);

        public static PaginationViewModel Create(
            int page,
            int totalItems,
            int pageSize = DefaultPageSize)
        {
            var model = new PaginationViewModel
            {
                Page = NormalizePage(page),
                PageSize = pageSize,
                TotalItems = totalItems
            };

            if (model.Page > model.TotalPages)
                model.Page = model.TotalPages;

            return model;
        }
    }
}

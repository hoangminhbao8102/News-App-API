namespace NewsAppApi.Models.DTOs
{
    public class CategoryFilter
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // name|description|id
        public string? SortBy { get; set; } = "name";
        public string SortDir { get; set; } = "asc";

        public string? Keyword { get; set; } // tìm trong Name/Description

        // Optional: theo Id khoảng (vì Category không có CreatedAt)
        public int? IdFrom { get; set; }
        public int? IdTo { get; set; }
    }
}

namespace NewsAppApi.Models.DTOs
{
    public class TagFilter
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // name|id
        public string? SortBy { get; set; } = "name";
        public string SortDir { get; set; } = "asc";

        public string? Keyword { get; set; } // tìm trong Name

        public int? IdFrom { get; set; }
        public int? IdTo { get; set; }
    }
}

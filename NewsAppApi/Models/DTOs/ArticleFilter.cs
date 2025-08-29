namespace NewsAppApi.Models.DTOs
{
    public class ArticleFilter
    {
        // Paging (dùng cho GET danh sách; export sẽ bỏ qua phân trang)
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Sorting: title|createdAt|id
        public string? SortBy { get; set; } = "createdAt";
        public string SortDir { get; set; } = "desc"; // asc|desc

        // Filters
        public int? AuthorId { get; set; }
        public int? CategoryId { get; set; }
        public int? TagId { get; set; }          // lọc theo một Tag cụ thể
        public string? Keyword { get; set; }     // tìm trong Title/Content

        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
}

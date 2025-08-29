namespace NewsAppApi.Models.DTOs
{
    public class UserFilter
    {
        // Paging
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Sorting: fullname|email|role|id|createdAt
        public string? SortBy { get; set; } = "createdAt";
        public string SortDir { get; set; } = "desc"; // asc|desc

        // Search
        public string? Keyword { get; set; }  // tìm trong FullName/Email/Role
        public string? Role { get; set; }     // Admin|User

        // Advanced filters
        public DateTime? CreatedFrom { get; set; } // UTC hoặc local đều được, để server parse
        public DateTime? CreatedTo { get; set; }
    }
}

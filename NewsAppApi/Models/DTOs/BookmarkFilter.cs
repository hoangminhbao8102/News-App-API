namespace NewsAppApi.Models.DTOs
{
    public class BookmarkFilter
    {
        public int? UserId { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
}

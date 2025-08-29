namespace NewsAppApi.Models.DTOs
{
    public class ReadHistoryFilter
    {
        public int? UserId { get; set; }
        public DateTime? ReadFrom { get; set; }
        public DateTime? ReadTo { get; set; }
    }
}

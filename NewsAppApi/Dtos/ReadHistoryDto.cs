public record ReadHistoryDto(int Id, int? UserId, int? ArticleId, DateTime ReadAt);

public class ReadHistoryCreateDto
{
    public int UserId { get; set; }
    public int ArticleId { get; set; }
}

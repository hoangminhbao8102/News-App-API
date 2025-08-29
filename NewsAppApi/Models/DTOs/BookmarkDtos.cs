namespace NewsAppApi.Models.DTOs;

public record BookmarkDto(int Id, int? UserId, int? ArticleId, DateTime CreatedAt);

public class BookmarkCreateDto
{
    public int UserId { get; set; }
    public int ArticleId { get; set; }
}

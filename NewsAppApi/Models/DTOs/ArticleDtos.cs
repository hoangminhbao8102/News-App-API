namespace NewsAppApi.Models.DTOs;

public record ArticleDto(
    int Id,
    string Title,
    string? Content,
    string? ImageUrl,
    int? AuthorId,
    string? AuthorName,
    int? CategoryId,
    string? CategoryName,
    DateTime CreatedAt,
    List<TagDto> Tags
);

public class ArticleCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }
    public int? AuthorId { get; set; }
    public int? CategoryId { get; set; }
    public List<int> TagIds { get; set; } = new();
}

public class ArticleUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? ImageUrl { get; set; }
    public int? CategoryId { get; set; }
    public List<int> TagIds { get; set; } = new();
}

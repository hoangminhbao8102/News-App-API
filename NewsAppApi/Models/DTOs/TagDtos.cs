namespace NewsAppApi.Models.DTOs;

public record TagDto(int Id, string Name);

public class TagCreateDto
{
    public string Name { get; set; } = string.Empty;
}

public class TagUpdateDto
{
    public string Name { get; set; } = string.Empty;
}

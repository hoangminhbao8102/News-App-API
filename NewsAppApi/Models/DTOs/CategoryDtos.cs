namespace NewsAppApi.Models.DTOs;

public record CategoryDto(int Id, string Name, string? Description);

public class CategoryCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CategoryUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

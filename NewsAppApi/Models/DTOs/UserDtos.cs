namespace NewsAppApi.Models.DTOs;

public record UserDto(
    int Id,
    string? FullName,
    string Email,
    string Role,
    DateTime CreatedAt
);

public class UserCreateDto
{
    public string? FullName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // sẽ hash trước khi lưu
    public string Role { get; set; } = "User";
}

public class UserUpdateDto
{
    public string? FullName { get; set; }
    public string? Role { get; set; } // Admin/User
}

public record UserLoginDto(string Email, string Password);

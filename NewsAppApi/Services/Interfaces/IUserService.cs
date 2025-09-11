namespace NewsAppApi.Services.Interfaces
{
    public interface IUserService
    {
        Task<PagedResult<UserDto>> GetPagedAsync(
            int page = 1, int pageSize = 10,
            string? sortBy = null, string sortDir = "desc", string? keyword = null);

        Task<List<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(int id);
        Task<UserDto> CreateAsync(UserCreateDto dto);
        Task<UserDto?> UpdateAsync(int id, UserUpdateDto dto);
        Task<bool> DeleteAsync(int id);

        Task<UserDto?> LoginAsync(UserLoginDto dto); // optional
    }
}

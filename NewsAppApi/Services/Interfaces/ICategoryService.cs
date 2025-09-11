namespace NewsAppApi.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<PagedResult<CategoryDto>> GetPagedAsync(
            int page = 1, int pageSize = 10,
            string? sortBy = null, string sortDir = "asc", string? keyword = null);

        Task<List<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(int id);
        Task<CategoryDto> CreateAsync(CategoryCreateDto dto);
        Task<CategoryDto?> UpdateAsync(int id, CategoryUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}

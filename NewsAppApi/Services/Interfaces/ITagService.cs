using NewsAppApi.Models.DTOs;

namespace NewsAppApi.Services.Interfaces
{
    public interface ITagService
    {
        Task<PagedResult<TagDto>> GetPagedAsync(
            int page = 1, int pageSize = 10,
            string? sortBy = null, string sortDir = "asc", string? keyword = null);
        Task<PagedResult<TagDto>> GetPagedAsync(TagFilter filter);
        Task<string> ExportCsvAsync(TagFilter filter);

        Task<List<TagDto>> GetAllAsync();
        Task<TagDto?> GetByIdAsync(int id);
        Task<TagDto> CreateAsync(TagCreateDto dto);
        Task<TagDto?> UpdateAsync(int id, TagUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}

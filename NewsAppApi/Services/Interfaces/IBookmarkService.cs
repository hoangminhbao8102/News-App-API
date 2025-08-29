using NewsAppApi.Models.DTOs;

namespace NewsAppApi.Services.Interfaces
{
    public interface IBookmarkService
    {
        Task<List<BookmarkDto>> GetAllByUserAsync(int userId);
        Task<BookmarkDto> CreateAsync(BookmarkCreateDto dto);
        Task<bool> DeleteAsync(int id);

        // NEW: Export CSV theo UserId + CreatedFrom/To
        Task<string> ExportCsvAsync(BookmarkFilter filter);
    }
}

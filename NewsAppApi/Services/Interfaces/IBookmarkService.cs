namespace NewsAppApi.Services.Interfaces
{
    public interface IBookmarkService
    {
        Task<List<BookmarkDto>> GetAllByUserAsync(int userId);
        Task<BookmarkDto> CreateAsync(BookmarkCreateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}

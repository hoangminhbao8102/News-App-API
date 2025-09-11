namespace NewsAppApi.Services.Interfaces
{
    public interface IArticleService
    {
        Task<(List<ArticleDto> Items, int Total)> GetPagedAsync(
            int page = 1, int pageSize = 10,
            int? categoryId = null, int? userId = null, int? tagId = null, string? keyword = null);

        Task<List<ArticleDto>> GetAllAsync();
        Task<ArticleDto?> GetByIdAsync(int id);
        Task<ArticleDto> CreateAsync(ArticleCreateDto dto);
        Task<ArticleDto?> UpdateAsync(int id, ArticleUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}

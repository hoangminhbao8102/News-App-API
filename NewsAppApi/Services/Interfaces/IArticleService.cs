using NewsAppApi.Models.DTOs;

namespace NewsAppApi.Services.Interfaces
{
    public interface IArticleService
    {
        Task<(List<ArticleDto> Items, int Total)> GetPagedAsync(
            int page = 1, int pageSize = 10,
            int? categoryId = null, int? authorId = null, int? tagId = null, string? keyword = null);

        Task<ArticleDto?> GetByIdAsync(int id);
        Task<ArticleDto> CreateAsync(ArticleCreateDto dto);
        Task<ArticleDto?> UpdateAsync(int id, ArticleUpdateDto dto);
        Task<bool> DeleteAsync(int id);

        Task<ArticleDto?> AttachTagsAsync(int articleId, List<int> tagIds);
        Task<bool> DetachTagAsync(int articleId, int tagId);

        Task<string> ExportCsvAsync(ArticleFilter filter);
    }
}

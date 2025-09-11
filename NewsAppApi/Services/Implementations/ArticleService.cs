using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NewsAppApi.Core.Entities;
using NewsAppApi.Data.Contexts;
using NewsAppApi.Services.Interfaces;

namespace NewsAppApi.Services.Implementations
{
    public class ArticleService : IArticleService
    {
        private readonly NewsAppDbContext _db;
        private readonly IMapper _mapper;

        public ArticleService(NewsAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<ArticleDto> CreateAsync(ArticleCreateDto dto)
        {
            // Kiểm tra FK tồn tại (nhanh, optional)
            if (dto.UserId.HasValue && !await _db.Users.AnyAsync(u => u.Id == dto.UserId.Value))
                throw new InvalidOperationException("User not found.");
            if (dto.CategoryId.HasValue && !await _db.Categories.AnyAsync(c => c.Id == dto.CategoryId.Value))
                throw new InvalidOperationException("Category not found.");

            var entity = new Article
            {
                Title = dto.Title,
                Content = dto.Content!,
                ImageUrl = dto.ImageUrl,
                UserId = (int)dto.UserId!,
                CategoryId = (int)dto.CategoryId!
            };

            // Gán TagIds
            if (dto.TagIds?.Count > 0)
            {
                var tagIds = await _db.Tags.Where(t => dto.TagIds.Contains(t.Id)).Select(t => t.Id).ToListAsync();
                foreach (var tid in tagIds)
                    entity.ArticleTags.Add(new ArticleTag { TagId = tid, Article = entity });
            }

            _db.Articles.Add(entity);
            await _db.SaveChangesAsync();

            // Include lại để map đủ Tag/User/Category
            var saved = await _db.Articles
                .Include(x => x.User)
                .Include(x => x.Category)
                .Include(x => x.ArticleTags).ThenInclude(t => t.Tag)
                .FirstAsync(x => x.Id == entity.Id);

            return _mapper.Map<ArticleDto>(saved);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _db.Articles.FindAsync(id);
            if (entity is null) return false;
            _db.Articles.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<ArticleDto>> GetAllAsync()
        {
            var articles = await _db.Articles
                .Include(a => a.User)
                .Include(a => a.Category)
                .Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
                .ToListAsync();

            return articles.Select(a => new ArticleDto(
                a.Id,
                a.Title,
                a.Content,
                a.ImageUrl,
                a.UserId,
                a.User?.FullName,
                a.CategoryId,
                a.Category?.Name,
                a.CreatedAt,
                a.ArticleTags.Select(at => new TagDto(at.Tag.Id, at.Tag.Name)).ToList()
            )).ToList();
        }

        public async Task<ArticleDto?> GetByIdAsync(int id)
        {
            var a = await _db.Articles
                .Include(x => x.User)
                .Include(x => x.Category)
                .Include(x => x.ArticleTags).ThenInclude(t => t.Tag)
                .FirstOrDefaultAsync(x => x.Id == id);

            return a is null ? null : _mapper.Map<ArticleDto>(a);
        }

        public async Task<(List<ArticleDto> Items, int Total)> GetPagedAsync(int page = 1, int pageSize = 10, int? categoryId = null, int? userId = null, int? tagId = null, string? keyword = null)
        {
            var query = _db.Articles
                .AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.Category)
                .Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
                .AsQueryable();

            if (categoryId.HasValue) query = query.Where(a => a.CategoryId == categoryId);
            if (userId.HasValue) query = query.Where(a => a.UserId == userId);
            if (tagId.HasValue) query = query.Where(a => a.ArticleTags.Any(t => t.TagId == tagId));
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim();
                query = query.Where(a => a.Title.Contains(k) || (a.Content ?? "").Contains(k));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (_mapper.Map<List<ArticleDto>>(items), total);
        }

        public async Task<ArticleDto?> UpdateAsync(int id, ArticleUpdateDto dto)
        {
            var entity = await _db.Articles
                .Include(x => x.ArticleTags)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (entity is null) return null;

            entity.Title = dto.Title;
            entity.Content = dto.Content!;
            entity.ImageUrl = dto.ImageUrl;
            entity.CategoryId = (int)dto.CategoryId!;

            // Cập nhật TagIds: xóa cũ, thêm mới tối giản
            if (dto.TagIds is not null)
            {
                var newIds = dto.TagIds.Distinct().ToHashSet();
                var currentIds = entity.ArticleTags.Select(t => t.TagId).ToHashSet();

                // remove
                var removeList = entity.ArticleTags.Where(t => !newIds.Contains(t.TagId)).ToList();
                _db.ArticlesTags.RemoveRange(removeList);

                // add
                var addIds = newIds.Except(currentIds).ToList();
                foreach (var tid in addIds)
                {
                    // đảm bảo tag tồn tại
                    if (await _db.Tags.AnyAsync(t => t.Id == tid))
                        entity.ArticleTags.Add(new ArticleTag { ArticleId = entity.Id, TagId = tid });
                }
            }

            await _db.SaveChangesAsync();

            var saved = await _db.Articles
                .Include(x => x.User)
                .Include(x => x.Category)
                .Include(x => x.ArticleTags).ThenInclude(t => t.Tag)
                .FirstAsync(x => x.Id == entity.Id);

            return _mapper.Map<ArticleDto>(saved);
        }
    }
}

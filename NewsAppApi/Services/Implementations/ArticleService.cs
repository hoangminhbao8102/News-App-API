using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NewsAppApi.Data.Contexts;
using NewsAppApi.Models.DTOs;
using NewsAppApi.Models.Entities;
using NewsAppApi.Services.Interfaces;
using NewsAppApi.Utils;
using System.Globalization;

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

        public async Task<(List<ArticleDto> Items, int Total)> GetPagedAsync(
            int page = 1, int pageSize = 10,
            int? categoryId = null, int? authorId = null, int? tagId = null, string? keyword = null)
        {
            var query = _db.Articles
                .AsNoTracking()
                .Include(a => a.Author)
                .Include(a => a.Category)
                .Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
                .AsQueryable();

            if (categoryId.HasValue) query = query.Where(a => a.CategoryId == categoryId);
            if (authorId.HasValue) query = query.Where(a => a.AuthorId == authorId);
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

        public async Task<ArticleDto?> GetByIdAsync(int id)
        {
            var a = await _db.Articles
                .Include(x => x.Author)
                .Include(x => x.Category)
                .Include(x => x.ArticleTags).ThenInclude(t => t.Tag)
                .FirstOrDefaultAsync(x => x.Id == id);

            return a is null ? null : _mapper.Map<ArticleDto>(a);
        }

        public async Task<ArticleDto> CreateAsync(ArticleCreateDto dto)
        {
            // Kiểm tra FK tồn tại (nhanh, optional)
            if (dto.AuthorId.HasValue && !await _db.Users.AnyAsync(u => u.Id == dto.AuthorId.Value))
                throw new InvalidOperationException("Author not found.");
            if (dto.CategoryId.HasValue && !await _db.Categories.AnyAsync(c => c.Id == dto.CategoryId.Value))
                throw new InvalidOperationException("Category not found.");

            var entity = new Article
            {
                Title = dto.Title,
                Content = dto.Content,
                ImageUrl = dto.ImageUrl,
                AuthorId = dto.AuthorId,
                CategoryId = dto.CategoryId
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

            // Include lại để map đủ Tag/Author/Category
            var saved = await _db.Articles
                .Include(x => x.Author)
                .Include(x => x.Category)
                .Include(x => x.ArticleTags).ThenInclude(t => t.Tag)
                .FirstAsync(x => x.Id == entity.Id);

            return _mapper.Map<ArticleDto>(saved);
        }

        public async Task<ArticleDto?> UpdateAsync(int id, ArticleUpdateDto dto)
        {
            var entity = await _db.Articles
                .Include(x => x.ArticleTags)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (entity is null) return null;

            entity.Title = dto.Title;
            entity.Content = dto.Content;
            entity.ImageUrl = dto.ImageUrl;
            entity.CategoryId = dto.CategoryId;

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
                .Include(x => x.Author)
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

        public async Task<ArticleDto?> AttachTagsAsync(int articleId, List<int> tagIds)
        {
            var article = await _db.Articles
                .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
                .Include(a => a.Author)
                .Include(a => a.Category)
                .FirstOrDefaultAsync(a => a.Id == articleId);

            if (article is null) return null;

            var validIds = await _db.Tags
                .Where(t => tagIds.Contains(t.Id))
                .Select(t => t.Id)
                .ToListAsync();

            var current = article.ArticleTags.Select(t => t.TagId).ToHashSet();
            foreach (var tid in validIds)
            {
                if (!current.Contains(tid))
                    article.ArticleTags.Add(new ArticleTag { ArticleId = article.Id, TagId = tid });
            }

            await _db.SaveChangesAsync();

            // load lại tags
            article = await _db.Articles
                .Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
                .Include(a => a.Author)
                .Include(a => a.Category)
                .FirstAsync(a => a.Id == articleId);

            return _mapper.Map<ArticleDto>(article);
        }

        public async Task<bool> DetachTagAsync(int articleId, int tagId)
        {
            var rel = await _db.ArticlesTags.FirstOrDefaultAsync(at => at.ArticleId == articleId && at.TagId == tagId);
            if (rel is null) return false;

            _db.ArticlesTags.Remove(rel);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<string> ExportCsvAsync(ArticleFilter filter)
        {
            var q = _db.Articles
                .AsNoTracking()
                .Include(a => a.Author)
                .Include(a => a.Category)
                .Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
                .AsQueryable();

            // ----- Filters -----
            if (filter.AuthorId.HasValue)
                q = q.Where(a => a.AuthorId == filter.AuthorId.Value);

            if (filter.CategoryId.HasValue)
                q = q.Where(a => a.CategoryId == filter.CategoryId.Value);

            if (filter.TagId.HasValue)
                q = q.Where(a => a.ArticleTags.Any(t => t.TagId == filter.TagId.Value));

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var k = filter.Keyword.Trim();
                q = q.Where(a => a.Title.Contains(k) || (a.Content ?? "").Contains(k));
            }

            if (filter.CreatedFrom.HasValue)
                q = q.Where(a => a.CreatedAt >= filter.CreatedFrom.Value);

            if (filter.CreatedTo.HasValue)
                q = q.Where(a => a.CreatedAt <= filter.CreatedTo.Value);

            // ----- Sorting (mặc định createdAt desc) -----
            var sortBy = (filter.SortBy ?? "createdAt").ToLowerInvariant();
            var desc = filter.SortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);

            q = sortBy switch
            {
                "title" => (desc ? q.OrderByDescending(x => x.Title) : q.OrderBy(x => x.Title)),
                "id" => (desc ? q.OrderByDescending(x => x.Id) : q.OrderBy(x => x.Id)),
                _ => (desc ? q.OrderByDescending(x => x.CreatedAt) : q.OrderBy(x => x.CreatedAt))
            };

            // Export: lấy toàn bộ kết quả (không phân trang)
            var list = await q.ToListAsync();

            // Chuẩn bị cột CSV
            var cols = new List<(string Header, Func<Article, string> Selector)>
            {
                ("Id",        a => a.Id.ToString()),
                ("Title",     a => a.Title),
                ("Author",    a => a.Author?.FullName ?? ""),
                ("Category",  a => a.Category?.Name ?? ""),
                ("CreatedAt", a => a.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)),
                ("Tags",      a => string.Join("|", a.ArticleTags.Select(t => t.Tag.Name))),
                ("ImageUrl",  a => a.ImageUrl ?? "")
                // Nếu muốn thêm "Content" thì cân nhắc dung lượng file CSV; có thể rút gọn:
                // ("Content", a => (a.Content ?? string.Empty).Length > 500 ? (a.Content!.Substring(0,500) + "...") : (a.Content ?? ""))
            };

            return CsvUtil.ToCsv(list, cols);
        }
    }
}

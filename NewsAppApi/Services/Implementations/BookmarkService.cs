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
    public class BookmarkService : IBookmarkService
    {
        private readonly NewsAppDbContext _db;
        private readonly IMapper _mapper;

        public BookmarkService(NewsAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<BookmarkDto>> GetAllByUserAsync(int userId)
        {
            var items = await _db.Bookmarks.AsNoTracking()
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<BookmarkDto>>(items);
        }

        public async Task<BookmarkDto> CreateAsync(BookmarkCreateDto dto)
        {
            // Ngăn bookmark trùng
            var exists = await _db.Bookmarks.AnyAsync(b => b.UserId == dto.UserId && b.ArticleId == dto.ArticleId);
            if (exists) throw new InvalidOperationException("Bookmark already exists.");

            var entity = new Bookmark { UserId = dto.UserId, ArticleId = dto.ArticleId };
            _db.Bookmarks.Add(entity);
            await _db.SaveChangesAsync();
            return _mapper.Map<BookmarkDto>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _db.Bookmarks.FindAsync(id);
            if (entity is null) return false;

            _db.Bookmarks.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<string> ExportCsvAsync(BookmarkFilter filter)
        {
            var q = _db.Bookmarks
                .AsNoTracking()
                .Include(b => b.User)
                .Include(b => b.Article)
                .AsQueryable();

            if (filter.UserId.HasValue)
                q = q.Where(b => b.UserId == filter.UserId.Value);

            if (filter.CreatedFrom.HasValue)
                q = q.Where(b => b.CreatedAt >= filter.CreatedFrom.Value);

            if (filter.CreatedTo.HasValue)
                q = q.Where(b => b.CreatedAt <= filter.CreatedTo.Value);

            var list = await q.OrderBy(b => b.CreatedAt).ToListAsync();

            var cols = new List<(string Header, Func<Bookmark, string> Selector)>
            {
                ("Id",         b => b.Id.ToString()),
                ("UserId",     b => b.UserId?.ToString() ?? ""),
                ("UserName",   b => b.User?.FullName ?? ""),
                ("ArticleId",  b => b.ArticleId?.ToString() ?? ""),
                ("Article",    b => b.Article?.Title ?? ""),
                ("CreatedAt",  b => b.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))
            };

            return CsvUtil.ToCsv(list, cols);
        }
    }
}

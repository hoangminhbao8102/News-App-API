using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NewsAppApi.Core.Entities;
using NewsAppApi.Data.Contexts;
using NewsAppApi.Services.Interfaces;

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

        public async Task<List<BookmarkDto>> GetAllByUserAsync(int userId)
        {
            var items = await _db.Bookmarks.AsNoTracking()
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<BookmarkDto>>(items);
        }
    }
}

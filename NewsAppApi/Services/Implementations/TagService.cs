using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NewsAppApi.Data.Contexts;
using NewsAppApi.Models.DTOs;
using NewsAppApi.Models.Entities;
using NewsAppApi.Services.Interfaces;
using NewsAppApi.Utils;

namespace NewsAppApi.Services.Implementations
{
    public class TagService : ITagService
    {
        private readonly NewsAppDbContext _db;
        private readonly IMapper _mapper;

        public TagService(NewsAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<TagDto>> GetAllAsync()
        {
            var items = await _db.Tags.AsNoTracking().OrderBy(t => t.Name).ToListAsync();
            return _mapper.Map<List<TagDto>>(items);
        }

        public async Task<TagDto?> GetByIdAsync(int id)
        {
            var item = await _db.Tags.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return item is null ? null : _mapper.Map<TagDto>(item);
        }

        public async Task<TagDto> CreateAsync(TagCreateDto dto)
        {
            var entity = new Tag { Name = dto.Name };
            _db.Tags.Add(entity);
            await _db.SaveChangesAsync();
            return _mapper.Map<TagDto>(entity);
        }

        public async Task<TagDto?> UpdateAsync(int id, TagUpdateDto dto)
        {
            var entity = await _db.Tags.FirstOrDefaultAsync(x => x.Id == id);
            if (entity is null) return null;

            entity.Name = dto.Name;
            await _db.SaveChangesAsync();
            return _mapper.Map<TagDto>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _db.Tags.FindAsync(id);
            if (entity is null) return false;
            _db.Tags.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<PagedResult<TagDto>> GetPagedAsync(int page = 1, int pageSize = 10, string? sortBy = null, string sortDir = "asc", string? keyword = null)
        {
            var q = _db.Tags.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim();
                q = q.Where(t => t.Name.Contains(k));
            }

            sortBy = (sortBy ?? "name").ToLowerInvariant();
            var desc = sortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);

            q = sortBy switch
            {
                "id" => (desc ? q.OrderByDescending(x => x.Id) : q.OrderBy(x => x.Id)),
                _ => (desc ? q.OrderByDescending(x => x.Name) : q.OrderBy(x => x.Name))
            };

            var total = await q.CountAsync();
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<TagDto>(page, pageSize, total, _mapper.Map<List<TagDto>>(items));
        }

        public async Task<PagedResult<TagDto>> GetPagedAsync(TagFilter filter)
        {
            var q = _db.Tags.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var k = filter.Keyword.Trim();
                q = q.Where(t => t.Name.Contains(k));
            }
            if (filter.IdFrom.HasValue) q = q.Where(t => t.Id >= filter.IdFrom.Value);
            if (filter.IdTo.HasValue) q = q.Where(t => t.Id <= filter.IdTo.Value);

            var sortBy = (filter.SortBy ?? "name").ToLowerInvariant();
            var desc = filter.SortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);

            q = sortBy switch
            {
                "id" => (desc ? q.OrderByDescending(x => x.Id) : q.OrderBy(x => x.Id)),
                _ => (desc ? q.OrderByDescending(x => x.Name) : q.OrderBy(x => x.Name))
            };

            var total = await q.CountAsync();
            var items = await q
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<TagDto>(filter.Page, filter.PageSize, total, _mapper.Map<List<TagDto>>(items));
        }

        public async Task<string> ExportCsvAsync(TagFilter filter)
        {
            var q = _db.Tags.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var k = filter.Keyword.Trim();
                q = q.Where(t => t.Name.Contains(k));
            }
            if (filter.IdFrom.HasValue) q = q.Where(t => t.Id >= filter.IdFrom.Value);
            if (filter.IdTo.HasValue) q = q.Where(t => t.Id <= filter.IdTo.Value);

            var list = await q.OrderBy(t => t.Id).ToListAsync();

            var cols = new List<(string, Func<Tag, string>)>
            {
                ("Id", t => t.Id.ToString()),
                ("Name", t => t.Name)
            };

            return CsvUtil.ToCsv(list, cols);
        }
    }
}

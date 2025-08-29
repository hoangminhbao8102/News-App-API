using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NewsAppApi.Data.Contexts;
using NewsAppApi.Models.DTOs;
using NewsAppApi.Models.Entities;
using NewsAppApi.Services.Interfaces;
using NewsAppApi.Utils;

namespace NewsAppApi.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly NewsAppDbContext _db;
        private readonly IMapper _mapper;

        public CategoryService(NewsAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {
            var items = await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
            return _mapper.Map<List<CategoryDto>>(items);
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var item = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return item is null ? null : _mapper.Map<CategoryDto>(item);
        }

        public async Task<CategoryDto> CreateAsync(CategoryCreateDto dto)
        {
            var entity = new Category { Name = dto.Name, Description = dto.Description };
            _db.Categories.Add(entity);
            await _db.SaveChangesAsync();
            return _mapper.Map<CategoryDto>(entity);
        }

        public async Task<CategoryDto?> UpdateAsync(int id, CategoryUpdateDto dto)
        {
            var entity = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (entity is null) return null;

            entity.Name = dto.Name;
            entity.Description = dto.Description;
            await _db.SaveChangesAsync();
            return _mapper.Map<CategoryDto>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _db.Categories.FindAsync(id);
            if (entity is null) return false;
            _db.Categories.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<PagedResult<CategoryDto>> GetPagedAsync(int page = 1, int pageSize = 10, string? sortBy = null, string sortDir = "asc", string? keyword = null)
        {
            var q = _db.Categories.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim();
                q = q.Where(c =>
                    c.Name.Contains(k) ||
                    (c.Description ?? "").Contains(k));
            }

            sortBy = (sortBy ?? "name").ToLowerInvariant();
            var desc = sortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);

            q = sortBy switch
            {
                "id" => (desc ? q.OrderByDescending(x => x.Id) : q.OrderBy(x => x.Id)),
                "description" => (desc ? q.OrderByDescending(x => x.Description) : q.OrderBy(x => x.Description)),
                _ => (desc ? q.OrderByDescending(x => x.Name) : q.OrderBy(x => x.Name))
            };

            var total = await q.CountAsync();
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<CategoryDto>(page, pageSize, total, _mapper.Map<List<CategoryDto>>(items));
        }

        public async Task<PagedResult<CategoryDto>> GetPagedAsync(CategoryFilter filter)
        {
            var q = _db.Categories.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var k = filter.Keyword.Trim();
                q = q.Where(c => c.Name.Contains(k) || (c.Description ?? "").Contains(k));
            }
            if (filter.IdFrom.HasValue) q = q.Where(c => c.Id >= filter.IdFrom.Value);
            if (filter.IdTo.HasValue) q = q.Where(c => c.Id <= filter.IdTo.Value);

            var sortBy = (filter.SortBy ?? "name").ToLowerInvariant();
            var desc = filter.SortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);

            q = sortBy switch
            {
                "id" => (desc ? q.OrderByDescending(x => x.Id) : q.OrderBy(x => x.Id)),
                "description" => (desc ? q.OrderByDescending(x => x.Description) : q.OrderBy(x => x.Description)),
                _ => (desc ? q.OrderByDescending(x => x.Name) : q.OrderBy(x => x.Name))
            };

            var total = await q.CountAsync();
            var items = await q
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<CategoryDto>(filter.Page, filter.PageSize, total, _mapper.Map<List<CategoryDto>>(items));
        }

        public async Task<string> ExportCsvAsync(CategoryFilter filter)
        {
            var q = _db.Categories.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var k = filter.Keyword.Trim();
                q = q.Where(c => c.Name.Contains(k) || (c.Description ?? "").Contains(k));
            }
            if (filter.IdFrom.HasValue) q = q.Where(c => c.Id >= filter.IdFrom.Value);
            if (filter.IdTo.HasValue) q = q.Where(c => c.Id <= filter.IdTo.Value);

            var list = await q.OrderBy(c => c.Id).ToListAsync();

            var cols = new List<(string, Func<Category, string>)>
            {
                ("Id", c => c.Id.ToString()),
                ("Name", c => c.Name),
                ("Description", c => c.Description ?? "")
            };

            return CsvUtil.ToCsv(list, cols);
        }
    }
}

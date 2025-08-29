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
    public class UserService : IUserService
    {
        private readonly NewsAppDbContext _db;
        private readonly IMapper _mapper;

        public UserService(NewsAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<UserDto>> GetAllAsync()
        {
            var users = await _db.Users.AsNoTracking().ToListAsync();
            return _mapper.Map<List<UserDto>>(users);
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return user is null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> CreateAsync(UserCreateDto dto)
        {
            // unique email kiểm tra nhanh
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                throw new InvalidOperationException("Email already exists.");

            var entity = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Password = dto.Password,
                Role = (dto.Role == "Admin" || dto.Role == "User") ? dto.Role : "User"
            };

            _db.Users.Add(entity);
            await _db.SaveChangesAsync();
            return _mapper.Map<UserDto>(entity);
        }

        public async Task<UserDto?> UpdateAsync(int id, UserUpdateDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user is null) return null;

            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName;

            if (!string.IsNullOrWhiteSpace(dto.Role))
                user.Role = (dto.Role == "Admin" || dto.Role == "User") ? dto.Role : user.Role;

            await _db.SaveChangesAsync();
            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user is null) return false;

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<UserDto?> LoginAsync(UserLoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);
            if (user is null) return null;

            // So sánh trực tiếp mật khẩu thuần
            if (user.Password == dto.Password)
            {
                return _mapper.Map<UserDto>(user);
            }

            return null;
        }

        public async Task<PagedResult<UserDto>> GetPagedAsync(int page = 1, int pageSize = 10, string? sortBy = null, string sortDir = "desc", string? keyword = null)
        {
            var q = _db.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.Trim();
                q = q.Where(u =>
                    (u.FullName ?? "").Contains(k) ||
                    (u.Email ?? "").Contains(k) ||
                    (u.Role ?? "").Contains(k));
            }

            // sort
            sortBy = (sortBy ?? "createdAt").ToLowerInvariant();
            var desc = sortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);

            q = sortBy switch
            {
                "fullname" => (desc ? q.OrderByDescending(x => x.FullName) : q.OrderBy(x => x.FullName)),
                "email" => (desc ? q.OrderByDescending(x => x.Email) : q.OrderBy(x => x.Email)),
                "role" => (desc ? q.OrderByDescending(x => x.Role) : q.OrderBy(x => x.Role)),
                "id" => (desc ? q.OrderByDescending(x => x.Id) : q.OrderBy(x => x.Id)),
                _ => (desc ? q.OrderByDescending(x => x.CreatedAt) : q.OrderBy(x => x.CreatedAt))
            };

            var total = await q.CountAsync();
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<UserDto>(page, pageSize, total, _mapper.Map<List<UserDto>>(items));
        }

        public async Task<PagedResult<UserDto>> GetPagedAsync(UserFilter filter)
        {
            var q = _db.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var k = filter.Keyword.Trim();
                q = q.Where(u =>
                    (u.FullName ?? "").Contains(k) ||
                    (u.Email ?? "").Contains(k) ||
                    (u.Role ?? "").Contains(k));
            }

            if (!string.IsNullOrWhiteSpace(filter.Role))
                q = q.Where(u => u.Role == filter.Role);

            if (filter.CreatedFrom.HasValue)
                q = q.Where(u => u.CreatedAt >= filter.CreatedFrom.Value);

            if (filter.CreatedTo.HasValue)
                q = q.Where(u => u.CreatedAt <= filter.CreatedTo.Value);

            var sortBy = (filter.SortBy ?? "createdAt").ToLowerInvariant();
            var desc = filter.SortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);

            q = sortBy switch
            {
                "fullname" => (desc ? q.OrderByDescending(x => x.FullName) : q.OrderBy(x => x.FullName)),
                "email" => (desc ? q.OrderByDescending(x => x.Email) : q.OrderBy(x => x.Email)),
                "role" => (desc ? q.OrderByDescending(x => x.Role) : q.OrderBy(x => x.Role)),
                "id" => (desc ? q.OrderByDescending(x => x.Id) : q.OrderBy(x => x.Id)),
                _ => (desc ? q.OrderByDescending(x => x.CreatedAt) : q.OrderBy(x => x.CreatedAt))
            };

            var total = await q.CountAsync();
            var items = await q
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<UserDto>(filter.Page, filter.PageSize, total, _mapper.Map<List<UserDto>>(items));
        }

        public async Task<string> ExportCsvAsync(UserFilter filter)
        {
            // Lấy toàn bộ phù hợp filter, không phân trang
            var q = _db.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var k = filter.Keyword.Trim();
                q = q.Where(u =>
                    (u.FullName ?? "").Contains(k) ||
                    (u.Email ?? "").Contains(k) ||
                    (u.Role ?? "").Contains(k));
            }
            if (!string.IsNullOrWhiteSpace(filter.Role))
                q = q.Where(u => u.Role == filter.Role);
            if (filter.CreatedFrom.HasValue)
                q = q.Where(u => u.CreatedAt >= filter.CreatedFrom.Value);
            if (filter.CreatedTo.HasValue)
                q = q.Where(u => u.CreatedAt <= filter.CreatedTo.Value);

            var list = await q
                .OrderBy(u => u.Id)
                .ToListAsync();

            var cols = new List<(string, Func<User, string>)>
            {
                ("Id", u => u.Id.ToString()),
                ("FullName", u => u.FullName ?? ""),
                ("Email", u => u.Email ?? ""),
                ("Role", u => u.Role ?? ""),
                ("CreatedAt", u => u.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))
            };

            return CsvUtil.ToCsv(list, cols);
        }
    }
}

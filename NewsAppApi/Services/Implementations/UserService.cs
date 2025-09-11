using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NewsAppApi.Core.Entities;
using NewsAppApi.Data.Contexts;
using NewsAppApi.Services.Interfaces;

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

        public async Task<UserDto> CreateAsync(UserCreateDto dto)
        {
            // unique email kiểm tra nhanh
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                throw new InvalidOperationException("Email already exists.");

            var entity = new User
            {
                FullName = dto.FullName!,
                Email = dto.Email,
                PasswordHash = dto.Password,
                Role = (dto.Role == "Admin" || dto.Role == "User") ? dto.Role : "User"
            };

            _db.Users.Add(entity);
            await _db.SaveChangesAsync();
            return _mapper.Map<UserDto>(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user is null) return false;

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return true;
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

        public async Task<UserDto?> LoginAsync(UserLoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);
            if (user is null) return null;

            // So sánh trực tiếp mật khẩu thuần
            if (user.PasswordHash == dto.Password)
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
    }
}

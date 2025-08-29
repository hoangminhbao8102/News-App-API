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
    public class ReadHistoryService : IReadHistoryService
    {
        private readonly NewsAppDbContext _db;
        private readonly IMapper _mapper;

        public ReadHistoryService(NewsAppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<ReadHistoryDto>> GetAllByUserAsync(int userId)
        {
            var items = await _db.ReadHistory.AsNoTracking()
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReadAt)
                .ToListAsync();

            return _mapper.Map<List<ReadHistoryDto>>(items);
        }

        public async Task<ReadHistoryDto> CreateAsync(ReadHistoryCreateDto dto)
        {
            var entity = new ReadHistory { UserId = dto.UserId, ArticleId = dto.ArticleId };
            _db.ReadHistory.Add(entity);
            await _db.SaveChangesAsync();
            return _mapper.Map<ReadHistoryDto>(entity);
        }

        public async Task<string> ExportCsvAsync(ReadHistoryFilter filter)
        {
            var q = _db.ReadHistory
                .AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Article)
                .AsQueryable();

            if (filter.UserId.HasValue)
                q = q.Where(r => r.UserId == filter.UserId.Value);

            if (filter.ReadFrom.HasValue)
                q = q.Where(r => r.ReadAt >= filter.ReadFrom.Value);

            if (filter.ReadTo.HasValue)
                q = q.Where(r => r.ReadAt <= filter.ReadTo.Value);

            var list = await q.OrderBy(r => r.ReadAt).ToListAsync();

            var cols = new List<(string Header, Func<ReadHistory, string> Selector)>
            {
                ("Id",         r => r.Id.ToString()),
                ("UserId",     r => r.UserId?.ToString() ?? ""),
                ("UserName",   r => r.User?.FullName ?? ""),
                ("ArticleId",  r => r.ArticleId?.ToString() ?? ""),
                ("Article",    r => r.Article?.Title ?? ""),
                ("ReadAt",     r => r.ReadAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))
            };

            return CsvUtil.ToCsv(list, cols);
        }
    }
}

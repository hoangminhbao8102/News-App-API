using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NewsAppApi.Core.Entities;
using NewsAppApi.Data.Contexts;
using NewsAppApi.Services.Interfaces;

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

        public async Task<ReadHistoryDto> CreateAsync(ReadHistoryCreateDto dto)
        {
            var entity = new ReadHistory { UserId = dto.UserId, ArticleId = dto.ArticleId };
            _db.ReadHistory.Add(entity);
            await _db.SaveChangesAsync();
            return _mapper.Map<ReadHistoryDto>(entity);
        }

        public async Task<List<ReadHistoryDto>> GetByUserIdAsync(int userId)
        {
            var items = await _db.ReadHistory.AsNoTracking()
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReadAt)
                .ToListAsync();

            return _mapper.Map<List<ReadHistoryDto>>(items);
        }
    }
}

using NewsAppApi.Models.DTOs;

namespace NewsAppApi.Services.Interfaces
{
    public interface IReadHistoryService
    {
        Task<List<ReadHistoryDto>> GetAllByUserAsync(int userId);
        Task<ReadHistoryDto> CreateAsync(ReadHistoryCreateDto dto);

        // NEW: Export CSV theo UserId + ReadFrom/To
        Task<string> ExportCsvAsync(ReadHistoryFilter filter);
    }
}

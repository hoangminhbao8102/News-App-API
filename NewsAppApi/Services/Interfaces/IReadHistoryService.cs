namespace NewsAppApi.Services.Interfaces
{
    public interface IReadHistoryService
    {
        Task<List<ReadHistoryDto>> GetByUserIdAsync(int userId);
        Task<ReadHistoryDto> CreateAsync(ReadHistoryCreateDto dto);
    }
}

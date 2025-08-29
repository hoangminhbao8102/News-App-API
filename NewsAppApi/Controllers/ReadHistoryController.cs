using Microsoft.AspNetCore.Mvc;
using NewsAppApi.Models.DTOs;
using NewsAppApi.Services.Interfaces;
using System.Text;

namespace NewsAppApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReadHistoryController : ControllerBase
    {
        private readonly IReadHistoryService _svc;
        public ReadHistoryController(IReadHistoryService svc) => _svc = svc;

        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetByUser(int userId)
            => Ok(await _svc.GetAllByUserAsync(userId));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReadHistoryCreateDto dto)
            => Ok(await _svc.CreateAsync(dto));

        // NEW: export CSV
        // GET /api/ReadHistory/export?userId=2&readFrom=2025-01-01&readTo=2025-12-31
        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] ReadHistoryFilter filter)
        {
            var csv = await _svc.ExportCsvAsync(filter);
            var fileName = $"readhistory_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
        }
    }
}

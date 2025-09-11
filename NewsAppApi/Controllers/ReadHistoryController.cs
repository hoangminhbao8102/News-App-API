using Microsoft.AspNetCore.Mvc;
using NewsAppApi.Services.Interfaces;

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
            => Ok(await _svc.GetByUserIdAsync(userId));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReadHistoryCreateDto dto)
            => Ok(await _svc.CreateAsync(dto));
    }
}

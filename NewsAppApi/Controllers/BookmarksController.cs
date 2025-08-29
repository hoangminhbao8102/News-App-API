using Microsoft.AspNetCore.Mvc;
using NewsAppApi.Models.DTOs;
using NewsAppApi.Services.Interfaces;
using System.Text;

namespace NewsAppApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookmarksController : ControllerBase
    {
        private readonly IBookmarkService _svc;
        public BookmarksController(IBookmarkService svc) => _svc = svc;

        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetByUser(int userId)
            => Ok(await _svc.GetAllByUserAsync(userId));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BookmarkCreateDto dto)
            => Ok(await _svc.CreateAsync(dto));

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _svc.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }

        // NEW: export CSV
        // GET /api/Bookmarks/export?userId=2&createdFrom=2025-01-01&createdTo=2025-12-31
        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] BookmarkFilter filter)
        {
            var csv = await _svc.ExportCsvAsync(filter);
            var fileName = $"bookmarks_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using NewsAppApi.Services.Interfaces;

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
    }
}

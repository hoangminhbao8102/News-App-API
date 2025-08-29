using Microsoft.AspNetCore.Mvc;
using NewsAppApi.Models.DTOs;
using NewsAppApi.Services.Interfaces;
using System.Text;

namespace NewsAppApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleService _svc;
        public ArticlesController(IArticleService svc) => _svc = svc;

        // GET /api/Articles?page=1&pageSize=10&categoryId=...&authorId=...&tagId=...&keyword=...
        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
            [FromQuery] int? categoryId = null, [FromQuery] int? authorId = null,
            [FromQuery] int? tagId = null, [FromQuery] string? keyword = null)
        {
            var (items, total) = await _svc.GetPagedAsync(page, pageSize, categoryId, authorId, tagId, keyword);
            return Ok(new { total, page, pageSize, items });
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] ArticleFilter filter)
        {
            var csv = await _svc.ExportCsvAsync(filter);
            var fileName = $"articles_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] ArticleFilter filter)
        {
            var (items, total) = await _svc.GetPagedAsync(
                page: filter.Page, pageSize: filter.PageSize,
                categoryId: filter.CategoryId, authorId: filter.AuthorId,
                tagId: filter.TagId, keyword: filter.Keyword);
            return Ok(new { total, filter.Page, filter.PageSize, items });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _svc.GetByIdAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ArticleCreateDto dto)
        {
            var created = await _svc.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // POST /api/Articles/{id}/tags
        [HttpPost("{id:int}/tags")]
        public async Task<IActionResult> AttachTags(int id, [FromBody] List<int> tagIds)
        {
            if (tagIds is null || tagIds.Count == 0) return BadRequest("tagIds is required.");
            var updated = await _svc.AttachTagsAsync(id, tagIds);
            return updated is null ? NotFound() : Ok(updated);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ArticleUpdateDto dto)
        {
            var updated = await _svc.UpdateAsync(id, dto);
            return updated is null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _svc.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }

        // DELETE /api/Articles/{id:int}/tags/{tagId:int}
        [HttpDelete("{id:int}/tags/{tagId:int}")]
        public async Task<IActionResult> DetachTag(int id, int tagId)
        {
            var ok = await _svc.DetachTagAsync(id, tagId);
            return ok ? NoContent() : NotFound();
        }
    }
}

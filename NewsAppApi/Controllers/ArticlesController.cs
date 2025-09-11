using Microsoft.AspNetCore.Mvc;
using NewsAppApi.Services.Interfaces;

namespace NewsAppApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleService _service;

        public ArticlesController(IArticleService service)
        {
            _service = service;
        }

        // GET: api/Articles
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _service.GetAllAsync());

        // GET: api/Articles/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var article = await _service.GetByIdAsync(id);
            return article is null ? NotFound() : Ok(article);
        }

        // GET: api/Articles/search?page=1&pageSize=10...
        [HttpGet("search")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
            [FromQuery] int? categoryId = null, [FromQuery] int? authorId = null,
            [FromQuery] int? tagId = null, [FromQuery] string? keyword = null)
        {
            var (items, total) = await _service.GetPagedAsync(page, pageSize, categoryId, authorId, tagId, keyword);
            return Ok(new { total, page, pageSize, items });
        }

        // POST: api/Articles
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ArticleCreateDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT: api/Articles/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ArticleUpdateDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return updated is null ? NotFound() : Ok(updated);
        }

        // DELETE: api/Articles/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}

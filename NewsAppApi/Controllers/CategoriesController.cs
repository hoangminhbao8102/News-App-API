using Microsoft.AspNetCore.Mvc;
using NewsAppApi.Services.Interfaces;

namespace NewsAppApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _svc;
        public CategoriesController(ICategoryService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _svc.GetByIdAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,        // name|description|id
            [FromQuery] string sortDir = "asc",       // asc|desc
            [FromQuery] string? keyword = null)
        {
            var result = await _svc.GetPagedAsync(page, pageSize, sortBy, sortDir, keyword);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
        {
            var created = await _svc.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateDto dto)
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
    }
}

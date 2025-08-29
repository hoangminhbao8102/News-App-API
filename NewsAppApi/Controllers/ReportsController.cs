using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsAppApi.Data.Contexts;
using NewsAppApi.Models.DTOs;
using NewsAppApi.Services.Interfaces;
using NewsAppApi.Utils;
using System.Globalization;
using System.Text;

namespace NewsAppApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IUserService _userSvc;
        private readonly ICategoryService _catSvc;
        private readonly ITagService _tagSvc;
        private readonly IArticleService _articleSvc;
        private readonly IBookmarkService _bookmarkSvc;
        private readonly IReadHistoryService _readSvc;
        private readonly NewsAppDbContext _db;

        public ReportsController(
            IUserService userSvc,
            ICategoryService catSvc,
            ITagService tagSvc,
            IArticleService articleSvc,
            IBookmarkService bookmarkSvc,
            IReadHistoryService readSvc,
            NewsAppDbContext db)
        {
            _userSvc = userSvc;
            _catSvc = catSvc;
            _tagSvc = tagSvc;
            _articleSvc = articleSvc;
            _bookmarkSvc = bookmarkSvc;
            _readSvc = readSvc;
            _db = db;
        }

        /// <summary>
        /// Export CSV tổng hợp.
        /// Ví dụ:
        ///  - /api/Reports/export?type=users&role=Admin&createdFrom=2025-01-01
        ///  - /api/Reports/export?type=categories&keyword=tech
        ///  - /api/Reports/export?type=tags&keyword=ai
        ///  - /api/Reports/export?type=articles&authorId=2&tagId=1&createdFrom=2025-01-01&sortBy=title&sortDir=asc
        ///  - /api/Reports/export?type=bookmarks&userId=3&createdFrom=2025-06-01&createdTo=2025-06-30
        ///  - /api/Reports/export?type=readhistory&userId=2&readFrom=2025-01-01&readTo=2025-12-31
        /// </summary>
        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return BadRequest("Missing required 'type' query. Supported: users, categories, tags, articles, bookmarks, readhistory.");

            type = type.Trim().ToLowerInvariant();
            string csv;
            string fileBase;

            switch (type)
            {
                case "users":
                    {
                        var f = BuildUserFilter(Request.Query);
                        // Validate khoảng thời gian đơn giản
                        if (f.CreatedFrom.HasValue && f.CreatedTo.HasValue && f.CreatedFrom > f.CreatedTo)
                            return BadRequest("CreatedFrom must be <= CreatedTo");
                        csv = await _userSvc.ExportCsvAsync(f);
                        fileBase = "users";
                        break;
                    }
                case "categories":
                    {
                        var f = BuildCategoryFilter(Request.Query);
                        csv = await _catSvc.ExportCsvAsync(f);
                        fileBase = "categories";
                        break;
                    }
                case "tags":
                    {
                        var f = BuildTagFilter(Request.Query);
                        csv = await _tagSvc.ExportCsvAsync(f);
                        fileBase = "tags";
                        break;
                    }
                case "articles":
                    {
                        var f = BuildArticleFilter(Request.Query);
                        if (f.CreatedFrom.HasValue && f.CreatedTo.HasValue && f.CreatedFrom > f.CreatedTo)
                            return BadRequest("CreatedFrom must be <= CreatedTo");
                        csv = await _articleSvc.ExportCsvAsync(f);
                        fileBase = "articles";
                        break;
                    }
                case "bookmarks":
                    {
                        var f = BuildBookmarkFilter(Request.Query);
                        if (f.CreatedFrom.HasValue && f.CreatedTo.HasValue && f.CreatedFrom > f.CreatedTo)
                            return BadRequest("CreatedFrom must be <= CreatedTo");
                        csv = await _bookmarkSvc.ExportCsvAsync(f);
                        fileBase = "bookmarks";
                        break;
                    }
                case "readhistory":
                    {
                        var f = BuildReadHistoryFilter(Request.Query);
                        if (f.ReadFrom.HasValue && f.ReadTo.HasValue && f.ReadFrom > f.ReadTo)
                            return BadRequest("ReadFrom must be <= ReadTo");
                        csv = await _readSvc.ExportCsvAsync(f);
                        fileBase = "readhistory";
                        break;
                    }
                default:
                    return BadRequest("Unsupported 'type'. Use one of: users, categories, tags, articles, bookmarks, readhistory.");
            }

            var fileName = $"{fileBase}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
        }

        // =========================
        //  A) Export EXCEL (.xlsx)
        // =========================
        // Ví dụ:
        // /api/Reports/export-excel?types=users,categories,tags,articles,bookmarks,readhistory
        // Kèm filter như CSV (role, createdFrom/To, authorId, categoryId, tagId, ...)
        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportExcel([FromQuery] string? types = null)
        {
            var want = (types ?? "users,categories,tags,articles,bookmarks,readhistory")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => s.ToLowerInvariant())
                .ToHashSet();

            using var wb = new XLWorkbook();

            if (want.Contains("users"))
            {
                var f = BuildUserFilter(Request.Query);
                await WriteUsersSheetAsync(wb, f);
            }
            if (want.Contains("categories"))
            {
                var f = BuildCategoryFilter(Request.Query);
                await WriteCategoriesSheetAsync(wb, f);
            }
            if (want.Contains("tags"))
            {
                var f = BuildTagFilter(Request.Query);
                await WriteTagsSheetAsync(wb, f);
            }
            if (want.Contains("articles"))
            {
                var f = BuildArticleFilter(Request.Query);
                await WriteArticlesSheetAsync(wb, f);
            }
            if (want.Contains("bookmarks"))
            {
                var f = BuildBookmarkFilter(Request.Query);
                await WriteBookmarksSheetAsync(wb, f);
            }
            if (want.Contains("readhistory"))
            {
                var f = BuildReadHistoryFilter(Request.Query);
                await WriteReadHistorySheetAsync(wb, f);
            }

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            var bytes = ms.ToArray();
            var fileName = $"reports_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        // =========================
        //  B) Export ZIP (nhiều CSV)
        // =========================
        // Ví dụ:
        // /api/Reports/export-zip?types=users,articles,bookmarks
        // -> Zip chứa users_*.csv, articles_*.csv, bookmarks_*.csv
        [HttpGet("export-zip")]
        public async Task<IActionResult> ExportZip([FromQuery] string? types = null)
        {
            var want = (types ?? "users,categories,tags,articles,bookmarks,readhistory")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => s.ToLowerInvariant())
                .ToHashSet();

            var files = new Dictionary<string, byte[]>();

            if (want.Contains("users"))
            {
                var f = BuildUserFilter(Request.Query);
                var csv = await _userSvc.ExportCsvAsync(f);
                files.Add($"users_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv", Encoding.UTF8.GetBytes(csv));
            }
            if (want.Contains("categories"))
            {
                var f = BuildCategoryFilter(Request.Query);
                var csv = await _catSvc.ExportCsvAsync(f);
                files.Add($"categories_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv", Encoding.UTF8.GetBytes(csv));
            }
            if (want.Contains("tags"))
            {
                var f = BuildTagFilter(Request.Query);
                var csv = await _tagSvc.ExportCsvAsync(f);
                files.Add($"tags_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv", Encoding.UTF8.GetBytes(csv));
            }
            if (want.Contains("articles"))
            {
                var f = BuildArticleFilter(Request.Query);
                var csv = await _articleSvc.ExportCsvAsync(f);
                files.Add($"articles_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv", Encoding.UTF8.GetBytes(csv));
            }
            if (want.Contains("bookmarks"))
            {
                var f = BuildBookmarkFilter(Request.Query);
                var csv = await _bookmarkSvc.ExportCsvAsync(f);
                files.Add($"bookmarks_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv", Encoding.UTF8.GetBytes(csv));
            }
            if (want.Contains("readhistory"))
            {
                var f = BuildReadHistoryFilter(Request.Query);
                var csv = await _readSvc.ExportCsvAsync(f);
                files.Add($"readhistory_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv", Encoding.UTF8.GetBytes(csv));
            }

            if (files.Count == 0)
                return BadRequest("No valid report 'types' specified.");

            var zipBytes = ZipUtil.CreateZip(files);
            var zipName = $"reports_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip";
            return File(zipBytes, "application/zip", zipName);
        }

        // ------------------------
        // Helpers: build filters
        // ------------------------
        private static UserFilter BuildUserFilter(IQueryCollection q)
        {
            return new UserFilter
            {
                // paging không dùng trong export nhưng không sao nếu có
                Page = GetInt(q, "page") ?? 1,
                PageSize = GetInt(q, "pageSize") ?? 10,
                SortBy = GetString(q, "sortBy") ?? "createdAt",
                SortDir = GetString(q, "sortDir") ?? "desc",
                Keyword = GetString(q, "keyword"),
                Role = GetString(q, "role"),
                CreatedFrom = GetDate(q, "createdFrom"),
                CreatedTo = GetDate(q, "createdTo")
            };
        }

        private static CategoryFilter BuildCategoryFilter(IQueryCollection q)
        {
            return new CategoryFilter
            {
                Page = GetInt(q, "page") ?? 1,
                PageSize = GetInt(q, "pageSize") ?? 10,
                SortBy = GetString(q, "sortBy") ?? "name",
                SortDir = GetString(q, "sortDir") ?? "asc",
                Keyword = GetString(q, "keyword"),
                IdFrom = GetInt(q, "idFrom"),
                IdTo = GetInt(q, "idTo")
            };
        }

        private static TagFilter BuildTagFilter(IQueryCollection q)
        {
            return new TagFilter
            {
                Page = GetInt(q, "page") ?? 1,
                PageSize = GetInt(q, "pageSize") ?? 10,
                SortBy = GetString(q, "sortBy") ?? "name",
                SortDir = GetString(q, "sortDir") ?? "asc",
                Keyword = GetString(q, "keyword"),
                IdFrom = GetInt(q, "idFrom"),
                IdTo = GetInt(q, "idTo")
            };
        }

        private static ArticleFilter BuildArticleFilter(IQueryCollection q)
        {
            return new ArticleFilter
            {
                Page = GetInt(q, "page") ?? 1,
                PageSize = GetInt(q, "pageSize") ?? 10,
                SortBy = GetString(q, "sortBy") ?? "createdAt",
                SortDir = GetString(q, "sortDir") ?? "desc",
                AuthorId = GetInt(q, "authorId"),
                CategoryId = GetInt(q, "categoryId"),
                TagId = GetInt(q, "tagId"),
                Keyword = GetString(q, "keyword"),
                CreatedFrom = GetDate(q, "createdFrom"),
                CreatedTo = GetDate(q, "createdTo")
            };
        }

        private static BookmarkFilter BuildBookmarkFilter(IQueryCollection q)
        {
            return new BookmarkFilter
            {
                UserId = GetInt(q, "userId"),
                CreatedFrom = GetDate(q, "createdFrom"),
                CreatedTo = GetDate(q, "createdTo")
            };
        }

        private static ReadHistoryFilter BuildReadHistoryFilter(IQueryCollection q)
        {
            return new ReadHistoryFilter
            {
                UserId = GetInt(q, "userId"),
                ReadFrom = GetDate(q, "readFrom"),
                ReadTo = GetDate(q, "readTo")
            };
        }

        // ------------------------
        // Parsing helpers
        // ------------------------
        private static string? GetString(IQueryCollection q, string key)
            => q.TryGetValue(key, out var v) ? v.ToString() : null;

        private static int? GetInt(IQueryCollection q, string key)
            => q.TryGetValue(key, out var v) && int.TryParse(v.ToString(), out var i) ? i : null;

        private static DateTime? GetDate(IQueryCollection q, string key)
        {
            if (!q.TryGetValue(key, out var v)) return null;
            var s = v.ToString();
            if (string.IsNullOrWhiteSpace(s)) return null;

            // Hỗ trợ ISO (yyyy-MM-dd hoặc yyyy-MM-ddTHH:mm:ss)
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
                return dt;

            // Thử yyyy-MM-dd
            if (DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out dt))
                return dt;

            return null; // để controller không nổ; bạn có thể BadRequest nếu cần strict
        }

        // ===========================================================
        // ============== Sheet writers (ClosedXML) ==================
        // ===========================================================
        private async Task WriteUsersSheetAsync(XLWorkbook wb, UserFilter filter)
        {
            var ws = wb.Worksheets.Add("Users");
            // Header
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "FullName";
            ws.Cell(1, 3).Value = "Email";
            ws.Cell(1, 4).Value = "Role";
            ws.Cell(1, 5).Value = "CreatedAt";

            var q = _db.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var k = filter.Keyword.Trim();
                q = q.Where(u => (u.FullName ?? "").Contains(k) || (u.Email ?? "").Contains(k) || (u.Role ?? "").Contains(k));
            }
            if (!string.IsNullOrWhiteSpace(filter.Role))
                q = q.Where(u => u.Role == filter.Role);
            if (filter.CreatedFrom.HasValue)
                q = q.Where(u => u.CreatedAt >= filter.CreatedFrom.Value);
            if (filter.CreatedTo.HasValue)
                q = q.Where(u => u.CreatedAt <= filter.CreatedTo.Value);

            var list = await q.OrderBy(u => u.Id).ToListAsync();

            var r = 2;
            foreach (var u in list)
            {
                ws.Cell(r, 1).Value = u.Id;
                ws.Cell(r, 2).Value = u.FullName ?? "";
                ws.Cell(r, 3).Value = u.Email ?? "";
                ws.Cell(r, 4).Value = u.Role ?? "";
                ws.Cell(r, 5).Value = u.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                r++;
            }
            ws.Columns().AdjustToContents();
        }

        private async Task WriteCategoriesSheetAsync(XLWorkbook wb, CategoryFilter filter)
        {
            var ws = wb.Worksheets.Add("Categories");
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "Name";
            ws.Cell(1, 3).Value = "Description";

            var q = _db.Categories.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var k = filter.Keyword.Trim();
                q = q.Where(c => c.Name.Contains(k) || (c.Description ?? "").Contains(k));
            }
            if (filter.IdFrom.HasValue) q = q.Where(c => c.Id >= filter.IdFrom.Value);
            if (filter.IdTo.HasValue) q = q.Where(c => c.Id <= filter.IdTo.Value);

            var list = await q.OrderBy(c => c.Id).ToListAsync();

            var r = 2;
            foreach (var c in list)
            {
                ws.Cell(r, 1).Value = c.Id;
                ws.Cell(r, 2).Value = c.Name;
                ws.Cell(r, 3).Value = c.Description ?? "";
                r++;
            }
            ws.Columns().AdjustToContents();
        }

        private async Task WriteTagsSheetAsync(XLWorkbook wb, TagFilter filter)
        {
            var ws = wb.Worksheets.Add("Tags");
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "Name";

            var q = _db.Tags.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var k = filter.Keyword.Trim();
                q = q.Where(t => t.Name.Contains(k));
            }
            if (filter.IdFrom.HasValue) q = q.Where(t => t.Id >= filter.IdFrom.Value);
            if (filter.IdTo.HasValue) q = q.Where(t => t.Id <= filter.IdTo.Value);

            var list = await q.OrderBy(t => t.Id).ToListAsync();

            var r = 2;
            foreach (var t in list)
            {
                ws.Cell(r, 1).Value = t.Id;
                ws.Cell(r, 2).Value = t.Name;
                r++;
            }
            ws.Columns().AdjustToContents();
        }

        private async Task WriteArticlesSheetAsync(XLWorkbook wb, ArticleFilter filter)
        {
            var ws = wb.Worksheets.Add("Articles");
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "Title";
            ws.Cell(1, 3).Value = "Author";
            ws.Cell(1, 4).Value = "Category";
            ws.Cell(1, 5).Value = "CreatedAt";
            ws.Cell(1, 6).Value = "Tags";
            ws.Cell(1, 7).Value = "ImageUrl";

            var q = _db.Articles.AsNoTracking()
                .Include(a => a.Author)
                .Include(a => a.Category)
                .Include(a => a.ArticleTags).ThenInclude(at => at.Tag)
                .AsQueryable();

            if (filter.AuthorId.HasValue)
                q = q.Where(a => a.AuthorId == filter.AuthorId.Value);
            if (filter.CategoryId.HasValue)
                q = q.Where(a => a.CategoryId == filter.CategoryId.Value);
            if (filter.TagId.HasValue)
                q = q.Where(a => a.ArticleTags.Any(t => t.TagId == filter.TagId.Value));
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var k = filter.Keyword.Trim();
                q = q.Where(a => a.Title.Contains(k) || (a.Content ?? "").Contains(k));
            }
            if (filter.CreatedFrom.HasValue)
                q = q.Where(a => a.CreatedAt >= filter.CreatedFrom.Value);
            if (filter.CreatedTo.HasValue)
                q = q.Where(a => a.CreatedAt <= filter.CreatedTo.Value);

            var list = await q.OrderByDescending(a => a.CreatedAt).ToListAsync();

            var r = 2;
            foreach (var a in list)
            {
                ws.Cell(r, 1).Value = a.Id;
                ws.Cell(r, 2).Value = a.Title;
                ws.Cell(r, 3).Value = a.Author?.FullName ?? "";
                ws.Cell(r, 4).Value = a.Category?.Name ?? "";
                ws.Cell(r, 5).Value = a.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                ws.Cell(r, 6).Value = string.Join("|", a.ArticleTags.Select(t => t.Tag.Name));
                ws.Cell(r, 7).Value = a.ImageUrl ?? "";
                r++;
            }
            ws.Columns().AdjustToContents();
        }

        private async Task WriteBookmarksSheetAsync(XLWorkbook wb, BookmarkFilter filter)
        {
            var ws = wb.Worksheets.Add("Bookmarks");
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "UserId";
            ws.Cell(1, 3).Value = "UserName";
            ws.Cell(1, 4).Value = "ArticleId";
            ws.Cell(1, 5).Value = "Article";
            ws.Cell(1, 6).Value = "CreatedAt";

            var q = _db.Bookmarks.AsNoTracking()
                .Include(b => b.User)
                .Include(b => b.Article)
                .AsQueryable();

            if (filter.UserId.HasValue)
                q = q.Where(b => b.UserId == filter.UserId.Value);
            if (filter.CreatedFrom.HasValue)
                q = q.Where(b => b.CreatedAt >= filter.CreatedFrom.Value);
            if (filter.CreatedTo.HasValue)
                q = q.Where(b => b.CreatedAt <= filter.CreatedTo.Value);

            var list = await q.OrderBy(b => b.CreatedAt).ToListAsync();

            var r = 2;
            foreach (var b in list)
            {
                ws.Cell(r, 1).Value = b.Id;
                ws.Cell(r, 2).Value = b.UserId?.ToString() ?? "";
                ws.Cell(r, 3).Value = b.User?.FullName ?? "";
                ws.Cell(r, 4).Value = b.ArticleId?.ToString() ?? "";
                ws.Cell(r, 5).Value = b.Article?.Title ?? "";
                ws.Cell(r, 6).Value = b.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                r++;
            }
            ws.Columns().AdjustToContents();
        }

        private async Task WriteReadHistorySheetAsync(XLWorkbook wb, ReadHistoryFilter filter)
        {
            var ws = wb.Worksheets.Add("ReadHistory");
            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "UserId";
            ws.Cell(1, 3).Value = "UserName";
            ws.Cell(1, 4).Value = "ArticleId";
            ws.Cell(1, 5).Value = "Article";
            ws.Cell(1, 6).Value = "ReadAt";

            var q = _db.ReadHistory.AsNoTracking()
                .Include(r => r.User)
                .Include(r => r.Article)
                .AsQueryable();

            if (filter.UserId.HasValue)
                q = q.Where(r => r.UserId == filter.UserId.Value);
            if (filter.ReadFrom.HasValue)
                q = q.Where(r => r.ReadAt >= filter.ReadFrom.Value);
            if (filter.ReadTo.HasValue)
                q = q.Where(r => r.ReadAt <= filter.ReadTo.Value);

            var list = await q.OrderBy(r => r.ReadAt).ToListAsync();

            var r = 2;
            foreach (var h in list)
            {
                ws.Cell(r, 1).Value = h.Id;
                ws.Cell(r, 2).Value = h.UserId?.ToString() ?? "";
                ws.Cell(r, 3).Value = h.User?.FullName ?? "";
                ws.Cell(r, 4).Value = h.ArticleId?.ToString() ?? "";
                ws.Cell(r, 5).Value = h.Article?.Title ?? "";
                ws.Cell(r, 6).Value = h.ReadAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                r++;
            }
            ws.Columns().AdjustToContents();
        }
    }
}

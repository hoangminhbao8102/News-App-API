using System.ComponentModel.DataAnnotations.Schema;

namespace NewsAppApi.Core.Entities
{
    [Table("ArticlesTags")]
    public class ArticleTag
    {
        // PK tổng hợp (ArticleId, TagId) -> cấu hình ở DbContext.OnModelCreating
        public int ArticleId { get; set; }
        public int TagId { get; set; }

        // Navigation
        public Article Article { get; set; } = default!;
        public Tag Tag { get; set; } = default!;
    }
}

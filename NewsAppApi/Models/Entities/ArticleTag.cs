using System.ComponentModel.DataAnnotations.Schema;

namespace NewsAppApi.Models.Entities
{
    [Table("ArticlesTags")]
    public class ArticleTag
    {
        // PK tổng hợp (ArticleId, TagId) -> cấu hình ở DbContext.OnModelCreating
        public int ArticleId { get; set; }
        public int TagId { get; set; }

        // Navigation
        public Article Article { get; set; } = null!;
        public Tag Tag { get; set; } = null!;
    }
}

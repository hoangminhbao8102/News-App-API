using NewsAppApi.Core.Contracts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsAppApi.Core.Entities
{
    [Table("Tags")]
    public class Tag : IEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = default!;

        // Navigation
        public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
    }
}

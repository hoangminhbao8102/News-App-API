using NewsAppApi.Models.Contracts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsAppApi.Models.Entities
{
    [Table("Articles")]
    public class Article : IEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        public string? Content { get; set; }

        [MaxLength(255)]
        public string? ImageUrl { get; set; }

        // FK: Users(Id)
        [ForeignKey(nameof(Author))]
        public int? AuthorId { get; set; }
        public User? Author { get; set; }

        // FK: Categories(Id)
        [ForeignKey(nameof(Category))]
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
        public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
        public ICollection<ReadHistory> ReadHistories { get; set; } = new List<ReadHistory>();
    }
}

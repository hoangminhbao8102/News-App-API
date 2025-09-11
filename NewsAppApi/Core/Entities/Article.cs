using NewsAppApi.Core.Contracts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsAppApi.Core.Entities
{
    [Table("Articles")]
    public class Article : IEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = default!;

        public string Content { get; set; } = default!;

        [MaxLength(255)]
        public string? ImageUrl { get; set; }

        // FK: Users(Id)
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public User User { get; set; } = default!;

        // FK: Categories(Id)
        [ForeignKey(nameof(Category))]
        public int CategoryId { get; set; }
        public Category Category { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<ArticleTag> ArticleTags { get; set; } = new List<ArticleTag>();
        public ICollection<ReadHistory> ReadHistories { get; set; } = new List<ReadHistory>();
        public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    }
}

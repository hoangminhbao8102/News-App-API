using NewsAppApi.Models.Contracts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsAppApi.Models.Entities
{
    [Table("Bookmarks")]
    public class Bookmark : IEntity
    {
        [Key]
        public int Id { get; set; }

        // FK: Users(Id)
        [ForeignKey(nameof(User))]
        public int? UserId { get; set; }
        public User? User { get; set; }

        // FK: Articles(Id)
        [ForeignKey(nameof(Article))]
        public int? ArticleId { get; set; }
        public Article? Article { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

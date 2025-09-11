using NewsAppApi.Core.Contracts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsAppApi.Core.Entities
{
    [Table("ReadHistory")]
    public class ReadHistory : IEntity
    {
        [Key]
        public int Id { get; set; }

        // FK: Users(Id)
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public User User { get; set; } = default!;

        // FK: Articles(Id)
        [ForeignKey(nameof(Article))]
        public int ArticleId { get; set; }
        public Article Article { get; set; } = default!;

        public DateTime ReadAt { get; set; } = DateTime.UtcNow;
    }
}

using NewsAppApi.Core.Contracts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsAppApi.Core.Entities
{
    [Table("Categories")]
    public class Category : IEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = default!;

        [MaxLength(255)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Article> Articles { get; set; } = new List<Article>();
    }
}

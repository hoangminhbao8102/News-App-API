using Microsoft.EntityFrameworkCore;
using NewsAppApi.Models.Contracts;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsAppApi.Models.Entities
{
    [Table("Users")]
    [Index(nameof(Email), IsUnique = true)]
    public class User : IEntity
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string? FullName { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(255)]
        public string? Password { get; set; }

        /// <summary>
        /// 'Admin' hoặc 'User' (theo CHECK constraint)
        /// </summary>
        [MaxLength(20)]
        public string Role { get; set; } = "User";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Article> AuthoredArticles { get; set; } = new List<Article>();
        public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
        public ICollection<ReadHistory> ReadHistories { get; set; } = new List<ReadHistory>();
    }
}

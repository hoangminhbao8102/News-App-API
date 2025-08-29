using Microsoft.EntityFrameworkCore;
using NewsAppApi.Models.Entities;

namespace NewsAppApi.Data.Contexts
{
    public class NewsAppDbContext : DbContext
    {
        public NewsAppDbContext(DbContextOptions<NewsAppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Article> Articles => Set<Article>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<ArticleTag> ArticlesTags => Set<ArticleTag>();
        public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
        public DbSet<ReadHistory> ReadHistory => Set<ReadHistory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // PK tổng hợp cho bảng nối
            modelBuilder.Entity<ArticleTag>()
                .HasKey(at => new { at.ArticleId, at.TagId });

            // Quan hệ Article <-> ArticleTag
            modelBuilder.Entity<ArticleTag>()
                .HasOne(at => at.Article)
                .WithMany(a => a.ArticleTags)
                .HasForeignKey(at => at.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ Tag <-> ArticleTag
            modelBuilder.Entity<ArticleTag>()
                .HasOne(at => at.Tag)
                .WithMany(t => t.ArticleTags)
                .HasForeignKey(at => at.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique Email đã khai báo bằng [Index] trên entity User.
            // Nếu muốn default DateTime do DB cấp (GETUTCDATE()), có thể bỏ set mặc định ở entity
            // và dùng HasDefaultValueSql("GETUTCDATE()") như dưới:

            // Default GETUTCDATE() từ SQL Server
            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<Article>()
                .Property(a => a.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<Bookmark>()
                .Property(b => b.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<ReadHistory>()
                .Property(r => r.ReadAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // ============================
            // Seed Users
            // ============================
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, FullName = "Admin User", Email = "admin@example.com", Password = "admin", Role = "Admin", CreatedAt = new DateTime(2025, 08, 01, 0, 0, 0, DateTimeKind.Utc) },
                new User { Id = 2, FullName = "Alice Nguyen", Email = "alice@example.com", Password = "alice-nguyen", Role = "User", CreatedAt = new DateTime(2025, 08, 01, 0, 0, 0, DateTimeKind.Utc) },
                new User { Id = 3, FullName = "Bob Tran", Email = "bob@example.com", Password = "bob-tran", Role = "User", CreatedAt = new DateTime(2025, 08, 01, 0, 0, 0, DateTimeKind.Utc) }
            );

            // ============================
            // Seed Categories
            // ============================
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Technology", Description = "Tin tức công nghệ" },
                new Category { Id = 2, Name = "Sports", Description = "Thể thao" },
                new Category { Id = 3, Name = "Lifestyle", Description = "Đời sống" }
            );

            // ============================
            // Seed Tags
            // ============================
            modelBuilder.Entity<Tag>().HasData(
                new Tag { Id = 1, Name = "AI" },
                new Tag { Id = 2, Name = "Cloud" },
                new Tag { Id = 3, Name = "Football" },
                new Tag { Id = 4, Name = "Health" }
            );

            // ============================
            // Seed Articles
            // ============================
            modelBuilder.Entity<Article>().HasData(
                new Article
                {
                    Id = 1,
                    Title = "AI đang thay đổi thế giới",
                    Content = "Nội dung bài viết về AI...",
                    ImageUrl = "/images/ai.png",
                    AuthorId = 1, // Admin
                    CategoryId = 1, // Technology
                    CreatedAt = new DateTime(2025, 08, 01, 0, 0, 0, DateTimeKind.Utc)
                },
                new Article
                {
                    Id = 2,
                    Title = "Chung kết bóng đá quốc gia",
                    Content = "Nội dung thể thao...",
                    ImageUrl = "/images/football.png",
                    AuthorId = 2, // Alice
                    CategoryId = 2, // Sports
                    CreatedAt = new DateTime(2025, 08, 01, 0, 0, 0, DateTimeKind.Utc)
                },
                new Article
                {
                    Id = 3,
                    Title = "Sống khỏe mỗi ngày",
                    Content = "Mẹo sống khỏe...",
                    ImageUrl = "/images/health.png",
                    AuthorId = 3, // Bob
                    CategoryId = 3, // Lifestyle
                    CreatedAt = new DateTime(2025, 08, 01, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // ============================
            // Seed ArticlesTags (n-n)
            // ============================
            modelBuilder.Entity<ArticleTag>().HasData(
                new { ArticleId = 1, TagId = 1 }, // AI
                new { ArticleId = 1, TagId = 2 }, // Cloud
                new { ArticleId = 2, TagId = 3 }, // Football
                new { ArticleId = 3, TagId = 4 }  // Health
            );

            // ============================
            // Seed Bookmarks
            // ============================
            modelBuilder.Entity<Bookmark>().HasData(
                new Bookmark { Id = 1, UserId = 2, ArticleId = 1, CreatedAt = new DateTime(2025, 08, 01, 0, 0, 0, DateTimeKind.Utc) }, // Alice bookmark AI
                new Bookmark { Id = 2, UserId = 3, ArticleId = 2, CreatedAt = new DateTime(2025, 08, 01, 0, 0, 0, DateTimeKind.Utc) }  // Bob bookmark Football
            );

            // ============================
            // Seed ReadHistory
            // ============================
            modelBuilder.Entity<ReadHistory>().HasData(
                new ReadHistory { Id = 1, UserId = 2, ArticleId = 1, ReadAt = new DateTime(2025, 08, 01, 0, 0, 0, DateTimeKind.Utc) },
                new ReadHistory { Id = 2, UserId = 2, ArticleId = 2, ReadAt = new DateTime(2025, 08, 01, 0, 0, 0, DateTimeKind.Utc) },
                new ReadHistory { Id = 3, UserId = 3, ArticleId = 3, ReadAt = new DateTime(2025, 08, 01, 0, 0, 0, DateTimeKind.Utc) }
            );
        }
    }
}

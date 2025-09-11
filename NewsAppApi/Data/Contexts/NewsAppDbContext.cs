using Microsoft.EntityFrameworkCore;
using NewsAppApi.Core.Entities;

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

            // Quan hệ Bookmark <-> User và Article (tránh multiple cascade paths)
            modelBuilder.Entity<Bookmark>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookmarks)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict); // hoặc .NoAction

            modelBuilder.Entity<Bookmark>()
                .HasOne(b => b.Article)
                .WithMany(a => a.Bookmarks)
                .HasForeignKey(b => b.ArticleId)
                .OnDelete(DeleteBehavior.Cascade); // vẫn giữ cascade nếu muốn

            // Quan hệ ReadHistory <-> User và Article (tránh multiple cascade paths)
            modelBuilder.Entity<ReadHistory>()
                .HasOne(r => r.User)
                .WithMany(u => u.ReadHistories)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict); // KHÔNG cascade khi xóa User

            modelBuilder.Entity<ReadHistory>()
                .HasOne(r => r.Article)
                .WithMany(a => a.ReadHistories)
                .HasForeignKey(r => r.ArticleId)
                .OnDelete(DeleteBehavior.Cascade); // Cho phép cascade từ Article

            // Unique Email đã khai báo bằng [Index] trên entity User.
            // Nếu muốn default DateTime do DB cấp (GETUTCDATE()), có thể bỏ set mặc định ở entity
            // và dùng HasDefaultValueSql("GETUTCDATE()") như dưới:

            // Default GETUTCDATE() từ SQL Server
            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<Category>()
                .Property(c => c.CreatedAt)
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
        }
    }
}

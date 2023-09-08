using BlogProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlogProject.Context
{
    public class BlogDbContext : IdentityDbContext
    {
        public BlogDbContext(DbContextOptions<BlogDbContext> options)
        : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<PostScore> PostScores { get; set; }
        public DbSet<CommentScore> CommentScores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityUserLogin<string>>().HasKey(x => new { x.LoginProvider, x.ProviderKey });
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CommentScore>()
                .HasKey(v => new { v.UserId, v.CommentId });

            modelBuilder.Entity<PostScore>()
                .HasKey(v => new { v.UserId, v.PostId });
        }
    }
}

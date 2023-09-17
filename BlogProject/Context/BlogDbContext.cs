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
        public DbSet<Community> Communities { get; set; }
        public DbSet<CommunityPost> CommunityPosts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityUserLogin<string>>().HasKey(x => new { x.LoginProvider, x.ProviderKey });
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CommentScore>()
                .HasKey(v => new { v.UserId, v.CommentId });

            modelBuilder.Entity<Comment>()
                .HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PostScore>()
                .HasKey(v => new { v.UserId, v.PostId });

            modelBuilder.Entity<CommunityPost>()
                .HasKey(cp => new {cp.PostId, cp.CommunityId});
        }
    }
}

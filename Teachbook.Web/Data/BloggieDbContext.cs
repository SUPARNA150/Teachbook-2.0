using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Teachbook.Web.Models.Domain;

namespace Teachbook.Web.Data
{
    public class BloggieDbContext : DbContext
    {
        public BloggieDbContext(DbContextOptions<BloggieDbContext> options) : base(options)
        {
        }


        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<BlogPostLike> BlogPostLike { get; set; }
        public DbSet<BlogPostSave> BlogPostSave { get; set; }
        public DbSet<BlogPostComment> BlogPostComment { get; set; }
        public DbSet<Messages> Message { get; set; }

    }
}

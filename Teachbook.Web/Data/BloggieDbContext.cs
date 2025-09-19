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

        //protected BloggieDbContext()
        //{
        //}

        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Tag> Tags { get; set; }

    }
}

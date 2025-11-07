using Azure;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Teachbook.Web.Data;
using Teachbook.Web.Models.Domain;

namespace Teachbook.Web.Repositories
{
    public class BlogPostRepository : IBlogPostRepository
    {
        private readonly BloggieDbContext bloggieDbContext;

        public BlogPostRepository(BloggieDbContext bloggieDbContext)
        {
            this.bloggieDbContext = bloggieDbContext;
        }
        public async Task<BlogPost> AddAsync(BlogPost blogPost)
        {
            await bloggieDbContext.AddAsync(blogPost);
            await bloggieDbContext.SaveChangesAsync();
            return blogPost;
        }

        public async Task<BlogPost?> DeleteAsync(Guid id)
        {
            var existingBlogPost = await bloggieDbContext.BlogPosts.FindAsync(id);
            if(existingBlogPost != null)
            {
                bloggieDbContext.BlogPosts.Remove(existingBlogPost);
                await bloggieDbContext.SaveChangesAsync();
                return existingBlogPost;
            }
            return null;
        }

        public async Task<IEnumerable<BlogPost>> GetAllAsync(
            string? searchQuery,
            string? sortBy,
            string? sortDirection,
            int pageNumber = 1,
            int pageSize = 100)
        {
            var query = bloggieDbContext.BlogPosts.AsQueryable();

            // Filtering
            if (string.IsNullOrWhiteSpace(searchQuery) == false)
            {
                query = query.Where(x => x.Heading.Contains(searchQuery) ||
                                         x.Tags.Any(t => t.Name.Contains(searchQuery)));
            }


            // Sorting
            if (string.IsNullOrWhiteSpace(sortBy) == false)
            {
                var isDesc = string.Equals(sortDirection, "Desc", StringComparison.OrdinalIgnoreCase);

                if (string.Equals(sortBy, "Heading", StringComparison.OrdinalIgnoreCase))
                {
                    query = isDesc ? query.OrderByDescending(x => x.Heading) : query.OrderBy(x => x.Heading);
                }
                if (string.Equals(sortBy, "Tags", StringComparison.OrdinalIgnoreCase))
                {
                    query = isDesc ? query.OrderByDescending(x => x.Tags.OrderBy(t => t.Name).Select(t => t.Name).FirstOrDefault())
                                   : query.OrderBy(x => x.Tags.OrderBy(t => t.Name).Select(t => t.Name).FirstOrDefault());
                }
            }

            // Pagination
            // Skip 0 Take 5 -> Page 1 of 5 results
            // Skip 5 Take next 5 -> Page 2 of 5 results
            var skipResults = (pageNumber - 1) * pageSize;
            query = query.Skip(skipResults).Take(pageSize);

            return await query.Include(x => x.Tags).ToListAsync();

            //return await bloggieDbContext.BlogPosts.Include(x => x.Tags).ToListAsync();
        }

        public async Task<BlogPost?> GetAsync(Guid id)
        {
            return await bloggieDbContext.BlogPosts.Include(x => x.Tags).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<BlogPost?> GetByUrlHandleAsync(string urlHandle)
        {
            return await bloggieDbContext.BlogPosts.Include(x => x.Tags).FirstOrDefaultAsync(x => x.UrlHandle == urlHandle);
        }

        public async Task<BlogPost?> UpdateAsync(BlogPost blogPost)
        {
            var existingBlog = await bloggieDbContext.BlogPosts.Include(x => x.Tags).FirstOrDefaultAsync(x => x.Id == blogPost.Id);

            if (existingBlog != null)
            {
                existingBlog.Id = blogPost.Id;
                existingBlog.Heading = blogPost.Heading;
                existingBlog.PageTitle = blogPost.PageTitle;
                existingBlog.Content = blogPost.Content;
                existingBlog.ShortDescription = blogPost.ShortDescription;
                existingBlog.FeaturedImageUrl = blogPost.FeaturedImageUrl;
                existingBlog.UrlHandle = blogPost.UrlHandle;
                existingBlog.PublishedDate = blogPost.PublishedDate;
                existingBlog.Author = blogPost.Author;
                existingBlog.Visible = blogPost.Visible;
                existingBlog.Tags = blogPost.Tags;

                //update tags
                /*existingBlog.Tags.Clear();
                foreach (var tag in blogPost.Tags)
                {
                    existingBlog.Tags.Add(tag);
                }*/


                await bloggieDbContext.SaveChangesAsync();

                return existingBlog;
            }

            return null;
        }

        public async Task<int> CountAsync()
        {
            return await bloggieDbContext.BlogPosts.CountAsync();
        }

        public async Task<IEnumerable<BlogPost>> GetPagedAsync(int skip, int take)
        {
            return await bloggieDbContext.BlogPosts
                        .Include(x => x.Tags)
                        .Skip(skip)
                        .Take(take)
                        .OrderByDescending(x => x.PublishedDate)
                        .ToListAsync();
        }
    }
}

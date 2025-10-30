
using Microsoft.EntityFrameworkCore;
using Teachbook.Web.Data;
using Teachbook.Web.Migrations;
using Teachbook.Web.Models.Domain;

namespace Teachbook.Web.Repositories
{
    public class BlogPostLikeRepository : IBlogPostLikeRepository
    {
        private readonly BloggieDbContext bloggieDbContext;

        public BlogPostLikeRepository(BloggieDbContext bloggieDbContext)
        {
            this.bloggieDbContext = bloggieDbContext;
        }

        // Toggles Like(Add if not liked, Remove if already liked)
        public async Task<(bool liked, int totalLikes)> ToggleLikeAsync(Guid blogPostId, Guid userId)
        {
            var existingLike = await bloggieDbContext.BlogPostLike
                .FirstOrDefaultAsync(x => x.BlogPostId == blogPostId && x.UserId == userId);

            bool liked;

            if (existingLike != null)
            {
                // User already liked → remove it (unlike)
                bloggieDbContext.BlogPostLike.Remove(existingLike);
                liked = false;
            }
            else
            {
                // User not liked yet → add like
                var like = new BlogPostLike
                {
                    BlogPostId = blogPostId,
                    UserId = userId
                };
                await bloggieDbContext.BlogPostLike.AddAsync(like);
                liked = true;
            }

            await bloggieDbContext.SaveChangesAsync();

            // Get updated total likes
            var totalLikes = await bloggieDbContext.BlogPostLike
                .CountAsync(x => x.BlogPostId == blogPostId);

            return (liked, totalLikes);
        }

        public async Task<bool> HasUserLikedAsync(Guid blogPostId, Guid userId)
        {
            return await bloggieDbContext.BlogPostLike
                .AnyAsync(x => x.BlogPostId == blogPostId && x.UserId == userId);
        }


        public async Task<int> GetTotalLikes(Guid blogPostId)
        {
            return await bloggieDbContext.BlogPostLike
                .CountAsync(x => x.BlogPostId == blogPostId);
        }


        /*public async Task<BlogPostLike> AddLikeForBlog(BlogPostLike blogPostLike)
        {
            await bloggieDbContext.BlogPostLike.AddAsync(blogPostLike);
            await bloggieDbContext.SaveChangesAsync();
            return blogPostLike;
        }

        public async Task<IEnumerable<BlogPostLike>> GetLikesForBlog(Guid blogPostId)
        {
            return await bloggieDbContext.BlogPostLike.Where(x => x.BlogPostId == blogPostId)
               .ToListAsync();
        }*/


    }
}

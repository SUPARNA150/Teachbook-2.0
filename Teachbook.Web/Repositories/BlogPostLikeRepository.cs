
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Teachbook.Web.Data;
using Teachbook.Web.Migrations;
using Teachbook.Web.Models.Domain;


namespace Teachbook.Web.Repositories
{
    public class BlogPostLikeRepository : IBlogPostLikeRepository
    {
        private readonly BloggieDbContext bloggieDbContext;
        private readonly IDatabase _redisDb;

        public BlogPostLikeRepository(BloggieDbContext bloggieDbContext, IConnectionMultiplexer redis)
        {
            this.bloggieDbContext = bloggieDbContext;
            _redisDb = redis.GetDatabase();
        }

        // Toggles Like(Add if not liked, Remove if already liked)
        public async Task<(bool liked, int totalLikes)> ToggleLikeAsync(Guid blogPostId, Guid userId)
        {
            string redisSetKey = $"blogLikes:{blogPostId}";
            string redisCountKey = $"blogLikesCount:{blogPostId}";
            bool liked;

            // Check if user already liked (from SQL, the source of truth)
            var existingLike = await bloggieDbContext.BlogPostLike
                .FirstOrDefaultAsync(x => x.BlogPostId == blogPostId && x.UserId == userId);

            if (existingLike != null)
            {
                // User already liked → remove it (unlike)
                bloggieDbContext.BlogPostLike.Remove(existingLike);
                liked = false;

                // Also remove from Redis cache set (if present)
                await _redisDb.SetRemoveAsync(redisSetKey, userId.ToString());
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

                // Add to Redis
                await _redisDb.SetAddAsync(redisSetKey, userId.ToString());
            }

            await bloggieDbContext.SaveChangesAsync();


            // Get updated total likes
            var totalLikes = await bloggieDbContext.BlogPostLike
                .CountAsync(x => x.BlogPostId == blogPostId);

            // Update Redis count cache
            await _redisDb.StringSetAsync($"blogLikesCount:{blogPostId}", totalLikes, TimeSpan.FromMinutes(10));

            return (liked, totalLikes);
        }

        public async Task<bool> HasUserLikedAsync(Guid blogPostId, Guid userId)
        {
            string redisSetKey = $"blogLikes:{blogPostId}";

            // Check Redis
            bool existsInCache = await _redisDb.SetContainsAsync(redisSetKey, userId.ToString());
            if (existsInCache)
                return true;


            // Check SQL
            bool liked = await bloggieDbContext.BlogPostLike
                .AnyAsync(x => x.BlogPostId == blogPostId && x.UserId == userId);


            // Cache result (only if liked)
            if (liked)
                await _redisDb.SetAddAsync(redisSetKey, userId.ToString());

            return liked;
        }


        public async Task<int> GetTotalLikes(Guid blogPostId)
        {
            string redisCountKey = $"blogLikesCount:{blogPostId}";

            // Try Redis first
            var cachedValue = await _redisDb.StringGetAsync(redisCountKey);

            if (cachedValue.HasValue)
            {
                return (int)cachedValue;
            }           

            // If not in Redis → query SQL
            int totalLikes = await bloggieDbContext.BlogPostLike
                .CountAsync(x => x.BlogPostId == blogPostId);

            // Cache the result for future
            await _redisDb.StringSetAsync(redisCountKey, totalLikes, TimeSpan.FromMinutes(10));

            return totalLikes;
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

using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Teachbook.Web.Data;
using Teachbook.Web.Models.Domain;

namespace Teachbook.Web.Repositories
{
    public class BlogPostSaveRepository : IBlogPostSaveRepository
    {
        private readonly BloggieDbContext bloggieDbContext;
        private readonly IDatabase _redisDb;
        private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(10);

        public BlogPostSaveRepository(BloggieDbContext bloggieDbContext, IConnectionMultiplexer redis)
        {
            this.bloggieDbContext = bloggieDbContext;
            _redisDb = redis.GetDatabase();
        }

        // Toggles Save(Add if not saved, Remove if already saved)
        public async Task<bool> ToggleSaveAsync(Guid blogPostId, Guid userId)
        {
            string redisSetKey = $"blogSaves:{blogPostId}";
            bool saved;

            // Check if user already saved (from SQL, the source of truth)
            var existingSave = await bloggieDbContext.BlogPostSave
                .FirstOrDefaultAsync(x => x.BlogPostId == blogPostId && x.UserId == userId);

            if (existingSave != null)
            {
                // User already saved → remove it (unsave)
                bloggieDbContext.BlogPostSave.Remove(existingSave);
                saved = false;

                // Also remove from Redis cache set (if present)
                await _redisDb.SetRemoveAsync(redisSetKey, userId.ToString());
            }
            else
            {
                // User not saved yet → add save
                var save = new BlogPostSave
                {
                    BlogPostId = blogPostId,
                    UserId = userId
                };
                await bloggieDbContext.BlogPostSave.AddAsync(save);
                saved = true;

                // Add to Redis
                await _redisDb.SetAddAsync(redisSetKey, userId.ToString());
            }

            await bloggieDbContext.SaveChangesAsync();
            await _redisDb.KeyExpireAsync(redisSetKey, _cacheExpiry);
            return saved;
        }

        public async Task<bool> HasUserSavedAsync(Guid blogPostId, Guid userId)
        {
            string redisSetKey = $"blogSaves:{blogPostId}";

            // Check Redis
            bool existsInCache = await _redisDb.SetContainsAsync(redisSetKey, userId.ToString());
            if (existsInCache)
                return true;


            // Check SQL
            bool saved = await bloggieDbContext.BlogPostSave
                .AnyAsync(x => x.BlogPostId == blogPostId && x.UserId == userId);


            // Cache result (only if saved)
            if (saved)
            {
                await _redisDb.SetAddAsync(redisSetKey, userId.ToString());
                await _redisDb.KeyExpireAsync(redisSetKey, _cacheExpiry);
            }
            return saved;
        }


        //Get all saved blogs for current user
        public async Task<IEnumerable<BlogPost>> GetSavedBlogsByUserAsync(Guid userId)
        {
            return await bloggieDbContext.BlogPostSave
                .Where(x => x.UserId == userId)
                .Include(x => x.BlogPost)
                .Select(x => x.BlogPost)
                .ToListAsync();
        }

    }
}

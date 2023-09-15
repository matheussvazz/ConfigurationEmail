using Blog.Data;
using Blog.ViewModels.Posts;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Controllers
{
    [ApiController]
    public class PostControllers : ControllerBase
    {
        [HttpGet("v1/posts")]
        public async Task<IActionResult> GetAsync(
         [FromServices] BlogDataContext context)
        {
            var posts = await context
                  .Posts //DbSet<Post>
                  .Include(x => x.Category)
                  .Include(x => x.Author)
                  .AsNoTracking()
                  .Select(x => new ListPostsViewModel
                  {
                      Id = x.Id,
                      Title = x.Title,
                      Slug = x.Slug,
                      LastUpdate = x.LastUpdate,
                      Category = x.Category.Name,
                      Author = $"{x.Author.Name} ({x.Author.Email})"
                  })
                  .ToListAsync();
            return Ok(posts);


        }
    }
}
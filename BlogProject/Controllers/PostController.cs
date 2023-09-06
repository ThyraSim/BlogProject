using BlogProject.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace BlogProject.Controllers
{
    public class PostController : Controller
    {
        private readonly BlogDbContext _dbContext;
        private readonly IDbConnection _connection;

        public PostController(BlogDbContext dbContext, IDbConnection connection)
        {
            _connection = connection;
            _dbContext = dbContext;
        }

        // GET: PostController
        public async Task<IActionResult> Index()
        {
            var posts = await _dbContext.Posts.FromSqlRaw("EXECUTE dbo.GetAllPosts").ToListAsync();

            var userIds = posts.Select(p => p.UserId).Distinct().ToList();

            var users = await _dbContext.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();

            foreach (var post in posts)
            {
                post.User = users.FirstOrDefault(u => u.Id == post.UserId);
            }

            return View(posts);
        }


        // GET: PostController/Details/5
        public async Task<IActionResult> PostDetails(int id)
        {
            var posts = await _dbContext.Posts.FromSqlRaw("EXECUTE dbo.GetPostById @Id", new SqlParameter("Id", id)).ToListAsync();
            var post = posts.FirstOrDefault();
            var comments = await _dbContext.Comments.FromSqlRaw("EXECUTE dbo.GetCommentsByPost @PostId", new SqlParameter("PostId", id)).ToListAsync();

            var userIds = posts.Select(p => p.UserId).Distinct().ToList();

            var users = await _dbContext.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();

            post.User = users.FirstOrDefault(u => u.Id == post.UserId);

            ViewBag.Post = post;
            ViewBag.Comments = comments;

            return View();
        }

        public async Task<IActionResult> UpdateScore(int postId, int change)
        {
            // Stored Procedure to call
            string storedProcName = change > 0 ? "IncrementPostScore" : "DecrementPostScore";

            // Create and configure a SQL parameter
            var postIdParam = new SqlParameter("@PostId", postId);

            // Execute the stored procedure
            await _dbContext.Database.ExecuteSqlRawAsync($"EXEC {storedProcName} @PostId", postIdParam);

            // Retrieve the updated score from the database to send back to the client
            var updatedPost = await _dbContext.Posts.FindAsync(postId);
            if (updatedPost == null)
            {
                return NotFound();
            }

            return Json(new { newScore = updatedPost.Score });
        }


        // GET: PostController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PostController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: PostController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: PostController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: PostController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PostController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}

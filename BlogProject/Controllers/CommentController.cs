using BlogProject.Context;
using BlogProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BlogProject.Controllers
{
    public class CommentController : Controller
    {
        private readonly BlogDbContext _dbContext; // replace YourDbContext with the actual name of your DbContext class

        public CommentController(BlogDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: CommentController
        public ActionResult Index()
        {
            return View();
        }

        // GET: CommentController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: CommentController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CommentController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int postId, string comment)
        {
            if (ModelState.IsValid)
            {
                // Assuming Comment model has properties like PostId, Body, CreatedAt
                var newComment = new Comment
                {
                    PostId = postId,
                    UserId = 1,
                    Body = comment,
                    Score = 0,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                    // other properties like UserId, Score can be added here
                };

                _dbContext.Comments.Add(newComment);
                _dbContext.SaveChanges();

                // Redirect back to the post details view
                return RedirectToAction("PostDetails", "Post", new { id = postId });
            }

            // Handle validation errors or other issues here
            return View(); // You might want to return some view or error message
        }

        // GET: CommentController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: CommentController/Edit/5
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

        // GET: CommentController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: CommentController/Delete/5
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

        public async Task<IActionResult> UpdateScore(int commentId, int change)
        {
            string storedProcName = change > 0 ? "IncrementCommentScore" : "DecrementCommentScore";

            var CommentIdParam = new SqlParameter("@CommentId", commentId);

            await _dbContext.Database.ExecuteSqlRawAsync($"EXEC {storedProcName} @CommentId", CommentIdParam);

            var updatedComment = await _dbContext.Comments.FindAsync(commentId);
            if (updatedComment == null)
            {
                return NotFound();
            }

            return Json(new { newScore = updatedComment.Score });
        }
    }
}

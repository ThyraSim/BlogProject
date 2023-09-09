using BlogProject.Context;
using BlogProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace BlogProject.Controllers
{
    public class CommentController : Controller
    {
        private readonly BlogDbContext _dbContext;
        private readonly IDbConnection _connection;
        private readonly UserManager<IdentityUser> _userManager;

        public CommentController(BlogDbContext dbContext, IDbConnection connection,
            UserManager<IdentityUser> userManager)
        {
            _connection = connection;
            _dbContext = dbContext;
            _userManager = userManager;
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
                    UserId = _userManager.GetUserId(User),
                    Body = comment,
                    Score = 0,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
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
            if (User.Identity?.IsAuthenticated != true)
            {
                return Unauthorized();
            }

            string userId = _userManager.GetUserId(User);

            var existingScore = await _dbContext.CommentScores
                                .FirstOrDefaultAsync(cs => cs.UserId == userId && cs.CommentId == commentId);

            int newVote;

            if (existingScore == null)
            {
                newVote = change;
                await ExecuteVoteChange(change, commentId);
                _dbContext.CommentScores.Add(new CommentScore
                {
                    UserId = userId,
                    CommentId = commentId,
                    Vote = change
                });
            }
            else if (existingScore.Vote != change)
            {
                // User wants to change their vote.
                await ExecuteVoteChange(-existingScore.Vote, commentId);  // Undo previous vote
                await ExecuteVoteChange(change, commentId);  // Apply new vote
                existingScore.Vote = change;
                newVote = change;
            }
            else
            {
                // User wants to retract their vote.
                await ExecuteVoteChange(-change, commentId);  // Undo previous vote
                _dbContext.CommentScores.Remove(existingScore);
                newVote = 0;
            }

            await _dbContext.SaveChangesAsync();

            var updatedComment = await _dbContext.Comments.FindAsync(commentId);
            if (updatedComment == null)
            {
                return NotFound();
            }

            return Json(new { newScore = updatedComment.Score, newVote });
        }

        private async Task ExecuteVoteChange(int change, int commentId)
        {
            string storedProcName = change > 0 ? "IncrementCommentScore" : "DecrementCommentScore";
            await ExecuteStoredProcedure(storedProcName, commentId);
        }

        private async Task ExecuteStoredProcedure(string procName, int commentId)
        {
            var sqlParam = new SqlParameter("@CommentId", commentId);
            await _dbContext.Database.ExecuteSqlRawAsync($"EXEC {procName} @CommentId", sqlParam);
        }
    }
}

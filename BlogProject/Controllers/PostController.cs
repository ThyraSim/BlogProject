using BlogProject.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using BlogProject.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using BlogProject.ViewModel;

namespace BlogProject.Controllers
{
    public class PostController : Controller
    {
        private readonly BlogDbContext _dbContext;
        private readonly IDbConnection _connection;
        private readonly UserManager<IdentityUser> _userManager;


        public PostController(BlogDbContext dbContext, IDbConnection connection, 
            UserManager<IdentityUser> userManager)
        {
            _connection = connection;
            _dbContext = dbContext;
            _userManager = userManager;
        }

        // GET: PostController
        public async Task<IActionResult> Index()
        {
            var posts = await _dbContext.Posts.FromSqlRaw("EXEC GetAllPosts").ToListAsync();
            var postIds = posts.Select(p => p.Id).ToList();

            var users = _userManager.Users.ToList();
            var usersDictionary = users.ToDictionary(u => u.Id, u => u);

            string currentUserId = _userManager.GetUserId(User);

            var comments = await _dbContext.Comments
                     .Where(c => postIds.Contains(c.PostId))
                     .ToListAsync();

            foreach (var post in posts)
            {
                post.Comments = comments.Where(c => c.PostId == post.Id).ToList();

                if (usersDictionary.TryGetValue(post.UserId, out var identityUser))
                {
                    post.User = ConvertToAppUser(identityUser);
                }

                foreach (var comment in post.Comments)
                {
                    if (usersDictionary.TryGetValue(comment.UserId, out var identityUser1))
                    {
                        comment.User = ConvertToAppUser(identityUser1);
                    }
                }
            }

            var postViewModels = posts.Select(p => new PostViewModel
            {
                Post = p,
                CurrentUserVote = _dbContext.PostScores.FirstOrDefault(ps => ps.PostId == p.Id && ps.UserId == currentUserId)?.Vote ?? 0
            }).ToList();

            return View(postViewModels);
        }

        public User ConvertToAppUser(IdentityUser identityUser)
        {
            return new User
            {
                Id = identityUser.Id,
                UserName = identityUser.UserName,
            };
        }

        // GET: PostController/Details/5
        public async Task<IActionResult> PostDetails(int id)
        {
            var posts = await _dbContext.Posts.FromSqlRaw("EXECUTE dbo.GetPostById @Id", new SqlParameter("Id", id)).ToListAsync();
            var post = posts.FirstOrDefault();

            var users = _userManager.Users.ToList();
            var usersDictionary = users.ToDictionary(u => u.Id, u => u);

            var comments = await _dbContext.Comments.FromSqlRaw("EXECUTE dbo.GetCommentsByPost @PostId", new SqlParameter("PostId", id)).ToListAsync();

            // If we don't find a matching post, return an error.
            if (post == null)
            {
                return NotFound($"No post found with ID {id}");
            }

            string currentUserId = _userManager.GetUserId(User);
            var postViewModel = new PostViewModel
            {
                Post = post,
                CurrentUserVote = _dbContext.PostScores.FirstOrDefault(ps => ps.PostId == post.Id && ps.UserId == currentUserId)?.Vote ?? 0
            };

            if (usersDictionary.TryGetValue(post.UserId, out var identityUser))
            {
                post.User = ConvertToAppUser(identityUser);
            }

            foreach (var comment in post.Comments)
            {
                if (usersDictionary.TryGetValue(comment.UserId, out var identityUser1))
                {
                    comment.User = ConvertToAppUser(identityUser1);
                }
            }

            ViewBag.PostView = postViewModel;
            ViewBag.Comments = comments;

            return View();
        }


        public async Task<IActionResult> UpdateScore(int postId, int change)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Unauthorized();
            }

            string userId = _userManager.GetUserId(User);

            var existingScore = await _dbContext.PostScores
                                .FirstOrDefaultAsync(ps => ps.UserId == userId && ps.PostId == postId);
            int newVote;

            if (existingScore == null)
            {
                newVote = change;
                // User hasn't voted yet; add their vote.
                await ExecuteVoteChange(change, postId);
                _dbContext.PostScores.Add(new PostScore
                {
                    UserId = userId,
                    PostId = postId,
                    Vote = change
                });
            }
            else if (existingScore.Vote != change)
            {
                // User wants to change their vote.
                await ExecuteVoteChange(-existingScore.Vote, postId);  // Undo previous vote
                await ExecuteVoteChange(change, postId);  // Apply new vote
                existingScore.Vote = change;
                newVote = change;
            }
            else
            {
                // User wants to retract their vote.
                await ExecuteVoteChange(-change, postId);  // Undo previous vote
                _dbContext.PostScores.Remove(existingScore);
                newVote = 0;
            }

            await _dbContext.SaveChangesAsync();

            var updatedPost = await _dbContext.Posts.FindAsync(postId);
            if (updatedPost == null)
            {
                return NotFound();
            }

            return Json(new { newScore = updatedPost.Score, newVote });
        }

        private async Task ExecuteVoteChange(int change, int postId)
        {
            string storedProcName = change > 0 ? "IncrementPostScore" : "DecrementPostScore";
            await ExecuteStoredProcedure(storedProcName, postId);
        }


        private async Task ExecuteStoredProcedure(string procName, int postId)
        {
            var sqlParam = new SqlParameter("@PostId", postId);
            await _dbContext.Database.ExecuteSqlRawAsync($"EXEC {procName} @PostId", sqlParam);
        }

        // GET: PostController/Create
        public ActionResult CreatePost()
        {
            return View();
        }

        // POST: PostController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(IFormCollection collection)
        {
            ClaimsPrincipal currentUser = this.User;
            string currentUserID;
            if (currentUser == null || !currentUser.Identity.IsAuthenticated)
            {
                return Challenge();
            } else
            {
                currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            }


            string Titre = collection["Titre"];
            string Body = collection["Body"];

            if (Titre is null || Body is null)
            {
                throw new ArgumentNullException("Tous les champs sont obligatoires");
            }
            
            // Use Entity Framework to insert data
            var post = new Post
            {
                UserId = currentUserID,
                Titre = Titre,
                Body = Body
            };

            _dbContext.Posts.Add(post);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index");
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

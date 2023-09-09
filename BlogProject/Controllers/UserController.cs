using BlogProject.Context;
using BlogProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.Data.SqlClient;

namespace BlogProject.Controllers
{
    public class UserController : Controller
    {
        private readonly BlogDbContext _dbContext;
        private readonly IDbConnection _connection;

        public UserController(BlogDbContext dbContext, IDbConnection connection)
        {
            _connection = connection;
            _dbContext = dbContext;
        }

        // GET: UserController
        public ActionResult Index()
        {
            return View();
        }

        // GET: UserController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: UserController/Create
        public ActionResult Create()
        {
            User u = new User();
            return View(u);
        }

        // POST: UserController/Create
        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                var idParam = new SqlParameter("@Id", user.Id);
                var usernameParam = new SqlParameter("@Username", user.UserName);
                var passwordParam = new SqlParameter("@Password", user.PasswordHash);

                await _dbContext.Database.ExecuteSqlRawAsync("EXEC CreateUser @Username, @Password", usernameParam, passwordParam);

                return RedirectToAction("Create");
            }

            return View(user);
        }

        // GET: UserController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: UserController/Edit/5
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

        // GET: UserController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UserController/Delete/5
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using lab3app.Models;
using System.Threading.Tasks;
using lab3app.Models;

namespace lab3app.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserContext _context;

        public AccountController(UserContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(User userLogin)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == userLogin.Username && u.UserPassword == userLogin.UserPassword);

            if (user == null)
            {
                TempData["LoginError"] = "Invalid username or password.";
                return View(userLogin);
            }

            TempData["UserId"] = user.UserID;
            TempData["Username"] = user.Username;

            return RedirectToAction("MoviesPage", "Movie");
        }


        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(User model)
        {
            if (ModelState.IsValid)
            {
                bool emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
                bool usernameExists = await _context.Users.AnyAsync(u => u.Username == model.Username);

                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Email is already taken.");
                }

                if (usernameExists)
                {
                    ModelState.AddModelError("Username", "Username is already taken.");
                }

                if (emailExists || usernameExists)
                {
                    return View(model);
                }

                model.DateCreated = DateTime.Now;

                _context.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login");
            }
            return View(model);
        }
    }
}

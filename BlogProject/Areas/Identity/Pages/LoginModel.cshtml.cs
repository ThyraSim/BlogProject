using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class LoginModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;
    public LoginModel(SignInManager<IdentityUser> signInManager)
    {
        _signInManager = signInManager;
        Console.WriteLine("At least login works");
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public class InputModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(Input.UserName, Input.Password, false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return RedirectToPage("/Index");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
        }

        return Page();
    }
}

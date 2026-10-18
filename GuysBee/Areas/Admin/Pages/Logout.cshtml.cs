using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GuysBee.Areas.Admin.Pages;

public class LogoutModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signIn;
    public LogoutModel(SignInManager<IdentityUser> signIn) => _signIn = signIn;

    public IActionResult OnGet() => RedirectToPage("/Login");

    public async Task<IActionResult> OnPostAsync()
    {
        await _signIn.SignOutAsync();
        return RedirectToPage("/Login");
    }
}

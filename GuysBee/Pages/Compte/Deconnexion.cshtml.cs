using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GuysBee.Pages.Compte;

public class DeconnexionModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signIn;
    public DeconnexionModel(SignInManager<IdentityUser> signIn) => _signIn = signIn;

    public IActionResult OnGet() => Redirect("/Compte");

    public async Task<IActionResult> OnPostAsync()
    {
        await _signIn.SignOutAsync();
        return Redirect("/");
    }
}

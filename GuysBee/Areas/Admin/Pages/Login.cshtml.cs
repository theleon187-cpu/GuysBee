using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GuysBee.Areas.Admin.Pages;

public class LoginModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signIn;
    public LoginModel(SignInManager<IdentityUser> signIn) => _signIn = signIn;

    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Merci d'indiquer votre e-mail."), EmailAddress]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Merci d'indiquer votre mot de passe."), DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid) return Page();

        var result = await _signIn.PasswordSignInAsync(Input.Email, Input.Password, isPersistent: false, lockoutOnFailure: true);
        if (result.Succeeded)
            return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToPage("/Index");

        ModelState.AddModelError("", result.IsLockedOut
            ? "Compte temporairement bloqué. Réessayez dans quelques minutes."
            : "E-mail ou mot de passe incorrect.");
        return Page();
    }
}

using System.ComponentModel.DataAnnotations;
using GuysBee.Services.Shop;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GuysBee.Pages.Compte;

public class ConnexionModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signIn;
    private readonly ShopSettings _shop;

    public ConnexionModel(SignInManager<IdentityUser> signIn, ShopSettings shop)
    {
        _signIn = signIn;
        _shop = shop;
    }

    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Merci d'indiquer votre e-mail."), EmailAddress(ErrorMessage = "E-mail invalide.")]
        public string Email { get; set; } = "";
        [Required(ErrorMessage = "Merci d'indiquer votre mot de passe."), DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }

    public async Task<IActionResult> OnGetAsync() =>
        (await _shop.GetAsync()).AccountsEnabled ? Page() : NotFound();

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!(await _shop.GetAsync()).AccountsEnabled) return NotFound();
        if (!ModelState.IsValid) return Page();

        var result = await _signIn.PasswordSignInAsync(Input.Email, Input.Password, isPersistent: false, lockoutOnFailure: true);
        if (result.Succeeded)
            return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : Redirect("/Compte");

        ModelState.AddModelError("", result.IsLockedOut
            ? "Compte temporairement bloqué. Réessayez dans quelques minutes."
            : "E-mail ou mot de passe incorrect.");
        return Page();
    }
}

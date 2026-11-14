using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GuysBee.Areas.Admin.Pages.MotDePasse;

public class IndexModel : PageModel
{
    private readonly UserManager<IdentityUser> _users;
    private readonly SignInManager<IdentityUser> _signIn;

    public IndexModel(UserManager<IdentityUser> users, SignInManager<IdentityUser> signIn)
    {
        _users = users;
        _signIn = signIn;
    }

    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Merci d'indiquer votre mot de passe actuel."), DataType(DataType.Password)]
        public string Current { get; set; } = "";

        [Required(ErrorMessage = "Merci de choisir un nouveau mot de passe."), DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "Le nouveau mot de passe doit contenir au moins 10 caractères.")]
        public string New { get; set; } = "";

        [DataType(DataType.Password), Compare(nameof(New), ErrorMessage = "Les deux mots de passe ne sont pas identiques.")]
        public string Confirm { get; set; } = "";
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var user = await _users.GetUserAsync(User);
        if (user is null) return Challenge();

        var result = await _users.ChangePasswordAsync(user, Input.Current, Input.New);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", Translate(error));
            return Page();
        }
        await _signIn.RefreshSignInAsync(user);
        TempData["Message"] = "Mot de passe modifié. Pensez à le noter en lieu sûr.";
        return RedirectToPage();
    }

    private static string Translate(IdentityError e) => e.Code switch
    {
        "PasswordMismatch" => "Le mot de passe actuel est incorrect.",
        "PasswordTooShort" => "Le nouveau mot de passe est trop court.",
        "PasswordRequiresDigit" => "Le nouveau mot de passe doit contenir au moins un chiffre.",
        "PasswordRequiresLower" => "Le nouveau mot de passe doit contenir au moins une minuscule.",
        "PasswordRequiresUpper" => "Le nouveau mot de passe doit contenir au moins une majuscule.",
        "PasswordRequiresNonAlphanumeric" => "Le nouveau mot de passe doit contenir au moins un caractère spécial (! ? # ...).",
        "PasswordRequiresUniqueChars" => "Le nouveau mot de passe doit contenir des caractères plus variés.",
        _ => e.Description
    };
}

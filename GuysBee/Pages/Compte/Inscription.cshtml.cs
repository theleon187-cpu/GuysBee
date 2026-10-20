using System.ComponentModel.DataAnnotations;
using GuysBee.Data;
using GuysBee.Data.Entities;
using GuysBee.Services.Shop;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GuysBee.Pages.Compte;

public class InscriptionModel : PageModel
{
    private readonly UserManager<IdentityUser> _users;
    private readonly SignInManager<IdentityUser> _signIn;
    private readonly AppDbContext _db;
    private readonly ShopSettings _shop;

    public InscriptionModel(UserManager<IdentityUser> users, SignInManager<IdentityUser> signIn, AppDbContext db, ShopSettings shop)
    {
        _users = users;
        _signIn = signIn;
        _db = db;
        _shop = shop;
    }

    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required(ErrorMessage = "Merci d'indiquer votre prénom.")] public string FirstName { get; set; } = "";
        [Required(ErrorMessage = "Merci d'indiquer votre nom.")] public string LastName { get; set; } = "";
        [Required(ErrorMessage = "Merci d'indiquer votre e-mail."), EmailAddress(ErrorMessage = "E-mail invalide.")]
        public string Email { get; set; } = "";
        [Required(ErrorMessage = "Merci de choisir un mot de passe."), DataType(DataType.Password)]
        public string Password { get; set; } = "";
        [DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "Les deux mots de passe sont différents.")]
        public string ConfirmPassword { get; set; } = "";
    }

    public async Task<IActionResult> OnGetAsync() =>
        (await _shop.GetAsync()).AccountsEnabled ? Page() : NotFound();

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!(await _shop.GetAsync()).AccountsEnabled) return NotFound();
        if (!ModelState.IsValid) return Page();

        var email = Input.Email.Trim();
        var user = new IdentityUser { UserName = email, Email = email };
        var result = await _users.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError("", e.Code is "DuplicateUserName" or "DuplicateEmail"
                    ? "Un compte existe déjà avec cet e-mail."
                    : e.Description);
            return Page();
        }

        _db.CustomerProfiles.Add(new CustomerProfile
        {
            UserId = user.Id,
            FirstName = Input.FirstName.Trim(),
            LastName = Input.LastName.Trim(),
        });
        await _db.SaveChangesAsync();

        await _signIn.SignInAsync(user, isPersistent: false);
        return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : Redirect("/Compte");
    }
}

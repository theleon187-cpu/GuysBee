using System.ComponentModel.DataAnnotations;
using GuysBee.Data;
using GuysBee.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GuysBee.Pages;

public class ContactModel : PageModel
{
    private readonly AppDbContext _db;
    public ContactModel(AppDbContext db) => _db = db;

    [BindProperty] public InputModel Input { get; set; } = new();
    [BindProperty] public string? Website { get; set; }
    public bool Sent { get; private set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Merci d'indiquer votre nom."), StringLength(100)]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Merci d'indiquer votre e-mail."), EmailAddress(ErrorMessage = "Adresse e-mail invalide."), StringLength(200)]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Merci d'indiquer un sujet."), StringLength(150)]
        public string Subject { get; set; } = "";

        [Required(ErrorMessage = "Merci d'écrire votre message."), StringLength(4000)]
        public string Body { get; set; } = "";
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        // Anti-spam : un robot remplit le champ caché.
        if (!string.IsNullOrEmpty(Website))
        {
            Sent = true;
            return Page();
        }
        if (!ModelState.IsValid) return Page();

        _db.ContactMessages.Add(new ContactMessage
        {
            Name = Input.Name.Trim(),
            Email = Input.Email.Trim(),
            Subject = Input.Subject.Trim(),
            Body = Input.Body.Trim()
        });
        await _db.SaveChangesAsync();
        Sent = true;
        ModelState.Clear();
        return Page();
    }
}

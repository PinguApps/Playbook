// snippet:complete
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VolunteerAccess;

[AllowAnonymous]
public sealed class LoginModel(SignInManager<IdentityUser> signIn) : PageModel
{
    [BindProperty] public LoginInput Input { get; set; } = new();
    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid) return Page();
        // PasswordSignInAsync retains lockout and configured confirmation checks.
        var result = await signIn.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded) return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/session");
        ModelState.AddModelError(string.Empty, "Sign-in could not be completed. Check your details and confirmation.");
        return Page();
    }
}

public sealed class LoginInput
{
    [Required, EmailAddress(ErrorMessage = "Enter a valid e-mail address.")]
    public string Email { get; set; } = "";
    [Required(ErrorMessage = "Password is required."), DataType(DataType.Password)]
    public string Password { get; set; } = "";
    public bool RememberMe { get; set; }
}
// endsnippet:complete

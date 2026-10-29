// snippet:complete
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace AquariumVolunteers;

public sealed class VolunteerRegister(UserManager<IdentityUser> users, SignInManager<IdentityUser> signIn,
    IEmailSender<IdentityUser> mail) : PageModel
{
    [BindProperty, Required, EmailAddress] public string Email { get; set; } = "";
    [BindProperty, Required] public string Password { get; set; } = "";

    public async Task<IActionResult> OnPostAsync(string? returnUrl)
    {
        returnUrl ??= "/";
        if (!Url.IsLocalUrl(returnUrl)) return BadRequest();
        if (!ModelState.IsValid) return Page();
        var user = new IdentityUser { UserName = Email, Email = Email };
        var created = await users.CreateAsync(user, Password);
        if (!created.Succeeded)
        {
            foreach (var error in created.Errors) ModelState.AddModelError("", error.Description);
            return Page();
        }
        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await users.GenerateEmailConfirmationTokenAsync(user)));
        var link = Url.Page("/Account/ConfirmEmail", null, new { area = "Identity", userId = user.Id, code }, Request.Scheme)!;
        await mail.SendConfirmationLinkAsync(user, Email, HtmlEncoder.Default.Encode(link));
        // Apply the SAME policy used by later sign-ins before any application cookie is issued.
        if (!await signIn.CanSignInAsync(user))
            return RedirectToPage("/Account/RegisterConfirmation", new { area = "Identity", email = Email, returnUrl });
        await signIn.SignInAsync(user, isPersistent: false);
        return LocalRedirect(returnUrl);
    }
}
// endsnippet:complete

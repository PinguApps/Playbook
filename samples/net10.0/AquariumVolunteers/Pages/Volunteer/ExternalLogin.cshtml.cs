// snippet:complete
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace AquariumVolunteers;

public sealed class VolunteerExternalLogin(UserManager<IdentityUser> users, SignInManager<IdentityUser> signIn,
    IEmailSender<IdentityUser> mail) : PageModel
{
    [BindProperty, Required, EmailAddress] public string Email { get; set; } = "";
    public string ReturnUrl { get; set; } = "/";
    public IActionResult OnPost(string provider, string? returnUrl)
    {
        if (provider != "LocalAquarium" || !Url.IsLocalUrl(returnUrl ?? "/")) return BadRequest();
        var callback = Url.Page("./ExternalLogin", "Callback", new { returnUrl })!;
        return Challenge(signIn.ConfigureExternalAuthenticationProperties(provider, callback), provider);
    }
    public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl)
    {
        ReturnUrl = returnUrl ?? "/";
        if (!Url.IsLocalUrl(ReturnUrl)) return BadRequest();
        var info = await signIn.GetExternalLoginInfoAsync();
        if (info is null) return BadRequest();
        var result = await signIn.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false, bypassTwoFactor: true);
        if (result.Succeeded) return LocalRedirect(ReturnUrl);
        if (result.IsLockedOut || result.IsNotAllowed) return RedirectToPage("/Account/Login", new { area = "Identity" });
        Email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? "";
        return Page();
    }
    public async Task<IActionResult> OnPostConfirmationAsync(string? returnUrl)
    {
        ReturnUrl = returnUrl ?? "/";
        if (!Url.IsLocalUrl(ReturnUrl)) return BadRequest();
        var info = await signIn.GetExternalLoginInfoAsync();
        if (info is null) return BadRequest();
        if (!ModelState.IsValid) return Page();
        var user = new IdentityUser { UserName = Email, Email = Email };
        var result = await users.CreateAsync(user);
        if (result.Succeeded) result = await users.AddLoginAsync(user, info);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return Page();
        }
        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await users.GenerateEmailConfirmationTokenAsync(user)));
        var link = Url.Page("/Account/ConfirmEmail", null, new { area = "Identity", userId = user.Id, code }, Request.Scheme)!;
        await mail.SendConfirmationLinkAsync(user, Email, HtmlEncoder.Default.Encode(link));
        if (!await signIn.CanSignInAsync(user))
            return RedirectToPage("/Account/RegisterConfirmation", new { area = "Identity", email = Email });
        await signIn.SignInAsync(user, false, info.LoginProvider);
        return LocalRedirect(ReturnUrl);
    }
}
// endsnippet:complete

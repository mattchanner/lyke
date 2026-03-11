using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lyke.Api.Pages.Account;

public class VerifyEmailModel : PageModel
{
    private readonly IAuthService _authService;

    public VerifyEmailModel(IAuthService authService)
    {
        _authService = authService;
    }

    public bool Succeeded { get; set; }

    public async Task<IActionResult> OnGetAsync(
        [FromQuery] string? email,
        [FromQuery] string? token,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            Succeeded = false;
            return Page();
        }

        try
        {
            await _authService.VerifyEmailAsync(email, token, cancellationToken);
            Succeeded = true;
        }
        catch
        {
            Succeeded = false;
        }

        return Page();
    }
}

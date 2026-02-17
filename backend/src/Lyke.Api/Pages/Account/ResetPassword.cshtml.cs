using System.ComponentModel.DataAnnotations;
using Lyke.Application.DTOs.Auth;
using Lyke.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lyke.Api.Pages.Account;

public class ResetPasswordModel : PageModel
{
    private readonly IAuthService _authService;

    public ResetPasswordModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    public ResetPasswordInput Input { get; set; } = new();

    public IActionResult OnGet([FromQuery] string? email, [FromQuery] string? token)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
        {
            return RedirectToPage("ResetPasswordResult", new { success = false });
        }

        Input = new ResetPasswordInput
        {
            Email = email,
            Token = token
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var request = new ResetPasswordRequest(
                Input.Email,
                Input.Token,
                Input.NewPassword,
                Input.ConfirmPassword);

            await _authService.ResetPasswordAsync(request, cancellationToken);
            return RedirectToPage("ResetPasswordResult", new { success = true });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}

public class ResetPasswordInput
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "Password must contain uppercase, lowercase, and a digit.")]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

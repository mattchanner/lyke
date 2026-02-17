using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Lyke.Api.Pages.Account;

public class ResetPasswordResultModel : PageModel
{
    [FromQuery]
    public bool Success { get; set; }

    public void OnGet()
    {
    }
}

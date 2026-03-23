using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MealPrep.Web.Pages;

public class IndexModel : PageModel
{
    public void OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            Response.Redirect("/Dashboard");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class AdminOnlyAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var userRole = context.HttpContext.Session.GetString("UserRole");

        if (userRole != "admin")
        {
            context.Result = new RedirectToActionResult("index", "Home", null);
        }
    }
}
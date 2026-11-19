using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using HabitTracker.Models;

namespace HabitTracker.Filters
{
    public class BlockedUserFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
                var signInManager = context.HttpContext.RequestServices.GetRequiredService<SignInManager<User>>();
                var current = await userManager.GetUserAsync(context.HttpContext.User);

                if (current == null || current.IsBlocked)
                {
                    await signInManager.SignOutAsync();

                    var tempData = context.HttpContext.RequestServices
                        .GetRequiredService<ITempDataDictionaryFactory>()
                        .GetTempData(context.HttpContext);
                    tempData["Error"] = "Your account has been blocked by an administrator.";

                    context.Result = new RedirectToActionResult("Login", "Auth", null);
                    return;
                }
            }

            await next();
        }
    }
}
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Localization;
using Habit_Tracker_Project.Models;
using Habit_Tracker_Project;

namespace Habit_Tracker_Project.Filters
{
    public class BlockedUserFilter : IAsyncActionFilter
    {
        private readonly IStringLocalizer<SharedResource> _localizer;

        public BlockedUserFilter(IStringLocalizer<SharedResource> localizer)
        {
            _localizer = localizer;
        }

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
                    tempData["Error"] = _localizer["Your account has been blocked by an administrator."].Value;

                    context.Result = new RedirectToActionResult("Login", "Auth", null);
                    return;
                }
            }

            await next();
        }
    }
}
using Clinics_Websites_Shops.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Clinics_Websites_Shops.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class PermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _permission;

        public PermissionAttribute(string permission)
        {
            _permission = permission;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new RedirectToActionResult("Login", "Account", new { area = "Identity" });
                return;
            }

            var permissionService = context.HttpContext.RequestServices
                .GetService<IPermissionService>();

            if (permissionService == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            var hasPermission = permissionService.UserHasPermissionAsync(user, _permission).Result;

            if (!hasPermission)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}


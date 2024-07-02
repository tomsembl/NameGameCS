using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace NameGameCS.Filters {
    
    

    public class UserAuthenticationFilter : IAsyncActionFilter {
        private readonly DBLogic _dbLogic;

        public UserAuthenticationFilter(DBLogic dbLogic) {
            _dbLogic = dbLogic;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
            var skipAuthentication = context.ActionDescriptor.EndpointMetadata
                .Any(metadata => metadata is SkipUserAuthenticationAttribute);

            if (skipAuthentication) {
                await next();
                return;
            }
            var controller = context.Controller as HomeController;
            if (controller != null) {
                var user = controller.getUserFromCookie(context.HttpContext.Request);
                if (user.user_id == 0) {
                    context.Result = new RedirectToActionResult("Home", "Home", null);
                    return;
                }
            }
            await next();
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class SkipUserAuthenticationAttribute : Attribute {
    }
}

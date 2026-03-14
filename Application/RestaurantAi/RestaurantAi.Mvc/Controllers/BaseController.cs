using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RestaurantAi.Mvc.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                    var jwt = handler.ReadJwtToken(token);

                    var isAdmin = jwt.Claims.Any(c =>
                        (c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                      || c.Type == "role")
                      && c.Value == "Admin");

                    ViewData["IsAdmin"] = isAdmin;
                    ViewData["IsLoggedIn"] = true;
                }
                catch
                {
                    ViewData["IsAdmin"] = false;
                    ViewData["IsLoggedIn"] = false;
                }
            }
            else
            {
                ViewData["IsAdmin"] = false;
                ViewData["IsLoggedIn"] = false;
            }

            base.OnActionExecuting(context);
        }
    }
}

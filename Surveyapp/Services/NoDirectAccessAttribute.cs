using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace Surveyapp.Services
{
    public class NoDirectAccessAttribute:ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            //check referer on request header
            var canAccess = false;
            var referer = context.HttpContext.Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                var rUri = new System.UriBuilder(referer).Uri;
                var req = context.HttpContext.Request;
                if (req.Host.Host==rUri.Host && req.Host.Port == rUri.Port && req.Scheme == rUri.Scheme)
                {
                    canAccess = true;
                }
            }
            //check other requirements
            if (!canAccess)
            {
                context.Result = new RedirectToRouteResult(new RouteValueDictionary(new{controller="Home",action="Index", area = ""}));
            }
        }
    }
}
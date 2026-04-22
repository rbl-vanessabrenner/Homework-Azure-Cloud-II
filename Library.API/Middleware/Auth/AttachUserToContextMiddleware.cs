using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Library.Interfaces;

namespace Library.API.Middleware.Auth
{
    public class AttachUserToContextMiddleware
    {
        private readonly RequestDelegate _next;

        public AttachUserToContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }
      
        public async Task Invoke(HttpContext context, IUserService userService)
        {

            if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {

                var email = context.User.FindFirst(AuthorizationConstants.ClaimSubject)?.Value;

                if (!string.IsNullOrEmpty(email))
                {
                    var user = userService.GetByEmail(email);

                    if (user != null)
                    {
                        context.Items["User"] = user;
                    }
                }
            }

            await _next(context);
        }
    
    }
}

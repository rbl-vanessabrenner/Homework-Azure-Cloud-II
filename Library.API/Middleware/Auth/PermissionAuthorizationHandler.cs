using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace Library.API.Middleware.Auth
{
    internal class PermissionAuthorizationHandler : AuthorizationHandler<PrivilegeRequirement>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PrivilegeRequirement requirement)
        {
            var claimsPrincipal = context.User;

            if (!AreClaimsValid(claimsPrincipal))
            {
                context.Fail();
                await Task.CompletedTask;
                return;
            }

            var userClaimsModel = ExtractUserClaims(claimsPrincipal);
            if (userClaimsModel.ClaimRoles == null)
                context.Fail();
            else
                ValidateUserPrivileges(context, requirement, userClaimsModel.ClaimRoles);

            await Task.CompletedTask;
        }

        private bool AreClaimsValid(ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal != null &&
                   claimsPrincipal.Identity != null &&
                   claimsPrincipal.Identity.IsAuthenticated &&
                   claimsPrincipal.HasClaim(c => c.Type.Equals(AuthorizationConstants.ClaimRole)) &&
                   claimsPrincipal.HasClaim(c => c.Type.Equals(AuthorizationConstants.ClaimSubject));
        }

        private UserClaimModel ExtractUserClaims(ClaimsPrincipal claimsPrincipal)
        {
            var claimRoleValues = claimsPrincipal
                .FindAll(c => c.Type.Equals(AuthorizationConstants.ClaimRole))
                .Select(c => c.Value);

            var claimEmailValue = claimsPrincipal
                .FindFirst(c => c.Type.Equals(AuthorizationConstants.ClaimSubject))?.Value ?? string.Empty;

            return new UserClaimModel
            {
                ClaimUserEmail = claimEmailValue,
                ClaimRoles = claimRoleValues
            };
        }

        private void ValidateUserPrivileges(AuthorizationHandlerContext context, PrivilegeRequirement requirement, IEnumerable<string> userClaimRoles)
        {
            if (requirement.Role == Policies.All)
                if (userClaimRoles.Contains(Policies.User) || userClaimRoles.Contains(Policies.Admin))
                {
                    context.Succeed(requirement);
                    return;
                }

            if (userClaimRoles.Contains(requirement.Role))
                context.Succeed(requirement);
            else
                context.Fail();
        }
    }
}

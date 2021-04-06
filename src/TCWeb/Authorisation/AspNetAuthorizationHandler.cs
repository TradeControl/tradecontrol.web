using System.Threading.Tasks;
using TradeControl.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Areas.Identity.Data;

namespace TradeControl.Web.Authorization
{    
    public class AspNetAdminAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, AspNet_UserRegistration>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, AspNet_UserRegistration resource)
        {
            if (context.User == null)
                return Task.CompletedTask;

            if (context.User.IsInRole(Constants.AdministratorsRole))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

    public class AspNetIsOwnerAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, AspNet_UserRegistration>
    {
        UserManager<TradeControlWebUser> _userManager;

        public AspNetIsOwnerAuthorizationHandler(UserManager<TradeControlWebUser> userManager)
        {
            _userManager = userManager;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, AspNet_UserRegistration resource)
        {
            if (context.User == null || resource == null)
                return Task.CompletedTask;

            if (requirement.Name != Constants.CreateOperationName &&
                requirement.Name != Constants.ReadOperationName &&
                requirement.Name != Constants.DeleteOperationName)
            {
                return Task.CompletedTask;
            }

            if (resource.Id == _userManager.GetUserId(context.User))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

    public class AspNetManagerAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, AspNet_UserRegistration>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, AspNet_UserRegistration resource)
        {
            if (context.User == null || resource == null)
                return Task.CompletedTask;

            if (requirement.Name != Constants.ApproveOperationName &&
                requirement.Name != Constants.RejectOperationName)
            {
                return Task.CompletedTask;
            }

            if (context.User.IsInRole(Constants.ManagersRole))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }

}

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NSS.Plugin.Misc.SwiftApi.Authorization.Requirements;

namespace NSS.Plugin.Misc.SwiftApi.Authorization.Policies
{
    public class CustomerRoleAuthorizationPolicy : AuthorizationHandler<CustomerRoleRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CustomerRoleRequirement requirement)
        {
            if (requirement.IsCustomerInRole())
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}

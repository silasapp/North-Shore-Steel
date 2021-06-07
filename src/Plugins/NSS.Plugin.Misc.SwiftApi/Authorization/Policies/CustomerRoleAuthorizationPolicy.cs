using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NSS.Plugin.Misc.SwiftApi.Authorization.Requirements;

namespace NSS.Plugin.Misc.SwiftApi.Authorization.Policies
{
    public class CustomerRoleAuthorizationPolicy : AuthorizationHandler<CustomerRoleRequirement>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CustomerRoleRequirement requirement)
        {
            if (await requirement.IsCustomerInRole())
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
    }
}

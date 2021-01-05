using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NSS.Plugin.DiscountRules.FirstTimeBuyer.Models;

namespace NSS.Plugin.DiscountRules.FirstTimeBuyer.Validators
{
    /// <summary>
    /// Represents an <see cref="RequirementModel"/> validator.
    /// </summary>
    public class RequirementModelValidator : BaseNopValidator<RequirementModel>
    {
        public RequirementModelValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.DiscountId)
                .NotEmpty()
                .WithMessage(localizationService.GetResource("Plugins.DiscountRules.FirstTimeBuyer.Fields.DiscountId.Required"));
        }
    }
}

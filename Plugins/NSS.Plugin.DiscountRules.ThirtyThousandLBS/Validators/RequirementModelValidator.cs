using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NSS.Plugin.DiscountRules.ThirtyThousandLBS.Models;

namespace NSS.Plugin.DiscountRules.ThirtyThousandLBS.Validators
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
                .WithMessage(localizationService.GetResource("Plugins.DiscountRules.ThirtyThousandLBS.Fields.DiscountId.Required"));
        }
    }
}

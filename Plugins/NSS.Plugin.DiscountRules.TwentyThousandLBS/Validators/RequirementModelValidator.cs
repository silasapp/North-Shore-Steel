using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using NSS.Plugin.DiscountRules.TwentyThousandLBS.Models;

namespace NSS.Plugin.DiscountRules.TwentyThousandLBS.Validators
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
                .WithMessage(localizationService.GetResource("Plugins.DiscountRules.TwentyThousandLBS.Fields.DiscountId.Required"));
        }
    }
}

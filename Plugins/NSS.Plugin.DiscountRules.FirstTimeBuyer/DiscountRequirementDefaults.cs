
namespace NSS.Plugin.DiscountRules.FirstTimeBuyer
{
    /// <summary>
    /// Represents constants for the discount requirement rule
    /// </summary>
    public static class DiscountRequirementDefaults
    {
        /// <summary>
        /// The system name of the discount requirement rule
        /// </summary>
        public const string SystemName = "DiscountRequirement.MustBeFirstTimeBuyer";

        /// <summary>
        /// The key of the settings to save restricted customer roles
        /// </summary>
        public const string SettingsKey = "DiscountRequirement.MustBeFirstTimeBuyer-{0}";

        /// <summary>
        /// The HTML field prefix for discount requirements
        /// </summary>
        public const string HtmlFieldPrefix = "DiscountRulesFirstTimeBuyer{0}";
    }
}

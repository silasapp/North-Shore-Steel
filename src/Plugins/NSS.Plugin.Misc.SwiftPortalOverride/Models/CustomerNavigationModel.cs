using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Models
{
    public partial record CustomerNavigationModel : BaseNopModel
    {
        public CustomerNavigationModel()
        {
            CustomerNavigationItems = new List<CustomerNavigationItemModel>();
            CompanyNavigationItems = new List<CompanyNavigationItemModel>();
        }

        public IList<CustomerNavigationItemModel> CustomerNavigationItems { get; set; }
        public IList<CompanyNavigationItemModel> CompanyNavigationItems { get; set; }

        public CustomerNavigationEnum SelectedTab { get; set; }
    }

    public record CustomerNavigationItemModel : BaseNopModel
    {
        public string RouteName { get; set; }
        public string Title { get; set; }
        public CustomerNavigationEnum Tab { get; set; }
        public string ItemClass { get; set; }
        public string ItemLogo { get; set; }
    }

    public record CompanyNavigationItemModel : BaseNopModel
    {
        public string RouteName { get; set; }
        public string Title { get; set; }
        public CustomerNavigationEnum Tab { get; set; }
        public string ItemClass { get; set; }
        public string ItemLogo { get; set; }
    }

    public enum CustomerNavigationEnum
    {
        Info = 0,
        Addresses = 10,
        Orders = 20,
        BackInStockSubscriptions = 30,
        ReturnRequests = 40,
        DownloadableProducts = 50,
        RewardPoints = 60,
        ChangePassword = 70,
        Avatar = 80,
        ForumSubscriptions = 90,
        ProductReviews = 100,
        VendorInfo = 110,
        GdprTools = 120,
        CheckGiftCardBalance = 130,
        NotificationPreferences = 140
    }
}
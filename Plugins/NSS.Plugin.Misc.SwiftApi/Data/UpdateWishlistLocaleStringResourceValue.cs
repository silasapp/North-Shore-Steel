using FluentMigrator;
using Nop.Data.Migrations;

namespace NSS.Plugin.Misc.SwiftApi.Data
{
    [NopMigration("2021/02/26 18:00:00", "Swift.Api Update Wishlist Value to Favorite in LocaleStringResource")]
    public class UpdateWishlistLocaleStringResourceValue : Migration
    {
        public override void Up()
        {
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'The product has been added to your <a href=\"{0}\">favorite items</a>.' Where [ResourceName] = 'products.producthasbeenaddedtothewishlist.link'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Favorite Items' Where [ResourceName] = 'pagetitle.wishlist'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'The product has been added to your favorite items.' Where [ResourceName] = 'products.producthasbeenaddedtothewishlist'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Favorite Items' Where [ResourceName] = 'wishlist'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Add to favorites' Where [ResourceName] = 'products.wishlist.addtowishlist'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Add to favorites' Where [ResourceName] = 'shoppingcart.addtowishlist'");
        }

        public override void Down()
        {
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'The product has been added to your <a href=\"{0}\">wishlist</a>' Where [ResourceName] = 'products.producthasbeenaddedtothewishlist.link'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Wishlist' Where [ResourceName] = 'pagetitle.wishlist'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'The product has been added to your wishlist' Where [ResourceName] = 'products.producthasbeenaddedtothewishlist'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Wishlist' Where [ResourceName] = 'wishlist'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Add to wishlist' Where [ResourceName] = 'products.wishlist.addtowishlist'");
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Add to wishlist' Where [ResourceName] = 'shoppingcart.addtowishlist'");
        }
    }
}

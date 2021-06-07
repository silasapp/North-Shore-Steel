using FluentMigrator;
using Nop.Data.Migrations;

namespace NSS.Plugin.Misc.SwiftApi.Data
{
    [NopMigration("2021/01/25 09:26:00", "Swift.Api Update ShoppingCart.ProductUnpublished Value in LocaleStringResource")]
    public class UpdateProductUnpublishedLocaleStringResourceValue : Migration
    {
        public override void Up()
        {
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Product is no longer available' Where [ResourceName] = 'ShoppingCart.ProductUnpublished'");
        }

        public override void Down()
        {
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Product is not published' Where [ResourceName] = 'ShoppingCart.ProductUnpublished'");
        }
    }
}

using FluentMigrator;
using Nop.Data.Migrations;

namespace NSS.Plugin.Misc.SwiftApi.Data
{
    [NopMigration("2021/01/19 13:03:00", "Swift.Api Update ShoppingCart.Totals.SubTotal Value in LocaleStringResource")]
    public class UpdateShopppingCartTotalsSubtotalLocaleStringResourceValue : Migration
    {
        public override void Up()
        {
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'SubTotal' Where [ResourceName] = 'ShoppingCart.Totals.SubTotal'");
        }

        public override void Down()
        {
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Sub-Total' Where [ResourceName] = 'ShoppingCart.Totals.SubTotal'");
        }
    }
}

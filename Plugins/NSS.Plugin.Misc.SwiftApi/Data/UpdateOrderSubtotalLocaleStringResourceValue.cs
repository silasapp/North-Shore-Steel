using FluentMigrator;
using Nop.Data.Migrations;

namespace NSS.Plugin.Misc.SwiftApi.Data
{
    [NopMigration("2021/01/19 10:36:00", "Swift.Api Update Order.Subtotal Value in LocaleStringResource")]
    public class UpdateOrderSubtotalLocaleStringResourceValue : Migration
    {
        public override void Up()
        {
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'SubTotal' Where [ResourceName] = 'Order.SubTotal'");
        }

        public override void Down()
        {
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Sub-Total' Where [ResourceName] = 'Order.SubTotal'");
        }
    }
}

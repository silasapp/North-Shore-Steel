using FluentMigrator;
using Nop.Data.Migrations;

namespace NSS.Plugin.Misc.SwiftApi.Data
{
    [NopMigration("2021/01/21 01:05:00", "Swift.Api Update Order.Subtotal Value in LocaleStringResource")]
    public class UpdateOrderSubtotalLocaleStringResourceValue : Migration
    {
        public override void Up()
        {
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Subtotal' Where [ResourceName] = 'Order.SubTotal'");
        }

        public override void Down()
        {
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Sub-Total' Where [ResourceName] = 'Order.SubTotal'");
        }
    }
}

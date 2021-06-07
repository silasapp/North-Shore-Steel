using FluentMigrator;
using Nop.Data.Migrations;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftApi.Data
{
    [NopMigration("2021/05/20 15:41:00", "Swift.Api Update Register Button Text")]
    public class UpdateRegisterButtonText : Migration
    {
        public override void Up()
        {
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Register now!' Where [ResourceName] = 'Account.Register'");
        }

        public override void Down()
        {
            Execute.Sql("Update [LocaleStringResource] Set [ResourceValue] = 'Register' Where [ResourceName] = 'Account.Register'");
        }
    }
}

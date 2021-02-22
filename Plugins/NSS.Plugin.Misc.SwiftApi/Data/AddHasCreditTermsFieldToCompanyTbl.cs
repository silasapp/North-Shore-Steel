using FluentMigrator;
using Nop.Data.Migrations;

namespace NSS.Plugin.Misc.SwiftApi.Data
{
    [NopMigration("2021/02/22 18:00:00", "Swift.Api Add HasCreditTerms Field to Company Table")]
    public class AddHasCreditTermsFieldToCompanyTbl : AutoReversingMigration
    {
        public override void Up()
        {
            if (Schema.Table("Company").Exists() && !Schema.Table("Company").Column("HasCreditTerms").Exists())
            {
                Alter
                    .Table("Company")
                    .AddColumn("HasCreditTerms")
                    .AsBoolean()
                    .WithDefaultValue(false);
            }
        }
    }
}

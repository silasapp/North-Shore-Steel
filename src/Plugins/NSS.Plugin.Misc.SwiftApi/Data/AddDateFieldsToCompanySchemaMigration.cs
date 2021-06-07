using FluentMigrator;
using FluentMigrator.Builders.Delete.Column;
using Nop.Data.Migrations;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftApi.Data
{
    [NopMigration("2020/12/08 15:00:00", "Swift.Api Add Date Fields to Company Schema")]
    public class AddDateFieldsToCompanySchemaMigration : AutoReversingMigration
    {
        public override void Up()
        {
            //if (Schema.Table("Company").Column("SalesContactLiveChatId").Exists())
            //{
            //    Schema.Table("Company").dr("SalesContactLiveChatId").Exists()

            //    //Rename
            //    //    .Column("SalesContactLiveChatId").OnTable("Company").To("SalesContactImageUrl");

            //}

            if (!Schema.Table("Company").Column("SalesContactImageUrl").Exists())
            {
                Alter
                    .Table("Company")
                    .AddColumn("SalesContactImageUrl")
                    .AsString()
                    .Nullable();
            }


            if (!Schema.Table("Company").Column("CreatedOnUtc").Exists())
            {
                Alter
                    .Table("Company")
                    .AddColumn("CreatedOnUtc")
                    .AsDateTime()
                    .NotNullable()
                    .WithDefaultValue(DateTime.UtcNow);
            }

            if (!Schema.Table("Company").Column("UpdatedOnUtc").Exists())
            {
                Alter
                    .Table("Company")
                    .AddColumn("UpdatedOnUtc")
                    .AsDateTime()
                    .NotNullable()
                    .WithDefaultValue(DateTime.UtcNow);

            }
        }
    }
}

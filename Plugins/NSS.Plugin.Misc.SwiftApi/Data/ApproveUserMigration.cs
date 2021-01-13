﻿using FluentMigrator;
using Nop.Data.Migrations;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;

namespace NSS.Plugin.Misc.SwiftApi.Data
{
    [NopMigration("2020/11/04 19:20:00", "Swift.Api Approve user schema")]
    public class ApproveUserMigration : AutoReversingMigration
    {

        #region Fields

        protected IMigrationManager _migrationManager;

        #endregion

        #region Ctor

        public ApproveUserMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            if (!Schema.Table("Company").Exists())
            {
                _migrationManager.BuildTable<Company>(Create);
            }

            if (!Schema.Table("CustomerCompany").Exists())
            {
                _migrationManager.BuildTable<CustomerCompany>(Create);


                var compKey = new[] { "CustomerId", "CompanyId" };
                Create.UniqueConstraint("Customer_Company_Unique").OnTable("CustomerCompany").Columns(compKey);

                Create.ForeignKey("FK_CustomerCompany_Customer").FromTable("CustomerCompany").ForeignColumn("CustomerId").ToTable("Customer").PrimaryColumn("Id");
                Create.ForeignKey("FK_CustomerCompany_Company").FromTable("CustomerCompany").ForeignColumn("CompanyId").ToTable("Company").PrimaryColumn("Id");
            }

            /** Commented out as per Chika's advice */
            //if (!Schema.Table("CustomerCompany").Column("CanCredit").Exists())
            //{

            //    Alter
            //        .Table("CustomerCompany")
            //        .AddColumn("CanCredit")
            //        .AsBoolean()
            //        .WithDefaultValue("");

            //}
        }
        #endregion
    }
}

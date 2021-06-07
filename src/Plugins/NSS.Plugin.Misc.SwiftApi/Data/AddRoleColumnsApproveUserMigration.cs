using FluentMigrator;
using Nop.Data.Migrations;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;

namespace NSS.Plugin.Misc.SwiftApi.Data
{
    [NopMigration("2020/11/29 15:20:00", "Swift.Api Add Role Columns Approve user schema")]
    public class AddRoleColumnsApproveUserMigration : AutoReversingMigration
    {

        #region Fields

        protected IMigrationManager _migrationManager;

        #endregion

        #region Ctor

        public AddRoleColumnsApproveUserMigration(IMigrationManager migrationManager)
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

            if (!Schema.Table("CustomerCompany").Column("CanCredit").Exists())
            {

                Alter
                    .Table("CustomerCompany")
                    .AddColumn("CanCredit")
                    .AsBoolean()
                    .WithDefaultValue(false);

            }

            if (!Schema.Table("CustomerCompany").Column("AP").Exists())
            {
                Alter.Table("CustomerCompany").AddColumn("AP").AsBoolean().WithDefaultValue(false); ;
            }
            if (!Schema.Table("CustomerCompany").Column("Buyer").Exists())
            {
                Alter.Table("CustomerCompany").AddColumn("Buyer").AsBoolean().WithDefaultValue(false); ;
            }
            if (!Schema.Table("CustomerCompany").Column("Operations").Exists())
            {
                Alter.Table("CustomerCompany").AddColumn("Operations").AsBoolean().WithDefaultValue(false); ;
            }
        }
        #endregion
    }
}

using FluentMigrator;
using Nop.Data.Migrations;
using NSS.Plugin.Misc.SwiftApi.Domain.Customers;

namespace NSS.Plugin.Misc.SwiftApi.Data
{
    [NopMigration("2020/09/21 14:00:00", "Swift.Api Approve user schema")]
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
            _migrationManager.BuildTable<Company>(Create);
            _migrationManager.BuildTable<CustomerCompany>(Create);

            var compKey = new[] { "CustomerId", "CompanyId" };
            Create.UniqueConstraint("Customer_Company_Unique").OnTable("CustomerCompany").Columns(compKey);

            Create.ForeignKey("FK_CustomerCompany_Customer").FromTable("CustomerCompany").ForeignColumn("CustomerId").ToTable("Customer").PrimaryColumn("Id");
            Create.ForeignKey("FK_CustomerCompany_Company").FromTable("CustomerCompany").ForeignColumn("CompanyId").ToTable("Company").PrimaryColumn("Id");
        }
        #endregion
    }
}

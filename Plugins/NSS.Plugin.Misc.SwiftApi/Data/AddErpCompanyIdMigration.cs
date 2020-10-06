using FluentMigrator;
using Nop.Data.Migrations;
using NSS.Plugin.Misc.SwiftApi.Domain.Customers;

namespace NSS.Plugin.Misc.SwiftApi.Data
{
    [NopMigration("2020/09/25 14:00:00", "Swift.Api Add ERP company")]
    public class AddErpCompanyMigration : AutoReversingMigration
    {

        #region Fields

        protected IMigrationManager _migrationManager;

        #endregion

        #region Ctor

        public AddErpCompanyMigration(IMigrationManager migrationManager)
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
            Create.Column("ErpCompanyId").OnTable("Company").AsInt64().WithDefaultValue(0);
            
        }
        #endregion
    }
}

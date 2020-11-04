using FluentMigrator;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Data.Migrations;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;

namespace NSS.Plugin.Misc.SwiftApi.Data
{
    [NopMigration("2020/11/04 12:30:00", "Swift.Api Create Customer Company Products schema")]
    public class CreateCustomerCompanyProductsMigration : AutoReversingMigration
    {

        #region Fields

        protected IMigrationManager _migrationManager;

        #endregion

        #region Ctor

        public CreateCustomerCompanyProductsMigration(IMigrationManager migrationManager)
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
            _migrationManager.BuildTable<CustomerCompanyProduct>(Create);


            Create.ForeignKey("FK_CustomerCompanyProduct_Product").FromTable("CustomerCompanyProduct").ForeignColumn("ProductId").ToTable("Product").PrimaryColumn("Id");
            Create.ForeignKey("FK_CustomerCompanyProduct_CustomerCompany").FromTable("CustomerCompanyProduct").ForeignColumn("CustomerCompanyId").ToTable("CustomerCompany").PrimaryColumn("Id");
        }
        #endregion
    }


    public class CreateCustomerCompanyProductBuilder : NopEntityBuilder<CustomerCompanyProduct>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(CustomerCompanyProduct.Id))
                .AsInt32()
                .NotNullable()
                .PrimaryKey();
        }
    }
}

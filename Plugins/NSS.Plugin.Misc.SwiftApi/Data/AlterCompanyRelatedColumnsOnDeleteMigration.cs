using FluentMigrator;
using Nop.Data.Migrations;

namespace NSS.Plugin.Misc.SwiftApi.Data
{
    [NopMigration("2020/12/08 16:00:00", "Swift.Api Alter Company Related Columns On Delete")]
    public class AlterCompanyRelatedColumnsOnDeleteMigration : Migration
    {
        public override void Down()
        {
            Create.ForeignKey("FK_CustomerCompany_Customer").FromTable("CustomerCompany").ForeignColumn("CustomerId").ToTable("Customer").PrimaryColumn("Id").OnDeleteOrUpdate(System.Data.Rule.Cascade);
            Create.ForeignKey("FK_CustomerCompany_Company").FromTable("CustomerCompany").ForeignColumn("CompanyId").ToTable("Company").PrimaryColumn("Id").OnDeleteOrUpdate(System.Data.Rule.Cascade);

            Create.ForeignKey("FK_CustomerCompanyProduct_Product").FromTable("CustomerCompanyProduct").ForeignColumn("ProductId").ToTable("Product").PrimaryColumn("Id").OnDeleteOrUpdate(System.Data.Rule.Cascade);
            Create.ForeignKey("FK_CustomerCompanyProduct_CustomerCompany").FromTable("CustomerCompanyProduct").ForeignColumn("CustomerCompanyId").ToTable("CustomerCompany").PrimaryColumn("Id").OnDeleteOrUpdate(System.Data.Rule.Cascade);
        }

        public override void Up()
        {
            if (Schema.Table("CustomerCompany").Constraint("FK_CustomerCompany_Customer").Exists())
            {
                Delete.ForeignKey("FK_CustomerCompany_Customer").OnTable("CustomerCompany");
                Delete.ForeignKey("FK_CustomerCompany_Company").OnTable("CustomerCompany");

                Create.ForeignKey("FK_CustomerCompany_Customer").FromTable("CustomerCompany").ForeignColumn("CustomerId").ToTable("Customer").PrimaryColumn("Id").OnDeleteOrUpdate(System.Data.Rule.Cascade);
                Create.ForeignKey("FK_CustomerCompany_Company").FromTable("CustomerCompany").ForeignColumn("CompanyId").ToTable("Company").PrimaryColumn("Id").OnDeleteOrUpdate(System.Data.Rule.Cascade);
            }

            if (Schema.Table("CustomerCompanyProduct").Constraint("FK_CustomerCompanyProduct_Product").Exists())
            {
                Delete.ForeignKey("FK_CustomerCompanyProduct_Product").OnTable("CustomerCompanyProduct");
                Delete.ForeignKey("FK_CustomerCompanyProduct_CustomerCompany").OnTable("CustomerCompanyProduct");

                Create.ForeignKey("FK_CustomerCompanyProduct_Product").FromTable("CustomerCompanyProduct").ForeignColumn("ProductId").ToTable("Product").PrimaryColumn("Id").OnDeleteOrUpdate(System.Data.Rule.Cascade);
                Create.ForeignKey("FK_CustomerCompanyProduct_CustomerCompany").FromTable("CustomerCompanyProduct").ForeignColumn("CustomerCompanyId").ToTable("CustomerCompany").PrimaryColumn("Id").OnDeleteOrUpdate(System.Data.Rule.Cascade);
            }

        }
    }
}

﻿using FluentMigrator;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Data.Migrations;
using NSS.Plugin.Misc.SwiftCore.Domain.Shapes;

namespace NSS.Plugin.Misc.SwiftApi.Data
{
    [NopMigration("2020/09/23 14:00:00", "Swift.Api Create Shapes schema")]
    public class CreateShapesMigration : AutoReversingMigration
    {

        #region Fields

        protected IMigrationManager _migrationManager;

        #endregion

        #region Ctor

        public CreateShapesMigration(IMigrationManager migrationManager)
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
            _migrationManager.BuildTable<ShapeAttribute>(Create);
            _migrationManager.BuildTable<Shape>(Create);


            Create.ForeignKey("FK_ShapeAttribute_Shape").FromTable("ShapeAttribute").ForeignColumn("ShapeId").ToTable("Shape").PrimaryColumn("Id");
            Create.ForeignKey("FK_Shape_Shape").FromTable("Shape").ForeignColumn("ParentId").ToTable("Shape").PrimaryColumn("Id");
        }
        #endregion
    }


    public class ShapeBuilder : NopEntityBuilder<Shape>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Shape.Id))
                .AsInt32()
                .NotNullable()
                .PrimaryKey();
        }
    }
}
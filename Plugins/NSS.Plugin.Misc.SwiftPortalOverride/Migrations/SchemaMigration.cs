﻿using FluentMigrator;
using Nop.Data.Migrations;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Migrations
{
    [SkipMigrationOnUpdate]
    [NopMigration("", "NSS.Plugin.Misc.SwiftPortalOverride schema")]
    public class SchemaMigration : AutoReversingMigration
    {
        private readonly IMigrationManager _migrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
        }
    }
}
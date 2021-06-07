using FluentMigrator;
using Nop.Data.Migrations;
using NSS.Plugin.Misc.SwiftCore.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace NSS.Plugin.Misc.SwiftApi.Data
{
    [NopMigration("2020/11/24 12:00:00", "Swift.Api.CreateUserRegistrationSchema")]
    public class CreateUserRegistrationSchema : AutoReversingMigration
    {
        #region Fields

        protected IMigrationManager _migrationManager;

        #endregion

        #region Ctor

        public CreateUserRegistrationSchema(IMigrationManager migrationManager)
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
            _migrationManager.BuildTable<UserRegistration>(Create);
        }
        #endregion
    }
}

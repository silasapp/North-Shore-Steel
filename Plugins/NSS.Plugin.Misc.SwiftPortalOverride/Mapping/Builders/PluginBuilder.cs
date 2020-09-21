using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using NSS.Plugin.Misc.SwiftPortalOverride.Domains;

namespace NSS.Plugin.Misc.SwiftPortalOverride.Mapping.Builders
{
    public class PluginBuilder : NopEntityBuilder<CustomTable>
    {
        #region Methods

        public override void MapEntity(CreateTableExpressionBuilder table)
        {
        }

        #endregion
    }
}
using Microsoft.EntityFrameworkCore;

namespace TradeControl.Web.Data
{
    public partial class NodeContext
    {
        // Register the convention so the model will be updated before finalization.
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);

            // Add the trigger-adding convention (uses the default dependencies)
            configurationBuilder.Conventions.Add(_ => new BlankTriggerAddingConvention());
        }
    }
}

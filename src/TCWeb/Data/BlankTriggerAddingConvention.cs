using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;

namespace TradeControl.Web.Data
{

    /// <summary>
    /// A convention that adds a synthetic trigger name for each mapped table in the model.
    /// </summary>
    /// <remarks>This convention ensures that each physical table (identified by its table name and schema) 
    /// has a corresponding trigger name. It avoids duplicates by tracking already processed tables  and uses stable
    /// public APIs to read table metadata, ensuring compatibility across EF Core versions.</remarks>
    public class BlankTriggerAddingConvention : IModelFinalizingConvention
    {
        public void ProcessModelFinalizing(
            IConventionModelBuilder modelBuilder,
            IConventionContext<IConventionModelBuilder> context)
        {
            // Register one trigger per physical table (tableName + schema) to avoid duplicates
            var addedTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                if (entityType.IsKeyless)
                    continue;

                // Read the relational table name from the annotation (works across EF Core versions)
                var tableNameAnn = entityType.FindAnnotation("Relational:TableName")?.Value;
                var tableSchemaAnn = entityType.FindAnnotation("Relational:Schema")?.Value;

                var tableName = tableNameAnn as string;
                var tableSchema = tableSchemaAnn as string;
                if (string.IsNullOrEmpty(tableName))
                    continue;

                var tableKey = tableSchema != null ? $"{tableSchema}.{tableName}" : tableName;
                if (!addedTables.Add(tableKey))
                    continue; 

                var triggerName = tableName + "_Trigger";
                entityType.Builder.HasTrigger(triggerName);
            }
        }
    }
}
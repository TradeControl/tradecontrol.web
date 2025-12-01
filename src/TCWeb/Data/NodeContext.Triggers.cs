using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TradeControl.Web.Data
{
	public partial class NodeContext
	{
		// Mark mapped tables as having triggers so EF Core SQL Server provider will avoid
		// generating OUTPUT ... (without INTO) statements that fail when triggers exist.
		partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
		{
			// Explicit list: add only the tables you know have triggers.
			// Start with the failing one.
			//modelBuilder.Entity<TradeControl.Web.Models.App_tbTaxCode>().HasTrigger("EFCore_MarkTrigger_App_tbTaxCode");

			// If other specific tables fail, add similar lines here:
			// modelBuilder.Entity<TradeControl.Web.Models.Subject_tbAccount>().HasTrigger("EFCore_MarkTrigger_Subject_tbAccount");
			// modelBuilder.Entity<TradeControl.Web.Models.Cash_tbPayment>().HasTrigger("EFCore_MarkTrigger_Cash_tbPayment");

			// Automatic approach: mark all mapped tables as having triggers.
			/*
			foreach (var entityType in modelBuilder.Model.GetEntityTypes())
			{
				// Skip keyless types / views / query types
				if (entityType.IsKeyless)
					continue;

				// Skip types without CLR mapping
				var clrType = entityType.ClrType;
				if (clrType == null)
					continue;

				// Only when mapped to a table
				var tableName = entityType.GetTableName();
				if (string.IsNullOrEmpty(tableName))
					continue;

				var schema = entityType.GetSchema() ?? "dbo";
				var triggerName = $"EFCore_MarkTrigger_{schema}_{tableName}";

				// Preferred: mark via the fluent API (keeps model builder semantics)
				modelBuilder.Entity(clrType).HasTrigger(triggerName);

				// Extra: ensure the relational trigger annotation is present on the mutable entity metadata.
				// This forces the built model to include trigger metadata even if HasTrigger didn't stick.
				// Annotation key mirrors EF Core's relational trigger metadata.
				const string triggersAnnotationKey = "Relational:Triggers";

				// If an annotation already exists, append our marker if not present.
				var existing = entityType.FindAnnotation(triggersAnnotationKey)?.Value as string[];
				if (existing == null || !existing.Contains(triggerName))
				{
					var newValue = existing == null ? new[] { triggerName } : existing.Concat(new[] { triggerName }).ToArray();
					// IMutableAnnotatable.AddAnnotation is available on the returned entityType (mutable model during OnModelCreating)
					entityType.AddAnnotation(triggersAnnotationKey, newValue);
				}
			}
			*/

		}
	}
}
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TradeControl.Web.Models;

namespace TradeControl.Web.Data
{
    public partial class NodeContext
    {
        public IQueryable<Subject_fnCurrentAgedInvoice> SubjectCurrentAgedInvoices(DateTime agedOn) =>
            FromExpression(() => SubjectCurrentAgedInvoices(agedOn));

        public IQueryable<Subject_fnCurrentAgedInvoiceItem> SubjectCurrentAgedInvoiceItems(DateTime agedOn) =>
            FromExpression(() => SubjectCurrentAgedInvoiceItems(agedOn));

        public IQueryable<Subject_fnDatedBalance> SubjectDatedBalances(DateTime asOfDate) =>
            FromExpression(() => SubjectDatedBalances(asOfDate));

        private static void ConfigureSubjectBalanceFunctions(ModelBuilder modelBuilder)
        {
            var currentInvoice = modelBuilder.Entity<Subject_fnCurrentAgedInvoice>();
            currentInvoice.HasNoKey();
            currentInvoice.Property(row => row.BusinessBalance).HasPrecision(18, 5);
            currentInvoice.Property(row => row.CurrentAmount).HasPrecision(18, 5);
            currentInvoice.Property(row => row.Days1To30Amount).HasPrecision(18, 5);
            currentInvoice.Property(row => row.Days31To60Amount).HasPrecision(18, 5);
            currentInvoice.Property(row => row.Days61To90Amount).HasPrecision(18, 5);
            currentInvoice.Property(row => row.Over90Amount).HasPrecision(18, 5);
            currentInvoice.Property(row => row.StatementBusinessBalance).HasPrecision(18, 5);
            currentInvoice.Property(row => row.ReconciliationResidual).HasPrecision(18, 5);

            var currentItem = modelBuilder.Entity<Subject_fnCurrentAgedInvoiceItem>();
            currentItem.HasNoKey();
            currentItem.Property(row => row.BusinessAmount).HasPrecision(18, 5);

            var datedBalance = modelBuilder.Entity<Subject_fnDatedBalance>();
            datedBalance.HasNoKey();
            datedBalance.Property(row => row.NativeStatementBalance).HasPrecision(18, 5);
            datedBalance.Property(row => row.BusinessBalance).HasPrecision(18, 5);
            datedBalance.Property(row => row.HumanBalance).HasPrecision(18, 5);

            modelBuilder
                .HasDbFunction(typeof(NodeContext).GetMethod(
                    nameof(SubjectCurrentAgedInvoices), [typeof(DateTime)])!)
                .HasName("fnCurrentAgedInvoices")
                .HasSchema("Subject");

            modelBuilder
                .HasDbFunction(typeof(NodeContext).GetMethod(
                    nameof(SubjectCurrentAgedInvoiceItems), [typeof(DateTime)])!)
                .HasName("fnCurrentAgedInvoiceItems")
                .HasSchema("Subject");

            modelBuilder
                .HasDbFunction(typeof(NodeContext).GetMethod(
                    nameof(SubjectDatedBalances), [typeof(DateTime)])!)
                .HasName("fnDatedBalances")
                .HasSchema("Subject");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using TradeControl.Web.Data;

namespace TradeControl.Web.Pages.Invoice.Register.Models
{
    public sealed record InvoiceRegisterYearOption(
        short YearNumber,
        string Description);

    public sealed record InvoiceRegisterPeriodOption(
        short YearNumber,
        short MonthNumber,
        DateTime StartOn,
        string Description,
        NodeEnum.CashStatus CashStatus);

    public sealed record InvoiceRegisterSelectOption(
        string Value,
        string Label);

    public sealed record InvoiceRegisterInvoiceTypeOption(
        short Value,
        string Label);

    public static class InvoiceRegisterOptionCatalog
    {
        public static string GetYearLabel(
            IReadOnlyList<InvoiceRegisterYearOption> years,
            short? selectedYearNumber)
        {
            if (!selectedYearNumber.HasValue || selectedYearNumber.Value <= 0)
            {
                return "All years";
            }

            return years.FirstOrDefault(year => year.YearNumber == selectedYearNumber.Value)?.Description
                ?? selectedYearNumber.Value.ToString();
        }

        public static string GetPeriodScopeLabel(
            IReadOnlyList<InvoiceRegisterPeriodOption> periods,
            DateTime? selectedPeriodStartOn)
        {
            if (!selectedPeriodStartOn.HasValue)
            {
                return "All months in year";
            }

            return periods.FirstOrDefault(period => period.StartOn == selectedPeriodStartOn.Value)?.Description
                ?? selectedPeriodStartOn.Value.ToString("d");
        }

        public static IReadOnlyList<InvoiceRegisterInvoiceTypeOption> GetInvoiceTypeOptions()
        {
            return new[]
            {
                new InvoiceRegisterInvoiceTypeOption((short)NodeEnum.InvoiceType.SalesInvoice, "Sales invoice"),
                new InvoiceRegisterInvoiceTypeOption((short)NodeEnum.InvoiceType.CreditNote, "Credit note"),
                new InvoiceRegisterInvoiceTypeOption((short)NodeEnum.InvoiceType.PurchaseInvoice, "Purchase invoice"),
                new InvoiceRegisterInvoiceTypeOption((short)NodeEnum.InvoiceType.DebitNote, "Debit note")
            };
        }
    }
}

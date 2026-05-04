namespace TradeControl.Web.AppServices.Execution
{
    public class SyntheticDatasetExecutionArguments
    {
        public bool IsCompany { get; set; } = true;

        public bool UseStdCompanyTemplate { get; set; }

        public bool? IsVatRegistered { get; set; }

        public int MisOrdersPerMonth { get; set; } = 2;

        public int MonthsForward { get; set; } = 3;

        public decimal PriceRatio { get; set; } = 1.0000000m;

        public decimal QuantityRatio { get; set; } = 1.0000000m;

        public decimal FloatRatio { get; set; } = 0.25m;
    }
}

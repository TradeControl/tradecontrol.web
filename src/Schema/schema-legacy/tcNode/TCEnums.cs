namespace TradeControl.Node
{
    public enum AuthenticationMode { Windows, SqlServer };
    public enum DemoInstallMode { Activities, Orders, Invoices, Payments };
    public enum TransmitStatus { Disconnected, Deploy, Update, Processed };
    public enum EventType { Error, Warning, Information, PriceChange, Reschedule, Delivered, StatusChange, Payment };
    public enum InvoiceType { SalesInvoice, CreditNote, PurchaseInvoice, DebitNote };
    public enum CashMode { Expense, Income, Neutral};
    public enum CoinType { Main, TestNet, Fiat };
    public enum CoinChangeType { Receipt, Change };
    public enum CoinChangeStatus {  Unused, Paid, Spent  };
    public enum TxStatus { Received, UTXO, Spent };
}

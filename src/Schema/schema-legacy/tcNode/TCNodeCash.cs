using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeControl.Node
{
    public class TCNodeCash : dbNodeCashDataContext
    {
        public TCNodeCash(string connection) : base(connection)
        {
            proc_WalletInitialise();
        }


        public string RootName
        {
            get
            {
                TCNodeNetwork tcNodeNetwork = new TCNodeNetwork(Connection.ConnectionString);
                return tcNodeNetwork.NetworkName.Replace(' ', '_');
            }
        }

        public List<string> CashAccountCodes
        {
            get
            {
                return (from tb in this.vwWallets orderby tb.AccountCode select tb.AccountCode).ToList<string>();
            }
        }

        public string CashAccountTrade
        {
            get
            {
                var v = vwInvoicedReceipts;

                return (from tb in this.vwWallets where tb.CashCode != null select tb.AccountCode).FirstOrDefault();
            }
        }

        public CoinType GetCoinType(string accountCode)
        {
            var coinType = vwWallets.Where(w => w.AccountCode == accountCode).Select(acc => acc.CoinTypeCode).First();
            return (CoinType)coinType;
        }

        public bool AddReceiptKey(string accountCode, string paymentAddress, string hdPath, int addressIndex, string notes)
        {
            return AddReceiptKey(accountCode, paymentAddress, hdPath, addressIndex, null, notes);
        }

        public bool AddReceiptKey(string accountCode, string paymentAddress, string keyName, int addressIndex, string notes, string invoiceNumber)
        {
            try
            {
                int rc = proc_ChangeNew(accountCode, keyName, (short?)CoinChangeType.Receipt, paymentAddress, addressIndex, invoiceNumber, notes);
                return rc == 0;
            }
            catch (Exception err)
            {
                string logCode = string.Empty;
                proc_EventLog(err.Message, (short)EventType.Error, ref logCode);
                return false;
            }
        }

        public bool DeleteChangeKey(string paymentAddress)
        {
            try
            {
                int rc = proc_ChangeDelete(paymentAddress);
                return rc == 0;
            }
            catch (Exception err)
            {
                string logCode = string.Empty;
                proc_EventLog(err.Message, (short)EventType.Error, ref logCode);
                return false;
            }
        }

        public bool ChangeKeyNote(string paymentAddress, string note)
        {
            try
            {
                int rc = proc_ChangeNote(paymentAddress, note);
                return rc == 0;
            }
            catch (Exception err)
            {
                string logCode = string.Empty;
                proc_EventLog(err.Message, (short)EventType.Error, ref logCode);
                return false;
            }
        }

        public bool TxPayIn(string accountCode, string paymentAddress, string txId, string subjectCode, string cashCode, DateTime paidOn, string paymentReference)
        {
            try
            {
                string paymentCode = string.Empty;
                proc_TxPayIn(accountCode, paymentAddress, txId, subjectCode, cashCode, paidOn, paymentReference, ref paymentCode);
                return paymentCode.Length > 0;
            }
            catch (Exception err)
            {
                string logCode = string.Empty;
                proc_EventLog(err.Message, (short)EventType.Error, ref logCode);
                return false;
            }
        }

        public Task<double> NamespaceBalance (string accountCode, string keyName)
        {
            return Task.Run(() =>
            {
                try
                {
                    double balance = (double)fnNamespaceBalance(accountCode, keyName);
                    return balance;
                }
                catch (Exception err)
                {
                    string logCode = string.Empty;
                    proc_EventLog(err.Message, (short)EventType.Error, ref logCode);
                    return 0;
                }
            });
        }

        public Task<double> KeyNameBalance(string accountCode, string keyName)
        {
            return Task.Run(() =>
            {
                try
                {
                    double balance = (double)fnKeyNameBalance(accountCode, keyName);
                    return balance;
                }
                catch (Exception err)
                {
                    string logCode = string.Empty;
                    proc_EventLog(err.Message, (short)EventType.Error, ref logCode);
                    return 0;
                }
            });
        }

        public Task<double> AccountBalance(string subjectCode)
        {
            return Task.Run(() =>
            {
                try
                {
                    decimal? balance = 0;
                    proc_BalanceToPay(subjectCode, ref balance);
                    return (double)balance * -1;
                }
                catch (Exception err)
                {
                    string logCode = string.Empty;
                    proc_EventLog(err.Message, (short)EventType.Error, ref logCode);
                    return 0;
                }
            });
        }
    }
}
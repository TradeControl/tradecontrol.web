using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

using System.Text;
using System.Threading.Tasks;

namespace TradeControl.Node
{
    public class TCNodeNetwork : dbNodeNetworkDataContext
    {
        public TCNodeNetwork(string connection) : base(connection) { }

        #region network
        public string NetworkName
        {
            get
            {
                try
                {
                    var owner = (from org in tbSubjects
                                 from opt in tbOptions
                                 where org.SubjectCode == opt.SubjectCode
                                 select org.SubjectName).First().ToString();
                    return owner;
                }
                catch (Exception err)
                {
                    string logCode = string.Empty;
                    proc_EventLog(err.Message, (short)EventType.Error, ref logCode);
                    return string.Empty;
                }
            }
        }

        public string UnitOfAccount
        {
            get
            {
                try
                {
                    var uoc = (from opt in tbOptions
                               select opt.UnitOfCharge != null ? opt.UnitOfCharge : string.Empty).First().ToString();
                    return uoc;
                }
                catch (Exception err)
                {
                    string logCode = string.Empty;
                    proc_EventLog(err.Message, (short)EventType.Error, ref logCode);
                    return string.Empty;
                }
            }
        }

        public Task<bool> AddNetworkProvider(string networkProvider, string publicKey, string privateKey, string consortiumAddress)
        {
            return Task.Run(() =>
            {
                try
                {
                    var provider = (from tb in tbEths where tb.NetworkProvider == networkProvider select tb).Select(tb => tb).FirstOrDefault();

                    if (provider == null)
                    {
                        tbEths.InsertOnSubmit(new tbEth { NetworkProvider = networkProvider, PublicKey = publicKey, PrivateKey = privateKey, ConsortiumAddress = consortiumAddress });
                        SubmitChanges();
                    }
                    else if (publicKey != provider.PublicKey || privateKey != provider.PrivateKey || (consortiumAddress != provider.ConsortiumAddress))
                    {
                        provider.PublicKey = publicKey.Length > 0 ? publicKey : provider.PublicKey;
                        provider.PrivateKey = privateKey.Length > 0 ? privateKey : provider.PrivateKey;
                        provider.ConsortiumAddress = consortiumAddress.Length > 0 ? consortiumAddress : provider.ConsortiumAddress;
                        SubmitChanges();
                    }
                    return true;
                }
                catch (Exception err)
                {
                    string logCode = string.Empty;
                    proc_EventLog(err.Message, (short)EventType.Error, ref logCode);
                    return false;
                }
            });
        }
        #endregion

        #region membership
        public Dictionary<string, string> Members
        {
            get
            {
                try
                {
                    Dictionary<string, string> members = new Dictionary<string, string>();

                    var orgs = (from tb in tbSubjects
                                where tb.TransmitStatusCode == (short)TransmitStatus.Deploy
                                orderby tb.SubjectName
                                select new { tb.SubjectCode, tb.SubjectName});

                    foreach (var member in orgs)
                        members.Add(member.SubjectCode, member.SubjectName);
                    return members;
                }
                catch (Exception err)
                {
                    string logCode = string.Empty;
                    proc_EventLog(err.Message, (short)EventType.Error, ref logCode);
                    return new Dictionary<string, string>();
                }
            }
        }

        public Dictionary<string, string> Candidates
        {
            get
            {
                try
                {
                    Dictionary<string, string> candidates = new Dictionary<string, string>();

                    var orgs = (from tb in tbSubjects
                                where tb.TransmitStatusCode == (short)TransmitStatus.Disconnected
                                    && tb.SubjectCode != (tbOptions.Select(o => o.SubjectCode).First().ToString())
                                orderby tb.SubjectName
                                select new { tb.SubjectCode, tb.SubjectName });

                    foreach (var candidate in orgs)
                        candidates.Add(candidate.SubjectCode, candidate.SubjectName);
                    return candidates;
                }
                catch (Exception err)
                {
                    string logCode = string.Empty;
                    proc_EventLog(err.Message, (short)EventType.Error, ref logCode);
                    return new Dictionary<string, string>();
                }
            }
        }

        public Task<bool> AddMember(string subjectCode)
        {
            return Task.Run(() =>
            {
                try
                {
                    var org = (from tb in tbSubjects where tb.SubjectCode == subjectCode select tb).First();
                    org.TransmitStatusCode = (short)TransmitStatus.Deploy;
                    SubmitChanges();
                    return true;
                }
                catch (Exception err)
                {
                    string logCode = string.Empty;
                    proc_EventLog(err.Message, (short)EventType.Error, ref logCode);
                    return false;
                }
            });
        }
        #endregion

        #region activities
        public Task<bool> AllocationTransmitted(string subjectCode, string allocationCode)
        {
            return Task.Run(() =>
            {
                try
                {
                    int result = proc_ObjectNetworkUpdated(subjectCode, allocationCode);
                    return result == 0;
                }
                catch (Exception err)
                {
                    string logCode = string.Empty;
                    proc_EventLog(err.Message, (short)EventType.Error, ref logCode);
                    return false;
                }
            });
        }

        public Task<bool> MirrorAllocation(string objectCode, string subjectCode, string allocationCode)
        {
            return Task.Run(() =>
            {
                try
                {
                    int result = proc_ObjectMirror(objectCode, subjectCode, allocationCode);
                    return result == 0;
                }
                catch (Exception err)
                {
                    string logCode = string.Empty;
                    proc_EventLog(err.Message, (short)EventType.Error, ref logCode);
                    return false;
                }
            });

        }
        #endregion

        #region projects
        public Task<bool> ProjectTransmitted(string projectCode)
        {
            return Task.Run(() =>
            {
                try
                {
                    int result = proc_ProjectNetworkUpdated(projectCode);
                    return result == 0;
                }
                catch (Exception err)
                {
                    string logCode = string.Empty;
                    proc_EventLog(err.Message, (short)EventType.Error, ref logCode);
                    return false;
                }
            });
        }


        public Task<bool> ProjectAllocation(tbAllocation allocation)
        {
            return Task.Run(() =>
            {

                try
                {
                    if (tbAllocations.Where(rec => rec.ContractAddress == allocation.ContractAddress).Select(s => s).SingleOrDefault() == null)
                        ProjectAllocationInsert(allocation);
                    else
                        ProjectAllocationUpdate(allocation);

                    SubmitChanges();

                    return true;
                }
                catch (Exception err)
                {
                    string logCode = string.Empty;
                    proc_EventLog(err.Message, (short)EventType.Error, ref logCode);
                    return false;
                }
            });
        }

        private void ProjectAllocationInsert(tbAllocation allocation)
        {
            tbAllocations.InsertOnSubmit(allocation);
        }

        private void ProjectAllocationUpdate(tbAllocation newAllocation)
        {
            tbAllocation existingAllocation = tbAllocations.Where(m => m.ContractAddress == newAllocation.ContractAddress).Single();

            existingAllocation.ProjectStatusCode = newAllocation.ProjectStatusCode;
            existingAllocation.UnitCharge = newAllocation.UnitCharge;
            existingAllocation.TaxRate = newAllocation.TaxRate;
            existingAllocation.ActionOn = newAllocation.ActionOn;
            existingAllocation.QuantityOrdered = newAllocation.QuantityOrdered;

        }
        #endregion

        #region cash codes
        public Task<bool> CashCodeTransmitted(string subjectCode, string chargeCode)
        {
            return Task.Run(() =>
            {
                try
                {
                    int result = proc_CashNetworkUpdated(subjectCode, chargeCode);
                    return result == 0;
                }
                catch (Exception err)
                {
                    string logCode = string.Empty;
                    proc_EventLog(err.Message, (short)EventType.Error, ref logCode);
                    return false;
                }
            });
        }

        public Task<bool> MirrorCashCode(string cashCode, string subjectCode, string chargeCode)
        {
            return Task.Run(() =>
            {
                try
                {
                    int result = proc_CashMirror(cashCode, subjectCode, chargeCode);
                    return result == 0;
                }
                catch (Exception err)
                {
                    string logCode = string.Empty;
                    proc_EventLog(err.Message, (short)EventType.Error, ref logCode);
                    return false;
                }
            });

        }
        #endregion

        #region invoices
        public InvoiceType GetInvoiceType(CashMode invoicePolarity, CashMode paymentPolarity)
        {
            switch (invoicePolarity)
            {
                case CashMode.Expense:
                    return paymentPolarity == CashMode.Expense ? InvoiceType.PurchaseInvoice : InvoiceType.DebitNote;
                case CashMode.Income:
                    return paymentPolarity == CashMode.Income ? InvoiceType.SalesInvoice : InvoiceType.CreditNote;
                default:
                    return InvoiceType.SalesInvoice;
            }
        }

        public Task<bool> InvoiceTransmitted(string invoiceNumber)
        {
            return Task.Run(() =>
            {
                try
                {
                    int result = proc_InvoiceNetworkUpdated(invoiceNumber);
                    return result == 0;
                }
                catch (Exception err)
                {
                    string logCode = string.Empty;
                    proc_EventLog(err.Message, (short)EventType.Error, ref logCode);
                    return false;
                }
            });
        }

        public Task<bool> InvoiceMirror (tbInvoiceMirror invoiceMirror)
        {
            return Task.Run(() =>
            {
                try
                {
                     if (tbInvoiceMirrors.Where(rec => rec.ContractAddress == invoiceMirror.ContractAddress).Select(s => s).SingleOrDefault() == null)
                        InvoiceMirrorInsert(invoiceMirror);
                    else
                        InvoiceMirrorUpdate(invoiceMirror);
                        
                    return true;
                }
                catch (Exception err)
                {
                    string logCode = string.Empty;
                    proc_EventLog(err.Message, (short)EventType.Error, ref logCode);
                    return false;
                }
            });
        }

        private void InvoiceMirrorInsert(tbInvoiceMirror invoiceMirror)
        {
            tbInvoiceMirrors.InsertOnSubmit(invoiceMirror);
        }

        private void InvoiceMirrorUpdate(tbInvoiceMirror newMirror)
        {
            tbInvoiceMirror existingMirror = tbInvoiceMirrors.Where(m => m.ContractAddress == newMirror.ContractAddress).Single();

            existingMirror.DueOn = newMirror.DueOn;
            existingMirror.InvoiceStatusCode = newMirror.InvoiceStatusCode;
            existingMirror.PaidValue = newMirror.PaidValue;
            existingMirror.PaidTaxValue = newMirror.PaidTaxValue;
            existingMirror.PaymentAddress = newMirror.PaymentAddress;

            SubmitChanges();
        }

        public Task<bool> InvoiceMirrorProject (tbMirrorProject mirrorProject)
        {
            return Task.Run(() =>
            {
                try
                {
                    tbMirrorProjects.InsertOnSubmit(mirrorProject);
                    return true;
                }
                catch (Exception err)
                {
                    string logCode = string.Empty;
                    proc_EventLog(err.Message, (short)EventType.Error, ref logCode);
                    return false;
                }
            });
        }

        public Task<bool> InvoiceMirrorItem(tbMirrorItem mirrorItem)
        {
            return Task.Run(() =>
            {
                try
                {
                    tbMirrorItems.InsertOnSubmit(mirrorItem);
                    return true;
                }
                catch (Exception err)
                {
                    string logCode = string.Empty;
                    proc_EventLog(err.Message, (short)EventType.Error, ref logCode);
                    return false;
                }
            });
        }


        public Task<bool> InvoiceMirrorReference(string contractAddress, string invoiceNumber)
        {
            return Task.Run(() =>
            {
                try
                {
                    if (!tbMirrorReferences.Where(r => r.ContractAddress == contractAddress && r.InvoiceNumber == invoiceNumber).Any())
                    {
                        tbMirrorReference mirrorReference = new tbMirrorReference
                        {
                            ContractAddress = contractAddress,
                            InvoiceNumber = invoiceNumber
                        };

                        tbMirrorReferences.InsertOnSubmit(mirrorReference);
                    }

                    return true;

                }
                catch (Exception err)
                {
                    string logCode = string.Empty;
                    proc_EventLog(err.Message, (short)EventType.Error, ref logCode);
                    return false;
                }
            });
        }


        #endregion
    }
}

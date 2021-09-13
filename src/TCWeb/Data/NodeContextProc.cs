#define DEBUG

using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

using TradeControl.Web.Areas.Identity.Data;
using TradeControl.Web.Models;

namespace TradeControl.Web.Data
{
    public partial class NodeContext : IdentityDbContext<TradeControlWebUser>
    {
        #region Procedure Datasets
        public virtual DbSet<Cash_proc_CodeDefaults> Cash_CodeDefaults { get; set; }
        public virtual DbSet<Activity_proc_WorkFlow> Activity_WorkFlow { get; set; }
        #endregion

        #region Cash Accounts
        public async Task<string> CurrentAccount()
        {
            try
            {
                var _cashAccountCode = new SqlParameter()
                {
                    ParameterName = "@CashAccountCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Output,
                    Size = 10
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Cash.proc_CurrentAccount";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_cashAccountCode);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (string)_cashAccountCode.Value;
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return string.Empty;
            }
        }

        public async Task<string> ReserveAccount()
        {
                try
                {
                    var _cashAccountCode = new SqlParameter()
                    {
                        ParameterName = "@CashAccountCode",
                        SqlDbType = System.Data.SqlDbType.VarChar,
                        Direction = System.Data.ParameterDirection.Output,
                        Size = 10
                    };

                    using (SqlConnection _connection = new (Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Cash.proc_ReserveAccount";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_cashAccountCode);

                            await _command.ExecuteNonQueryAsync();
                        }
                        _connection.Close();
                    }

                    return (string)_cashAccountCode.Value;
                }
                catch (Exception e)
                {
                    await ErrorLog(e);
                    return string.Empty;
                }
        }

        public async Task<NodeEnum.CoinType> CoinType()
        {
            try
            {
                var _coinTypeCode = new SqlParameter()
                {
                    ParameterName = "@CoinTypeCode",
                    SqlDbType = System.Data.SqlDbType.SmallInt,
                    Direction = System.Data.ParameterDirection.Output
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Cash.proc_CoinType";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_coinTypeCode);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (NodeEnum.CoinType)_coinTypeCode.Value;
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return NodeEnum.CoinType.Fiat;
            }
        }

        public Task<bool> PostAsset(string paymentCode)
        {
            return Task.Run(() =>
            {
                try
                {
                    var entry = Cash_tbPayments.Where(t => t.PaymentCode == paymentCode).FirstOrDefault();

                    if (entry == null)
                        return false;
                    else
                    {
                        entry.PaymentStatusCode = (short)NodeEnum.PaymentStatus.Posted;
                        return true;
                    }
                                     
                }
                catch (Exception e)
                {
                    _ = ErrorLog(e);
                    return false;
                }
            });
        }
        public async Task<string> NextPaymentCode()
        {
            try
            {
                var _paymentCode = new SqlParameter()
                {
                    ParameterName = "@PaymentCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Output,
                    Size = 20
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Cash.proc_NextPaymentCode";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_paymentCode);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (string)_paymentCode.Value;
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return string.Empty;
            }
        }


        public async Task<string> AddPayment(string cashAccountCode, string accountCode, string cashCode, DateTime paidOn, decimal toPay)
        {
            try
            {
                var _cashAccountCode = new SqlParameter()
                {
                    ParameterName = "@CashAccountCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 10,
                    Value = cashAccountCode
                };

                var _accountCode = new SqlParameter()
                {
                    ParameterName = "@AccountCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 10,
                    Value = accountCode
                };

                var _cashCode = new SqlParameter()
                {
                    ParameterName = "@CashCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 50,
                    Value = cashCode
                };

                var _paidOn = new SqlParameter()
                {
                    ParameterName = "@PaidOn",
                    SqlDbType = System.Data.SqlDbType.DateTime,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = paidOn.Date
                };

                var _toPay = new SqlParameter()
                {
                    ParameterName = "@ToPay",
                    SqlDbType = System.Data.SqlDbType.Decimal,
                    Size = 18,
                    Precision = 5,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = toPay
                };

                var _paymentCode = new SqlParameter()
                {
                    ParameterName = "@PaymentCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Output,
                    Size = 20
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Cash.proc_PaymentAdd";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_accountCode);
                        _command.Parameters.Add(_cashAccountCode);
                        _command.Parameters.Add(_cashCode);
                        _command.Parameters.Add(_paidOn);
                        _command.Parameters.Add(_toPay);

                        _command.Parameters.Add(_paymentCode);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (string)_paymentCode.Value;



            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return string.Empty;
            }
        }

        #endregion

        #region Cash Codes
        public async Task<decimal> VatBalance()
        {
            try
            {
                var _balanceParam = new SqlParameter()
                {
                    ParameterName = "@Balance",
                    SqlDbType = System.Data.SqlDbType.Decimal,
                    Direction = System.Data.ParameterDirection.Output,
                    Size = 18,
                    Precision = 5
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Cash.proc_VatBalance";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_balanceParam);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (decimal)_balanceParam.Value;
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return 0;
            }
        }
        #endregion

        #region Profiles
        public async Task<string> CompanyName()
        {
            try
            {
                var _accountName = new SqlParameter()
                {
                    ParameterName = "@AccountName",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Output,
                    Size = 255
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "App.proc_CompanyName";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_accountName);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return _accountName.Value != DBNull.Value ? (string)_accountName.Value : string.Empty;
                
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return string.Empty;
            }
            
        }

        public async Task<string> GetUserId(string aspnetId)
        {
            try
            {
                var _aspnetId = new SqlParameter()
                {
                    ParameterName = "@Id",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 450,
                    Value = aspnetId

                };

                var _userId = new SqlParameter()
                {
                    ParameterName = "@UserId",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Output,
                    Size = 10
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "AspNetGetUserId";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_aspnetId);
                        _command.Parameters.Add(_userId);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (string)_userId.Value;

            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return string.Empty;
            }
        }

        public async Task<string> GetUserName(string aspnetId)
        {
            try
            {
                var _aspnetId = new SqlParameter()
                {
                    ParameterName = "@Id",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 450,
                    Value = aspnetId
                };

                var _userName = new SqlParameter()
                {
                    ParameterName = "@UserName",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Output,
                    Size = 50
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "dbo.AspNetGetUserName";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_aspnetId);
                        _command.Parameters.Add(_userName);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (string)_userName.Value;

            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return string.Empty;
            }
        }

        public async Task<string> GetAspNetId(string userId)
        {
            try
            {
                var _userId = new SqlParameter()
                {
                    ParameterName = "@UserId",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 10,
                    Value = userId
                };

                var _aspnetId = new SqlParameter()
                {
                    ParameterName = "@Id",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Output,
                    Size = 450
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "dbo.AspNetGetId";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_userId);
                        _command.Parameters.Add(_aspnetId);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (string)_aspnetId.Value;

            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return string.Empty;
            }
        }
        #endregion

        #region Orgs
        public async Task<string> NextAddressCode(string accountCode)
        {
            try
            {
                var _accountCode = new SqlParameter()
                {
                    ParameterName = "@AccountCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 10,
                    Value = accountCode
                };

                var _addressCode = new SqlParameter()
                {
                    ParameterName = "@AddressCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Output,
                    Size = 15
                };
                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Org.proc_NextAddressCode";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_accountCode);
                        _command.Parameters.Add(_addressCode);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (string)_addressCode.Value;
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return string.Empty;
            }
        }

        public async Task<string> OrgAccountCodeDefault(string accountName)
        {
            try
            {
                var _accountName = new SqlParameter()
                {
                    ParameterName = "@AccountName",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 255,
                    Value = accountName
                };

                var _accountCode = new SqlParameter()
                {
                    ParameterName = "@AccountCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Output,
                    Size = 10
                };
                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Org.proc_DefaultAccountCode";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_accountName);
                        _command.Parameters.Add(_accountCode);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (string)_accountCode.Value;
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return string.Empty;
            }
        }

        public async Task<string> OrgTaxCodeDefault(string accountCode)
        {
            try
            {
                var _accountCode = new SqlParameter()
                {
                    ParameterName = "@AccountCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 10,
                    Value = accountCode
                };

                var _taxCode = new SqlParameter()
                {
                    ParameterName = "@TaxCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Output,
                    Size = 10
                };
                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Org.proc_DefaultTaxCode";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_accountCode);
                        _command.Parameters.Add(_taxCode);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (string)_taxCode.Value;
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return string.Empty;
            }
        }

        public async Task<string> OrgEmailAddressDefault(string accountCode)
        {
            try
            {
                var _accountCode = new SqlParameter()
                {
                    ParameterName = "@AccountCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 10,
                    Value = accountCode
                };

                var _emailAddress = new SqlParameter()
                {
                    ParameterName = "@EmailAddress",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Output,
                    Size = 255
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Org.proc_DefaultEmailAddress";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_accountCode);
                        _command.Parameters.Add(_emailAddress);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (string)_emailAddress.Value;
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return string.Empty;
            }
        }

        public async Task<decimal> BalanceOutstanding(string accountCode)
        {
            try
            {
                var _accountCode = new SqlParameter()
                {
                    ParameterName = "@AccountCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 10,
                    Value = accountCode
                };

                var _balance = new SqlParameter()
                {
                    ParameterName = "@Balance",
                    SqlDbType = System.Data.SqlDbType.Decimal,
                    Size = 18,
                    Precision = 5,
                    Direction = System.Data.ParameterDirection.Output
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Org.proc_BalanceOutstanding";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_accountCode);
                        _command.Parameters.Add(_balance);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (decimal)_balance.Value;
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return 0;
            }
        }

        public async Task<decimal> BalanceToPay(string accountCode)
        {
            try
            {
                var _accountCode = new SqlParameter()
                {
                    ParameterName = "@AccountCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 10,
                    Value = accountCode
                };

                var _balance = new SqlParameter()
                {
                    ParameterName = "@Balance",
                    SqlDbType = System.Data.SqlDbType.Decimal,
                    Size = 18,
                    Precision = 5,
                    Direction = System.Data.ParameterDirection.Output
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Org.proc_BalanceToPay";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_accountCode);
                        _command.Parameters.Add(_balance);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (decimal)_balance.Value;
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return 0;
            }
        }

        #endregion

        #region Activities
        public async Task<string> ParentActivity(string activityCode)
        {
            try
            {
                var _activityCode = new SqlParameter()
                {
                    ParameterName = "@ActivityCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 50,
                    Value = activityCode
                };

                var _parentCode = new SqlParameter()
                {
                    ParameterName = "@ParentCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Output,
                    Size = 50
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Activity.proc_Parent";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_activityCode);
                        _command.Parameters.Add(_parentCode);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (string)_parentCode.Value;
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return string.Empty;
            }
        }

        public async Task<short> GetActivityStepNumber(string activityCode)
        {
            try
            {
                var _activityCode = new SqlParameter()
                {
                    ParameterName = "@ActivityCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 50,
                    Value = activityCode
                };

                var _stepNumber = new SqlParameter()
                {
                    ParameterName = "@StepNumber",
                    SqlDbType = System.Data.SqlDbType.SmallInt,
                    Direction = System.Data.ParameterDirection.Output
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Activity.proc_NextStepNumber";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_activityCode);
                        _command.Parameters.Add(_stepNumber);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (short)_stepNumber.Value;

            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return (short)10;
            }
        }

        public async Task<short> GetActivityAtttributeOrder(string activityCode)
        {
            try
            {
                var _activityCode = new SqlParameter()
                {
                    ParameterName = "@ActivityCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 50,
                    Value = activityCode
                };

                var _printOrder = new SqlParameter()
                {
                    ParameterName = "@PrintOrder",
                    SqlDbType = System.Data.SqlDbType.SmallInt,
                    Direction = System.Data.ParameterDirection.Output
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Activity.proc_NextAttributeOrder";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_activityCode);
                        _command.Parameters.Add(_printOrder);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (short)_printOrder.Value;

            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return (short)10;
            }
        }

        public async Task<short> GetActivityOperationNumber(string activityCode)
        {
            try
            {
                var _activityCode = new SqlParameter()
                {
                    ParameterName = "@ActivityCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 50,
                    Value = activityCode
                };

                var _operationNumber = new SqlParameter()
                {
                    ParameterName = "@OperationNumber",
                    SqlDbType = System.Data.SqlDbType.SmallInt,
                    Direction = System.Data.ParameterDirection.Output
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Activity.proc_NextOperationNumber";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_activityCode);
                        _command.Parameters.Add(_operationNumber);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (short)_operationNumber.Value;

            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return (short)10;
            }
        }
        #endregion

        #region Invoice
        public async Task<string> InvoiceRaise(string taskCode, NodeEnum.InvoiceType invoiceType, DateTime invoicedOn)
        {
            try
            {
                var _taskCode = new SqlParameter()
                {
                    ParameterName = "@TaskCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 20,
                    Value = taskCode
                };

                var _invoiceType = new SqlParameter()
                {
                    ParameterName = "@InvoiceTypeCode",
                    SqlDbType = System.Data.SqlDbType.SmallInt,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = (short)invoiceType
                };

                var _invoicedOn = new SqlParameter()
                {
                    ParameterName = "@InvoicedOn",
                    SqlDbType = System.Data.SqlDbType.SmallInt,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = invoicedOn.Date
                };

                var _invoiceNumber = new SqlParameter()
                {
                    ParameterName = "@InvoiceNumber",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Output,
                    Size = 20
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Invoice.proc_Raise";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_taskCode);
                        _command.Parameters.Add(_invoiceType);
                        _command.Parameters.Add(_invoicedOn);
                        _command.Parameters.Add(_invoiceNumber);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (string)_invoiceNumber.Value;
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return string.Empty;
            }
        }

        public async Task<string> InvoiceRaiseBlank(string accountCode, NodeEnum.InvoiceType invoiceType)
        {
            try
            {
                var _accountCode = new SqlParameter()
                {
                    ParameterName = "@AccountCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 10,
                    Value = accountCode
                };

                var _invoiceType = new SqlParameter()
                {
                    ParameterName = "@InvoiceTypeCode",
                    SqlDbType = System.Data.SqlDbType.SmallInt,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = (short)invoiceType
                };

                var _invoiceNumber = new SqlParameter()
                {
                    ParameterName = "@InvoiceNumber",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Output,
                    Size = 20
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Invoice.proc_RaiseBlank";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_accountCode);
                        _command.Parameters.Add(_invoiceType);
                        _command.Parameters.Add(_invoiceNumber);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (string)_invoiceNumber.Value;
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return string.Empty;
            }
        }

        public async Task<string> InvoiceCredit(string invoiceNumber)
        {
            try
            {
                var _invoiceNumber = new SqlParameter()
                {
                    ParameterName = "@InvoiceNumber",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.InputOutput,
                    Size = 20,
                    Value = invoiceNumber
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Invoice.proc_Credit";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_invoiceNumber);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (string)_invoiceNumber.Value;
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return string.Empty;
            }
        }

        public async Task<string> InvoicePay(string invoiceNumber, DateTime paidOn, bool postPayment)
        {
            try
            {
                var _invoiceNumber = new SqlParameter()
                {
                    ParameterName = "@InvoiceNumber",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 20,
                    Value = invoiceNumber
                };

                var _paidOn = new SqlParameter()
                {
                    ParameterName = "@PaidOn",
                    SqlDbType = System.Data.SqlDbType.SmallInt,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = paidOn.Date
                };

                var _postPayment = new SqlParameter()
                {
                    ParameterName = "@Post",
                    SqlDbType = System.Data.SqlDbType.Bit,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = postPayment
                };

                var _paymentCode = new SqlParameter()
                {
                    ParameterName = "@PaymentCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Output,
                    Size = 20
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Invoice.proc_Pay";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_invoiceNumber);
                        _command.Parameters.Add(_paidOn);
                        _command.Parameters.Add(_postPayment);
                        _command.Parameters.Add(_paymentCode);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (string)_paymentCode.Value;
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return string.Empty;
            }
        }

        public async Task<NodeEnum.DocType> InvoiceDefaultDocType(string invoiceNumber)
        {
            try
            {
                var _invoiceNumber = new SqlParameter()
                {
                    ParameterName = "@InvoiceNumber",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 20,
                    Value = invoiceNumber
                };

                var _docType = new SqlParameter()
                {
                    ParameterName = "@DocTypeCode",
                    SqlDbType = System.Data.SqlDbType.SmallInt,
                    Direction = System.Data.ParameterDirection.Output
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Invoice.proc_Pay";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_invoiceNumber);
                        _command.Parameters.Add(_docType);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (NodeEnum.DocType)_docType.Value;
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return NodeEnum.DocType.SalesInvoice;
            }
        }

        public async Task<DateTime> InvoiceDefaultPaymentOn(string accountCode, DateTime actionOn)
        {
            try
            {
                var _accountCode = new SqlParameter()
                {
                    ParameterName = "@AccountCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 10,
                    Value = accountCode
                };

                var _actionOn = new SqlParameter()
                {
                    ParameterName = "@ActionOn",
                    SqlDbType = System.Data.SqlDbType.DateTime,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = actionOn.Date
                };

                var _paymentOn = new SqlParameter()
                {
                    ParameterName = "@PaymentOn",
                    SqlDbType = System.Data.SqlDbType.DateTime,
                    Direction = System.Data.ParameterDirection.Output
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Invoice.proc_DefaultPaymentOn";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_accountCode);
                        _command.Parameters.Add(_actionOn);
                        _command.Parameters.Add(_paymentOn);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (DateTime)_paymentOn.Value;
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return actionOn;
            }
        }

        public async Task<string> InvoiceMirror(string contractAddress)
        {
            try
            {
                var _contractAddress = new SqlParameter()
                {
                    ParameterName = "@ContractAddress",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 42,
                    Value = contractAddress
                };


                var _invoiceNumber = new SqlParameter()
                {
                    ParameterName = "@InvoiceNumber",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Output,
                    Size = 20
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Invoice.proc_Mirror";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_contractAddress);
                        _command.Parameters.Add(_invoiceNumber);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (string)_invoiceNumber.Value;
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return string.Empty;
            }
        }
        #endregion

        #region Periods
        public async Task<short> GetYearFromPeriod(DateTime startOn)
        {
            try
            {
                var _startOn = new SqlParameter()
                {
                    ParameterName = "@StartOn",
                    SqlDbType = System.Data.SqlDbType.DateTime,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = startOn
                };


                var _yearNumber = new SqlParameter()
                {
                    ParameterName = "@YearNumber",
                    SqlDbType = System.Data.SqlDbType.SmallInt,
                    Direction = System.Data.ParameterDirection.Output
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "App.proc_PeriodGetYear";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_startOn);
                        _command.Parameters.Add(_yearNumber);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (short)_yearNumber.Value;
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return (short)DateTime.Today.Year;
            }
        }

        public async Task<DateTime> AdjustToCalendar(DateTime sourceDate, short offsetDays)
        {
            try
            {
                var _sourceDate = new SqlParameter()
                {
                    ParameterName = "@SourceDate",
                    SqlDbType = System.Data.SqlDbType.DateTime,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = sourceDate
                };

                var _offsetDays = new SqlParameter()
                {
                    ParameterName = "@OffsetDays",
                    SqlDbType = System.Data.SqlDbType.SmallInt,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = offsetDays
                };

                var _outputDate = new SqlParameter()
                {
                    ParameterName = "@OutputDate",
                    SqlDbType = System.Data.SqlDbType.DateTime,
                    Direction = System.Data.ParameterDirection.Output
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "App.proc_PeriodGetYear";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_sourceDate);
                        _command.Parameters.Add(_offsetDays);
                        _command.Parameters.Add(_outputDate);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (DateTime)_outputDate.Value;

            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return sourceDate;
            }
        }

        public async Task<bool> AdjustTax(DateTime startOn, NodeEnum.TaxType taxType, double taxAdjustment)
        {
            try
            {
                var _startOn = new SqlParameter()
                {
                    ParameterName = "@StartOn",
                    SqlDbType = System.Data.SqlDbType.DateTime,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = startOn
                };

                var _taxTypeCode = new SqlParameter()
                {
                    ParameterName = "@TaxTypeCode",
                    SqlDbType = System.Data.SqlDbType.SmallInt,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = (short)taxType
                };

                var _taxAdjustment = new SqlParameter()
                {
                    ParameterName = "@TaxAdjustment",
                    SqlDbType = System.Data.SqlDbType.Decimal,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = taxAdjustment
                };

                using (SqlConnection _connection = new(Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Cash.proc_TaxAdjustment";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_startOn);
                        _command.Parameters.Add(_taxTypeCode);
                        _command.Parameters.Add(_taxAdjustment);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return true;

            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return false;
            }
        }

        public async Task<bool> TaxRate(DateTime startOn, DateTime endOn, float taxRate)
        {
            try
            {
                var _startOn = new SqlParameter()
                {
                    ParameterName = "@StartOn",
                    SqlDbType = System.Data.SqlDbType.DateTime,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = startOn
                };

                var _endOn = new SqlParameter()
                {
                    ParameterName = "@EndOn",
                    SqlDbType = System.Data.SqlDbType.DateTime,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = endOn
                };

                var _taxRate = new SqlParameter()
                {
                    ParameterName = "@CorporationTaxRate",
                    SqlDbType = System.Data.SqlDbType.Real,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = taxRate
                };

                using (SqlConnection _connection = new(Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "App.proc_TaxRates";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_startOn);
                        _command.Parameters.Add(_endOn);
                        _command.Parameters.Add(_taxRate);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return true;

            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return false;
            }
        }

        #endregion

        #region Tasks
        public async Task<bool> IsTaskProject(string taskCode)
        {
            try
            {
                var _taskCode = new SqlParameter()
                {
                    ParameterName = "@TaskCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 20,
                    Value = taskCode
                };


                var _isProject = new SqlParameter()
                {
                    ParameterName = "@IsProject",
                    SqlDbType = System.Data.SqlDbType.Bit,
                    Direction = System.Data.ParameterDirection.Output
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Task.proc_IsProject";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_taskCode);
                        _command.Parameters.Add(_isProject);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (bool)_isProject.Value;

            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return false;
            }

        }

        public async Task<bool> IsTaskFullyInvoiced(string taskCode)
        {
            try
            {
                var _taskCode = new SqlParameter()
                {
                    ParameterName = "@TaskCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 20,
                    Value = taskCode
                };


                var _isFullyInvoiced = new SqlParameter()
                {
                    ParameterName = "@FullyInvoiced",
                    SqlDbType = System.Data.SqlDbType.Bit,
                    Direction = System.Data.ParameterDirection.Output
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Task.proc_FullyInvoiced";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_taskCode);
                        _command.Parameters.Add(_isFullyInvoiced);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (bool)_isFullyInvoiced.Value;

            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return false;
            }
        }

        public async Task<string> ParentTaskCode(string taskCode)
        {
            try
            {
                var _taskCode = new SqlParameter()
                {
                    ParameterName = "@TaskCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 20,
                    Value = taskCode
                };


                var _parentTaskCode = new SqlParameter()
                {
                    ParameterName = "@ParentTaskCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Size = 20,
                    Direction = System.Data.ParameterDirection.Output
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Task.proc_Parent";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_taskCode);
                        _command.Parameters.Add(_parentTaskCode);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (string)_parentTaskCode.Value;

            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return string.Empty;
            }
        }

        public async Task<string> ProjectTaskCode(string taskCode)
        {
            try
            {
                var _taskCode = new SqlParameter()
                {
                    ParameterName = "@TaskCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 20,
                    Value = taskCode
                };


                var _parentTaskCode = new SqlParameter()
                {
                    ParameterName = "@ParentTaskCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Size = 20,
                    Direction = System.Data.ParameterDirection.Output
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Task.proc_Project";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_taskCode);
                        _command.Parameters.Add(_parentTaskCode);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (string)_parentTaskCode.Value;

            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return string.Empty;
            }
        }

        public async Task<string> GetNextTaskCode(string activityCode)
        {
            try
            {
                var _activityCode = new SqlParameter()
                {
                    ParameterName = "@ActivityCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 50,
                    Value = activityCode
                };


                var _taskCode = new SqlParameter()
                {
                    ParameterName = "@TaskCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Size = 20,
                    Direction = System.Data.ParameterDirection.Output
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Task.proc_NextCode";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_activityCode);
                        _command.Parameters.Add(_taskCode);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (string)_taskCode.Value;

            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return string.Empty;
            }
        }

        public async Task<string> TaskCopy(string taskCode)
        {
            try
            {
                var _taskCode = new SqlParameter()
                {
                    ParameterName = "@FromTaskCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 20,
                    Value = taskCode
                };


                var _parentTaskCode = new SqlParameter()
                {
                    ParameterName = "@ParentTaskCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Size = 20,
                    Direction = System.Data.ParameterDirection.Output
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Task.proc_Copy";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_taskCode);
                        _command.Parameters.Add(_parentTaskCode);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (string)_parentTaskCode.Value;

            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return string.Empty;
            }
        }

        public async Task<short> GetTaskAtttributeOrder(string taskCode)
        {
            try
            {
                var _taskCode = new SqlParameter()
                {
                    ParameterName = "@TaskCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 20,
                    Value = taskCode
                };

                var _printOrder = new SqlParameter()
                {
                    ParameterName = "@PrintOrder",
                    SqlDbType = System.Data.SqlDbType.SmallInt,
                    Direction = System.Data.ParameterDirection.Output
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Task.proc_NextAttributeOrder";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_taskCode);
                        _command.Parameters.Add(_printOrder);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (short)_printOrder.Value;

            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return (short)10;
            }
        }

        public async Task<short> GetTaskOperationNumber(string taskCode)
        {
            try
            {
                var _taskCode = new SqlParameter()
                {
                    ParameterName = "@TaskCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 20,
                    Value = taskCode
                };

                var _operationNumber = new SqlParameter()
                {
                    ParameterName = "@OperationNumber",
                    SqlDbType = System.Data.SqlDbType.SmallInt,
                    Direction = System.Data.ParameterDirection.Output
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Task.proc_NextOperationNumber";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_taskCode);
                        _command.Parameters.Add(_operationNumber);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (short)_operationNumber.Value;

            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return (short)10;
            }
        }

        public async Task<decimal> GetTaskCost(string taskCode)
        {
            try
            {
                var _taskCode = new SqlParameter()
                {
                    ParameterName = "@TaskCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 20,
                    Value = taskCode
                };

                var _totalCost = new SqlParameter()
                {
                    ParameterName = "@TotalCost",
                    SqlDbType = System.Data.SqlDbType.Decimal,
                    Size = 18,
                    Precision = 5,
                    Direction = System.Data.ParameterDirection.Output
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Task.proc_Cost";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_taskCode);
                        _command.Parameters.Add(_totalCost);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (decimal)_totalCost.Value;

            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return (decimal)0;
            }
        }

        public async Task<string> TaskPay(string taskCode, bool postPayment)
        {
            try
            {
                var _invoiceNumber = new SqlParameter()
                {
                    ParameterName = "@TaskCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 20,
                    Value = taskCode
                };

                var _postPayment = new SqlParameter()
                {
                    ParameterName = "@Post",
                    SqlDbType = System.Data.SqlDbType.Bit,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = postPayment
                };

                var _paymentCode = new SqlParameter()
                {
                    ParameterName = "@PaymentCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Output,
                    Size = 20
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Task.proc_Pay";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_invoiceNumber);
                        _command.Parameters.Add(_postPayment);
                        _command.Parameters.Add(_paymentCode);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (string)_paymentCode.Value;
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return string.Empty;
            }
        }

        public async Task<string> TaskTaxCodeDefault(string accountCode, string cashCode)
        {
            try
            {
                var _accountCode = new SqlParameter()
                {
                    ParameterName = "@AccountCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 10,
                    Value = accountCode
                };

                var _cashCode = new SqlParameter()
                {
                    ParameterName = "@CashCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 10,
                    Value = cashCode
                };


                var _taxCode = new SqlParameter()
                {
                    ParameterName = "@TaxCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Output,
                    Size = 10
                };
                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Task.proc_DefaultTaxCode";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_accountCode);
                        _command.Parameters.Add(_cashCode);
                        _command.Parameters.Add(_taxCode);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (string)_taxCode.Value;
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return string.Empty;
            }
        }

        public async Task<NodeEnum.InvoiceType> TaskInvoiceTypeDefault(string taskCode)
        {
            try
            {
                var _taskCode = new SqlParameter()
                {
                    ParameterName = "@TaskCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 20,
                    Value = taskCode
                };

                var _invoiceType = new SqlParameter()
                {
                    ParameterName = "@InvoiceTypeCode",
                    SqlDbType = System.Data.SqlDbType.SmallInt,
                    Direction = System.Data.ParameterDirection.Output
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Task.proc_DefaultInvoiceType";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_taskCode);
                        _command.Parameters.Add(_invoiceType);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (NodeEnum.InvoiceType)_invoiceType.Value;
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return NodeEnum.InvoiceType.SalesInvoice;
            }
        }

        public async Task<NodeEnum.DocType> TaskDocTypeDefault(string taskCode)
        {
            try
            {
                var _taskCode = new SqlParameter()
                {
                    ParameterName = "@TaskCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 20,
                    Value = taskCode
                };

                var _docType = new SqlParameter()
                {
                    ParameterName = "@DocTypeCode",
                    SqlDbType = System.Data.SqlDbType.SmallInt,
                    Direction = System.Data.ParameterDirection.Output
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Task.proc_DefaultDocType";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_taskCode);
                        _command.Parameters.Add(_docType);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (NodeEnum.DocType)_docType.Value;
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return NodeEnum.DocType.SalesOrder;
            }
        }

        public async Task<DateTime> TaskPaymentOnDefault(string accountCode, DateTime actionOn)
        {
            try
            {
                var _accountCode = new SqlParameter()
                {
                    ParameterName = "@AccountCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 10,
                    Value = accountCode
                };

                var _actionOn = new SqlParameter()
                {
                    ParameterName = "@ActionOn",
                    SqlDbType = System.Data.SqlDbType.DateTime,
                    Direction = System.Data.ParameterDirection.Input,
                    Value = actionOn
                };

                var _paymentOn = new SqlParameter()
                {
                    ParameterName = "@PaymentOn",
                    SqlDbType = System.Data.SqlDbType.DateTime,
                    Direction = System.Data.ParameterDirection.Output
                };


                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Task.proc_DefaultPaymentOn";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_accountCode);
                        _command.Parameters.Add(_actionOn);
                        _command.Parameters.Add(_paymentOn);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (DateTime)_paymentOn.Value;
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return actionOn;
            }
        }

        public async Task<string> TaskEmailAddress(string taskCode)
        {
            try
            {
                var _taskCode = new SqlParameter()
                {
                    ParameterName = "@TaskCode",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Direction = System.Data.ParameterDirection.Input,
                    Size = 20,
                    Value = taskCode
                };

                var _emailAddress = new SqlParameter()
                {
                    ParameterName = "@EmailAddress",
                    SqlDbType = System.Data.SqlDbType.VarChar,
                    Size = 255,
                    Direction = System.Data.ParameterDirection.Output
                };

                using (SqlConnection _connection = new (Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "Task.proc_EmailAddress";
                        _command.CommandType = CommandType.StoredProcedure;
                        _command.Parameters.Add(_taskCode);
                        _command.Parameters.Add(_emailAddress);

                        await _command.ExecuteNonQueryAsync();
                    }
                    _connection.Close();
                }

                return (string)_emailAddress.Value;
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                return string.Empty;
            }
        }
        #endregion

        #region logs
        public async Task<string> ErrorLog(Exception e)
        {
            static string ParseString(string message)
            {
                char[] source = message.ToCharArray();
                string target = string.Empty;

                for (int i = 0; i < source.Length; i++)
                {
                    if (source[i] == ',')
                        target += ';';
                    else if (source[i] > 31 && source[i] < 127)
                        target += source[i];
                    else
                        target += '\x0020';
                }

                return target;
            }

            try
            {
                StringBuilder eventMessage = new();
                
                eventMessage.AppendLine($"Message: {e.Message}");
#if DEBUG
                eventMessage.AppendLine($". Code: {e.HResult}");
                eventMessage.AppendLine($". Source: {e.Source}");
                eventMessage.AppendLine($". Stack Trace: {e?.StackTrace}");
                
                if (e.InnerException != null)
                    eventMessage.AppendLine($". Inner Exception: {ParseString(e.InnerException.Message)}");
#endif
                return await EventLog(NodeEnum.EventType.IsError, eventMessage.ToString());
            }
            catch //(Exception err)
            {
                return string.Empty;
            }
        }

        public async Task<string> EventLog(NodeEnum.EventType eventType, string eventMessage)
        {
            var _eventMessage = new SqlParameter()
            {
                ParameterName = "@EventMessage",
                SqlDbType = System.Data.SqlDbType.VarChar,
                Direction = System.Data.ParameterDirection.Input,
                Size = eventMessage.Length,
                Value = eventMessage
            };

            var _eventType = new SqlParameter()
            {
                ParameterName = "@EventTypeCode",
                SqlDbType = System.Data.SqlDbType.SmallInt,
                Direction = System.Data.ParameterDirection.Input,
                Value = eventType
            };

            var _logCode = new SqlParameter()
            {
                ParameterName = "@LogCode",
                SqlDbType = System.Data.SqlDbType.VarChar,
                Direction = System.Data.ParameterDirection.Output,
                Size = 20,
                Value = null
            };

            using (SqlConnection _connection = new (Database.GetConnectionString()))
            {
                _connection.Open();
                using (SqlCommand _command = _connection.CreateCommand())
                {
                    _command.CommandText = "App.proc_EventLog";
                    _command.CommandType = CommandType.StoredProcedure;

                    _command.Parameters.Add(_eventMessage);
                    _command.Parameters.Add(_eventType);
                    _command.Parameters.Add(_logCode);
                    await _command.ExecuteNonQueryAsync();
                }
                _connection.Close();
            }

            return _logCode.Value == null ? string.Empty : (string)_logCode.Value;
        }

        #endregion

        #region config
        public async Task ConfigureNode(string accountCode,
                                    string businessName,
                                    string fullName,
                                    string businessAddress,
                                    string businessEmailAddress,
                                    string userEmailAddress,
                                    string phoneNumber,
                                    string companyNumber,
                                    string vatNumber,
                                    string calendarCode,
                                    string uocName)
        {
            try
            {
                string unitOfCharge = await App_tbUocs.Where(u => u.UocName == uocName).Select(u => u.UnitOfCharge).SingleAsync();

                using SqlConnection _connection = new(Database.GetConnectionString());
                _connection.Open();

                using (SqlCommand command = _connection.CreateCommand())
                {
                    command.CommandText = "App.proc_NodeInitialisation";
                    command.CommandType = CommandType.StoredProcedure;

                    SqlParameter p1 = command.CreateParameter();
                    p1.DbType = DbType.String;
                    p1.ParameterName = "@AccountCode";
                    p1.Value = accountCode;
                    command.Parameters.Add(p1);

                    SqlParameter p2 = command.CreateParameter();
                    p2.DbType = DbType.String;
                    p2.ParameterName = "@BusinessName";
                    p2.Value = businessName;
                    command.Parameters.Add(p2);

                    SqlParameter p3 = command.CreateParameter();
                    p3.DbType = DbType.String;
                    p3.ParameterName = "@FullName";
                    p3.Value = fullName;
                    command.Parameters.Add(p3);

                    SqlParameter p4 = command.CreateParameter();
                    p4.DbType = DbType.String;
                    p4.ParameterName = "@BusinessAddress";
                    p4.Value = businessAddress;
                    command.Parameters.Add(p4);

                    SqlParameter p5 = command.CreateParameter();
                    p5.DbType = DbType.String;
                    p5.ParameterName = "@BusinessEmailAddress";
                    p5.Value = businessEmailAddress;
                    command.Parameters.Add(p5);

                    SqlParameter p6 = command.CreateParameter();
                    p6.DbType = DbType.String;
                    p6.ParameterName = "@UserEmailAddress";
                    p6.Value = userEmailAddress;
                    command.Parameters.Add(p6);

                    SqlParameter p7 = command.CreateParameter();
                    p7.DbType = DbType.String;
                    p7.ParameterName = "@PhoneNumber";
                    p7.Value = phoneNumber;
                    command.Parameters.Add(p7);

                    SqlParameter p8 = command.CreateParameter();
                    p8.DbType = DbType.String;
                    p8.ParameterName = "@CompanyNumber";
                    p8.Value = companyNumber;
                    command.Parameters.Add(p8);

                    SqlParameter p9 = command.CreateParameter();
                    p9.DbType = DbType.String;
                    p9.ParameterName = "@VatNumber";
                    p9.Value = vatNumber;
                    command.Parameters.Add(p9);

                    SqlParameter p10 = command.CreateParameter();
                    p10.DbType = DbType.String;
                    p10.ParameterName = "@CalendarCode";
                    p10.Value = calendarCode;
                    command.Parameters.Add(p10);

                    SqlParameter p11 = command.CreateParameter();
                    p11.DbType = DbType.String;
                    p11.ParameterName = "@UnitOfCharge";
                    p11.Value = unitOfCharge;
                    command.Parameters.Add(p11);

                    await command.ExecuteNonQueryAsync();

                }
                _connection.Close();
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                throw;
            }
        }

        public async Task InstallBasicSetup(string templateName,
                                        short financialMonth,                                        
                                        string govAccountName,
                                        string bankName,
                                        string bankAddress,
                                        string dummyAccount,
                                        string currentAccount,
                                        string ca_SortCode,
                                        string ca_AccountNumber,
                                        string reserveAccount,
                                        string ra_SortCode,
                                        string ra_AccountNumber)
        {
            try
            {
                NodeEnum.CoinType coinType = NodeEnum.CoinType.Fiat;

                using SqlConnection _connection = new(Database.GetConnectionString());
                _connection.Open();
                using (SqlCommand command = _connection.CreateCommand())
                {
                    command.CommandText = "App.proc_BasicSetup";
                    command.CommandType = CommandType.StoredProcedure;

                    SqlParameter pk = command.CreateParameter();
                    pk.DbType = DbType.String;
                    pk.ParameterName = "@TemplateName";
                    pk.Value = templateName;
                    command.Parameters.Add(pk);

                    SqlParameter p0 = command.CreateParameter();
                    p0.DbType = DbType.Int16;
                    p0.ParameterName = "@FinancialMonth";
                    p0.Value = financialMonth;
                    command.Parameters.Add(p0);

                    SqlParameter p1 = command.CreateParameter();
                    p1.DbType = DbType.Int16;
                    p1.ParameterName = "@CoinTypeCode";
                    p1.Value = (short)coinType;
                    command.Parameters.Add(p1);

                    SqlParameter p2 = command.CreateParameter();
                    p2.DbType = DbType.String;
                    p2.ParameterName = "@GovAccountName";
                    p2.Value = govAccountName;
                    command.Parameters.Add(p2);

                    SqlParameter p3 = command.CreateParameter();
                    p3.DbType = DbType.String;
                    p3.ParameterName = "@BankName";
                    p3.Value = bankName;
                    command.Parameters.Add(p3);

                    SqlParameter p4 = command.CreateParameter();
                    p4.DbType = DbType.String;
                    p4.ParameterName = "@BankAddress";
                    p4.Value = bankAddress;
                    command.Parameters.Add(p4);

                    SqlParameter p5 = command.CreateParameter();
                    p5.DbType = DbType.String;
                    p5.ParameterName = "@DummyAccount";
                    p5.Value = dummyAccount;
                    command.Parameters.Add(p5);

                    SqlParameter p6 = command.CreateParameter();
                    p6.DbType = DbType.String;
                    p6.ParameterName = "@CurrentAccount";
                    p6.Value = currentAccount;
                    command.Parameters.Add(p6);

                    SqlParameter p7 = command.CreateParameter();
                    p7.DbType = DbType.String;
                    p7.ParameterName = "@CA_SortCode";
                    p7.Value = ca_SortCode;
                    command.Parameters.Add(p7);

                    SqlParameter p8 = command.CreateParameter();
                    p8.DbType = DbType.String;
                    p8.ParameterName = "@CA_AccountNumber";
                    p8.Value = ca_AccountNumber;
                    command.Parameters.Add(p8);

                    SqlParameter p9 = command.CreateParameter();
                    p9.DbType = DbType.String;
                    p9.ParameterName = "@ReserveAccount";
                    p9.Value = reserveAccount;
                    command.Parameters.Add(p9);

                    SqlParameter p10 = command.CreateParameter();
                    p10.DbType = DbType.String;
                    p10.ParameterName = "@RA_SortCode";
                    p10.Value = ra_SortCode;
                    command.Parameters.Add(p10);

                    SqlParameter p11 = command.CreateParameter();
                    p11.DbType = DbType.String;
                    p11.ParameterName = "@RA_AccountNumber";
                    p11.Value = ra_AccountNumber;
                    command.Parameters.Add(p11);

                    await command.ExecuteNonQueryAsync();
                }
                _connection.Close();
            }
            catch (Exception e)
            {
                await ErrorLog(e);
                throw;
            }
        }
        #endregion
    }
}

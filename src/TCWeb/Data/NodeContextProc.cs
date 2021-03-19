using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using TradeControl.Web.Models;

namespace TradeControl.Web.Data
{
    public partial class NodeContext : DbContext
    {
        #region Procedure Datasets
        public virtual DbSet<Cash_proc_CodeDefaults> Cash_CodeDefaults { get; set; }
        public virtual DbSet<Activity_proc_WorkFlow> Activity_WorkFlow { get; set; }
        #endregion

        #region Cash Accounts
        public Task<string> CurrentAccount
        {
            get
            {
                return Task.Run(() =>
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

                        using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                        {
                            _connection.Open();
                            using (SqlCommand _command = _connection.CreateCommand())
                            {
                                _command.CommandText = "Cash.proc_CurrentAccount";
                                _command.CommandType = CommandType.StoredProcedure;
                                _command.Parameters.Add(_cashAccountCode);

                                _command.ExecuteNonQuery();
                            }
                            _connection.Close();
                        }

                        return (string)_cashAccountCode.Value;
                    }
                    catch (Exception e)
                    {
                        ErrorLog(e);
                        return string.Empty;
                    }
                });
            }
        }

        public Task<string> ReserveAccount
        {
            get
            {
                return Task.Run(() =>
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

                        using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                        {
                            _connection.Open();
                            using (SqlCommand _command = _connection.CreateCommand())
                            {
                                _command.CommandText = "Cash.proc_ReserveAccount";
                                _command.CommandType = CommandType.StoredProcedure;
                                _command.Parameters.Add(_cashAccountCode);

                                _command.ExecuteNonQuery();
                            }
                            _connection.Close();
                        }

                        return (string)_cashAccountCode.Value;
                    }
                    catch (Exception e)
                    {
                        ErrorLog(e);
                        return string.Empty;
                    }
                });
            }
        }

        public Task<NodeEnum.CoinType> CoinType
        {
            get
            {
                return Task.Run(() =>
                {
                    try
                    {
                        var _coinTypeCode = new SqlParameter()
                        {
                            ParameterName = "@CoinTypeCode",
                            SqlDbType = System.Data.SqlDbType.SmallInt,
                            Direction = System.Data.ParameterDirection.Output
                        };

                        using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                        {
                            _connection.Open();
                            using (SqlCommand _command = _connection.CreateCommand())
                            {
                                _command.CommandText = "Cash.proc_CoinType";
                                _command.CommandType = CommandType.StoredProcedure;
                                _command.Parameters.Add(_coinTypeCode);

                                _command.ExecuteNonQuery();
                            }
                            _connection.Close();
                        }

                        return (NodeEnum.CoinType)_coinTypeCode.Value;
                    }
                    catch (Exception e)
                    {
                        ErrorLog(e);
                        return NodeEnum.CoinType.Fiat;
                    }
                });
            }
        }

        public Task<string> NextPaymentCode()
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Cash.proc_NextPaymentCode";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_paymentCode);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (string)_paymentCode.Value;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return string.Empty;
                }
            });
        }


        public Task<string> AddPayment(string cashAccountCode, string accountCode, string cashCode, DateTime paidOn, decimal toPay)
        {
            return Task.Run(() =>
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
                        Direction = System.Data.ParameterDirection.Input
                    };

                    var _paymentCode = new SqlParameter()
                    {
                        ParameterName = "@PaymentCode",
                        SqlDbType = System.Data.SqlDbType.VarChar,
                        Direction = System.Data.ParameterDirection.Output,
                        Size = 20
                    };

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
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

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (string)_paymentCode.Value;



                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return string.Empty;
                }
            });
        }

        #endregion

        #region Cash Codes
        public Task<decimal> VatBalance
        {
            get
            {
                return Task.Run(() =>
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

                        using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                        {
                            _connection.Open();
                            using (SqlCommand _command = _connection.CreateCommand())
                            {
                                _command.CommandText = "Cash.proc_VatBalance";
                                _command.CommandType = CommandType.StoredProcedure;
                                _command.Parameters.Add(_balanceParam);

                                _command.ExecuteNonQuery();
                            }
                            _connection.Close();
                        }

                        return (decimal)_balanceParam.Value;
                    }
                    catch (Exception e)
                    {
                        ErrorLog(e);
                        return 0;
                    }
                });
            }
        }
        #endregion

        #region Profiles
        public Task<string> CompanyName
        {
            get
            {
                return Task.Run(() =>
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

                        using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                        {
                            _connection.Open();
                            using (SqlCommand _command = _connection.CreateCommand())
                            {
                                _command.CommandText = "App.proc_CompanyName";
                                _command.CommandType = CommandType.StoredProcedure;
                                _command.Parameters.Add(_accountName);

                                _command.ExecuteNonQuery();
                            }
                            _connection.Close();
                        }

                        return (string)_accountName.Value;
                    }
                    catch (Exception e)
                    {
                        ErrorLog(e);
                        return string.Empty;
                    }
                });
            }
        }
        #endregion

        #region Orgs
        public Task<string> NextAddressCode(string accountCode)
        {
            return Task.Run(() =>
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
                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Org.proc_NextAddressCode";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_accountCode);
                            _command.Parameters.Add(_addressCode);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (string)_addressCode.Value;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return string.Empty;
                }
            });
        }

        public Task<string> OrgAccountCodeDefault(string accountName)
        {
            return Task.Run(() =>
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
                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Org.proc_DefaultAccountCode";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_accountName);
                            _command.Parameters.Add(_accountCode);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (string)_accountCode.Value;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return string.Empty;
                }
            });
        }

        public Task<string> OrgTaxCodeDefault(string accountCode)
        {
            return Task.Run(() =>
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
                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Org.proc_DefaultTaxCode";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_accountCode);
                            _command.Parameters.Add(_taxCode);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (string)_taxCode.Value;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return string.Empty;
                }
            });
        }

        public Task<string> OrgEmailAddressDefault(string accountCode)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Org.proc_DefaultEmailAddress";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_accountCode);
                            _command.Parameters.Add(_emailAddress);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (string)_emailAddress.Value;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return string.Empty;
                }
            });
        }

        public Task<decimal> BalanceOutstanding(string accountCode)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Org.proc_BalanceOutstanding";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_accountCode);
                            _command.Parameters.Add(_balance);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (decimal)_balance.Value;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return 0;
                }
            });
        }

        public Task<decimal> BalanceToPay(string accountCode)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Org.proc_BalanceToPay";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_accountCode);
                            _command.Parameters.Add(_balance);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (decimal)_balance.Value;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return 0;
                }
            });
        }

        #endregion

        #region Activities
        public Task<string> ParentActivity(string activityCode)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Activity.proc_Parent";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_activityCode);
                            _command.Parameters.Add(_parentCode);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (string)_parentCode.Value;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return string.Empty;
                }
            });
        }

        public Task<short> GetActivityStepNumber(string activityCode)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Activity.proc_NextStepNumber";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_activityCode);
                            _command.Parameters.Add(_stepNumber);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (short)_stepNumber.Value;

                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return (short)10;
                }
            });
        }

        public Task<short> GetActivityAtttributeOrder(string activityCode)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Activity.proc_NextAttributeOrder";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_activityCode);
                            _command.Parameters.Add(_printOrder);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (short)_printOrder.Value;

                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return (short)10;
                }
            });
        }

        public Task<short> GetActivityOperationNumber(string activityCode)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Activity.proc_NextOperationNumber";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_activityCode);
                            _command.Parameters.Add(_operationNumber);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (short)_operationNumber.Value;

                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return (short)10;
                }
            });
        }
        #endregion

        #region Invoice
        public Task<string> InvoiceRaise(string taskCode, NodeEnum.InvoiceType invoiceType, DateTime invoicedOn)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
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

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (string)_invoiceNumber.Value;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return string.Empty;
                }
            });
        }

        public Task<string> InvoiceRaiseBlank(string accountCode, NodeEnum.InvoiceType invoiceType)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Invoice.proc_RaiseBlank";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_accountCode);
                            _command.Parameters.Add(_invoiceType);
                            _command.Parameters.Add(_invoiceNumber);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (string)_invoiceNumber.Value;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return string.Empty;
                }
            });
        }

        public Task<string> InvoiceCredit(string invoiceNumber)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Invoice.proc_Credit";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_invoiceNumber);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (string)_invoiceNumber.Value;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return string.Empty;
                }
            });
        }

        public Task<string> InvoicePay(string invoiceNumber,DateTime paidOn, bool postPayment)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
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

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (string)_paymentCode.Value;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return string.Empty;
                }
            });
        }

        public Task<NodeEnum.DocType> InvoiceDefaultDocType(string invoiceNumber)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Invoice.proc_Pay";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_invoiceNumber);
                            _command.Parameters.Add(_docType);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (NodeEnum.DocType)_docType.Value;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return NodeEnum.DocType.SalesInvoice;
                }
            });
        }

        public Task<DateTime> InvoiceDefaultPaymentOn(string accountCode, DateTime actionOn)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Invoice.proc_DefaultPaymentOn";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_accountCode);
                            _command.Parameters.Add(_actionOn);
                            _command.Parameters.Add(_paymentOn);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (DateTime)_paymentOn.Value;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return actionOn;
                }
            });
        }

        public Task<string> InvoiceMirror(string contractAddress)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Invoice.proc_Mirror";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_contractAddress);
                            _command.Parameters.Add(_invoiceNumber);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (string)_invoiceNumber.Value;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return string.Empty;
                }
            });
        }
        #endregion

        #region Periods
        public Task<short> GetYearFromPeriod(DateTime startOn)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "App.proc_PeriodGetYear";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_startOn);
                            _command.Parameters.Add(_yearNumber);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (short)_yearNumber.Value;
                }               
                catch (Exception e)
                {
                    ErrorLog(e);
                    return (short)DateTime.Today.Year;
                }
            });
        }

        public Task<DateTime> AdjustToCalendar(DateTime sourceDate, short offsetDays)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "App.proc_PeriodGetYear";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_sourceDate);
                            _command.Parameters.Add(_offsetDays);
                            _command.Parameters.Add(_outputDate);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (DateTime)_outputDate.Value;

                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return sourceDate;
                }
            });
        }

        #endregion

        #region Tasks
        public Task<bool> IsTaskProject(string taskCode)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Task.proc_IsProject";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_taskCode);
                            _command.Parameters.Add(_isProject);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (bool)_isProject.Value;

                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return false;
                }
            });

        }

        public Task<bool> IsTaskFullyInvoiced(string taskCode)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Task.proc_FullyInvoiced";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_taskCode);
                            _command.Parameters.Add(_isFullyInvoiced);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (bool)_isFullyInvoiced.Value;

                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return false;
                }
            });
        }

        public Task<string> ParentTaskCode(string taskCode)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Task.proc_Parent";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_taskCode);
                            _command.Parameters.Add(_parentTaskCode);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (string)_parentTaskCode.Value;

                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return string.Empty;
                }
            });
        }

        public Task<string> ProjectTaskCode(string taskCode)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Task.proc_Project";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_taskCode);
                            _command.Parameters.Add(_parentTaskCode);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (string)_parentTaskCode.Value;

                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return string.Empty;
                }
            });
        }

        public Task<string> GetNextTaskCode(string activityCode)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Task.proc_NextCode";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_activityCode);
                            _command.Parameters.Add(_taskCode);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (string)_taskCode.Value;

                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return string.Empty;
                }
            });
        }

        public Task<string> TaskCopy(string taskCode)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Task.proc_Copy";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_taskCode);
                            _command.Parameters.Add(_parentTaskCode);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (string)_parentTaskCode.Value;

                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return string.Empty;
                }
            });
        }

        public Task<short> GetTaskAtttributeOrder(string taskCode)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Task.proc_NextAttributeOrder";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_taskCode);
                            _command.Parameters.Add(_printOrder);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (short)_printOrder.Value;

                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return (short)10;
                }
            });
        }

        public Task<short> GetTaskOperationNumber(string taskCode)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Task.proc_NextOperationNumber";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_taskCode);
                            _command.Parameters.Add(_operationNumber);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (short)_operationNumber.Value;

                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return (short)10;
                }
            });
        }

        public Task<decimal> GetTaskCost(string taskCode)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Task.proc_Cost";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_taskCode);
                            _command.Parameters.Add(_totalCost);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (decimal)_totalCost.Value;

                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return (decimal)0;
                }
            });
        }

        public Task<string> TaskPay(string taskCode, bool postPayment)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Task.proc_Pay";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_invoiceNumber);
                            _command.Parameters.Add(_postPayment);
                            _command.Parameters.Add(_paymentCode);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (string)_paymentCode.Value;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return string.Empty;
                }
            });
        }

        public Task<string> TaskTaxCodeDefault(string accountCode, string cashCode)
        {
            return Task.Run(() =>
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
                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Task.proc_DefaultTaxCode";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_accountCode);
                            _command.Parameters.Add(_cashCode);
                            _command.Parameters.Add(_taxCode);

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (string)_taxCode.Value;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return string.Empty;
                }
            });
        }

        public Task<NodeEnum.InvoiceType> TaskInvoiceTypeDefault(string taskCode)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Task.proc_DefaultInvoiceType";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_taskCode);
                            _command.Parameters.Add(_invoiceType);
  

                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (NodeEnum.InvoiceType)_invoiceType.Value;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return NodeEnum.InvoiceType.SalesInvoice;
                }
            });
        }

        public Task<NodeEnum.DocType> TaskDocTypeDefault(string taskCode)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Task.proc_DefaultDocType";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_taskCode);
                            _command.Parameters.Add(_docType);


                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (NodeEnum.DocType)_docType.Value;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return NodeEnum.DocType.SalesOrder;
                }
            });
        }

        public Task<DateTime> TaskPaymentOnDefault(string accountCode, DateTime actionOn)
        {
            return Task.Run(() =>
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


                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Task.proc_DefaultPaymentOn";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_accountCode);
                            _command.Parameters.Add(_actionOn);
                            _command.Parameters.Add(_paymentOn);
                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (DateTime)_paymentOn.Value;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return actionOn;
                }
            });
        }

        public Task<string> TaskEmailAddress(string taskCode)
        {
            return Task.Run(() =>
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

                    using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                    {
                        _connection.Open();
                        using (SqlCommand _command = _connection.CreateCommand())
                        {
                            _command.CommandText = "Task.proc_EmailAddress";
                            _command.CommandType = CommandType.StoredProcedure;
                            _command.Parameters.Add(_taskCode);
                            _command.Parameters.Add(_emailAddress);


                            _command.ExecuteNonQuery();
                        }
                        _connection.Close();
                    }

                    return (string)_emailAddress.Value;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return string.Empty;
                }
            });
        }
        #endregion

        #region logs
        public string ErrorLog(Exception e)
        {
            string ParseString(string message)
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
                string eventMessage = $"{ParseString(e.Message)},{e.Source},{e.TargetSite.Name.ToString()},{ParseString(e.InnerException != null ? e.InnerException.Message : string.Empty)}";
                return EventLog(NodeEnum.EventType.IsError, eventMessage);
            }
            catch //(Exception err)
            {
                return string.Empty;
            }
        }

        public string EventLog(NodeEnum.EventType eventType, string eventMessage)
        {
            try
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

                using (SqlConnection _connection = new SqlConnection(Database.GetConnectionString()))
                {
                    _connection.Open();
                    using (SqlCommand _command = _connection.CreateCommand())
                    {
                        _command.CommandText = "App.proc_EventLog";
                        _command.CommandType = CommandType.StoredProcedure;

                        _command.Parameters.Add(_eventMessage);
                        _command.Parameters.Add(_eventType);
                        _command.Parameters.Add(_logCode);
                        _command.ExecuteNonQuery();
                    }
                    _connection.Close();
                }

                return _logCode.Value == null ? string.Empty : (string)_logCode.Value;
            }
            catch //(Exception err)
            {
                return string.Empty;
            }
        }

        #endregion


    }
}

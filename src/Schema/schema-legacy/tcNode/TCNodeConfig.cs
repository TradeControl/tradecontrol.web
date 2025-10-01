using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace TradeControl.Node
{
    public struct SemVer
    {
        public short Major;
        public short Minor;
        public int ReleaseNumber;

        public float VersionNumber { get { return float.Parse(string.Concat(Major, '.', Minor)); } }

        public void FromString(string version)
        {
            try
            {
                string[] vs = version.Split('.');
                Major = short.Parse(vs[0]);
                Minor = short.Parse(vs[1]);
                ReleaseNumber = int.Parse(vs[2]);

            }
            catch (Exception e)
            {
                Major = 0;
                Minor = 0;
                ReleaseNumber = 0;


                throw e;
            }
        }

        public override string ToString()
        {
            return $"{Major}.{Minor}.{ReleaseNumber}";
        }

        public static bool operator <(SemVer v1, SemVer v2)
        {
            return v1.VersionNumber < v2.VersionNumber || (v1.VersionNumber == v2.VersionNumber && v1.ReleaseNumber < v2.ReleaseNumber);
        }

        public static bool operator >(SemVer v1, SemVer v2)
        {
            return v1.VersionNumber > v2.VersionNumber || (v1.VersionNumber == v2.VersionNumber && v1.ReleaseNumber > v2.ReleaseNumber);
        }
    }

    public sealed class TCNodeConfig : IDisposable
    {
        public string SqlServerName { get; }
        public AuthenticationMode Authentication { get; }
        public string SqlUserName { get; }
        public string DatabaseName { get; }
        public string Password { get; }

        public DemoInstallMode InstallMode = DemoInstallMode.Activities;
        public int CommandTimeout = 60;

        const string TCNodeCreationScript = "tc_create_node";

        private bool IsInError = false;

        public TCNodeConfig(string sqlServerName, AuthenticationMode authenticationMode, string sqlUserName, string databaseName, string password)
        {
            SqlServerName = sqlServerName;
            Authentication = authenticationMode;
            SqlUserName = sqlUserName;
            DatabaseName = databaseName;
            Password = password;
        }

        public TCNodeConfig()
        {
            Authentication = (AuthenticationMode)Properties.Settings.Default.AuthenticationMode;
            SqlServerName = Properties.Settings.Default.SqlServerName;
            SqlUserName = Properties.Settings.Default.SqlUserName;
            DatabaseName = Properties.Settings.Default.DatabaseName;
        }

        public void Dispose()
        {
            Properties.Settings.Default.SqlServerName = SqlServerName;
            Properties.Settings.Default.AuthenticationMode = (int)Authentication;
            Properties.Settings.Default.SqlUserName = SqlUserName;
            Properties.Settings.Default.DatabaseName = DatabaseName;
            Properties.Settings.Default.Save();
        }

        #region version
        public SemVer InstalledVersion = new SemVer();

        public static SemVer CurrentVersion
        {
            get
            {
                try
                {
                    SemVer version = new SemVer();
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    AssemblyName assemblyName = assembly.GetName();
                    version.FromString(assemblyName.Version.ToString());
                    return version;
                }
                catch
                {
                    return new SemVer();
                }
            }
        }
        #endregion

        #region Connection and Authentication
        public bool Authenticated
        {
            get
            {
                try
                {
                    if ((DatabaseName.Length > 0 && SqlServerName.Length > 0)
                        && (Authentication == AuthenticationMode.SqlServer && Password.Length > 0 || Authentication == AuthenticationMode.Windows))
                        using (SqlConnection connection = new SqlConnection(ConnectionString))
                        {
                            connection.Open();
                            return connection.State == System.Data.ConnectionState.Open;
                        }
                    else
                        return false;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    throw e;
                }
            }
        }

        private bool ServerAuthenticated
        {
            get
            {
                try
                {
                    if ((SqlServerName.Length > 0)
                        && (Authentication == AuthenticationMode.SqlServer && Password.Length > 0 || Authentication == AuthenticationMode.Windows))
                        using (SqlConnection connection = new SqlConnection(ServerConnectionString))
                        {
                            connection.Open();
                            return connection.State == System.Data.ConnectionState.Open;
                        }
                    else
                        return false;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    throw e;
                }
            }
        }

        public string ConnectionString
        {
            get
            {
                try
                {
                    string connectionStr = string.Empty;
                    switch (Authentication)
                    {
                        case AuthenticationMode.Windows:
                            connectionStr = $"Data Source={SqlServerName};Initial Catalog={DatabaseName};Integrated Security=True";
                            break;
                        case AuthenticationMode.SqlServer:
                            connectionStr = $"Data Source={SqlServerName};Initial Catalog={DatabaseName};Persist Security Info=True;User Id={SqlUserName};Password={Password};";
                            break;
                    }

                    return connectionStr;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return string.Empty;
                }
            }
        }

        private string ServerConnectionString
        {
            get
            {
                try
                {
                    string connectionStr = string.Empty;
                    switch (Authentication)
                    {
                        case AuthenticationMode.Windows:
                            connectionStr = $"Data Source={SqlServerName};Integrated Security=True";
                            break;
                        case AuthenticationMode.SqlServer:
                            connectionStr = $"Data Source={SqlServerName};Persist Security Info=True;User Id={SqlUserName};Password={Password};";
                            break;
                    }

                    return connectionStr;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return string.Empty;
                }
            }
        }

        public List<string> SqlDatabases
        {
            get
            {
                try
                {
                    List<string> sqlDatabases = new List<string>();

                    if (ServerAuthenticated)
                    {
                        string[] excluded_dbs = new string[] { "model", "msdb", "master", "tempdb" };

                        using (SqlConnection connection = new SqlConnection(ServerConnectionString))
                        {
                            connection.Open();

                            sqlDatabases = connection.GetSchema(SqlClientMetaDataCollectionNames.Databases).Select().OrderBy(s => s.Field<string>("database_name"))
                                            .Where(f => !excluded_dbs.Contains(f.Field<string>("database_name"))).Select(f => f.Field<string>("database_name")).ToList<string>();

                            connection.Close();
                        }
                    }

                    return sqlDatabases;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return new List<string>();
                }
            }
        }
        public static List<string> SqlServers
        {
            get
            {
                try
                {
                    List<string> sqlServers = new List<string>();

                    SqlDataSourceEnumerator instance = SqlDataSourceEnumerator.Instance;
                    DataTable sources = instance.GetDataSources();
                    foreach (DataRow row in sources.Rows)
                    {
                        string sqlname = string.Concat(row[0].ToString(), '\\', row[1].ToString());
                        sqlServers.Add(sqlname);
                    }
                    return sqlServers;
                }
                catch {
                    return new List<string>();
                }
            }
        }

        public string WinUserName
        {
            get
            {
                try
                {
                    if (Authenticated)
                    {
                        const string query = "select SUSER_SNAME() AS UserName";
                        string userName = string.Empty;

                        using (SqlConnection connection = new SqlConnection(ConnectionString))
                        {
                            connection.Open();
                            using (SqlCommand cmd = new SqlCommand(query, connection))
                            {
                                SqlDataReader reader = cmd.ExecuteReader();

                                while (reader.Read())
                                    userName = reader["UserName"].ToString();
                            }

                        }

                        return userName;
                    }
                    else
                        return string.Empty;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    throw e;
                }
            }
        }
        #endregion

        #region Instance State
        public bool IsEmptyDatabase
        {
            get
            {
                try
                {
                    const string query = "select count(*) AS tbcount from sys.tables where type = 'U'";
                    int tbcount = 0;

                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand cmd = new SqlCommand(query, connection))
                        {
                            SqlDataReader reader = cmd.ExecuteReader();

                            while (reader.Read())
                                tbcount = (int)reader["tbcount"];
                        }

                    }

                    return tbcount == 0;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    throw e;
                }
            }
        }

        public bool IsTCNode
        {
            get
            {
                try
                {
                    string query = "select count(*) AS tbcount from sys.tables where type = 'U' and [name] = 'tbInstall' and SCHEMA_NAME(schema_id) = 'App'";
                    int tbcount = 0;

                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        using (SqlCommand cmd = new SqlCommand(query, connection))
                        {
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                    tbcount = (int)reader["tbcount"];
                                reader.Close();
                            }
                        }

                        if (tbcount == 1)
                        {
                            query = "SELECT App.fnVersion() VersionString";
                            using (SqlCommand cmd = new SqlCommand(query, connection))
                            {
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                        InstalledVersion.FromString(reader["VersionString"].ToString());
                                    reader.Close();
                                }
                            }
                        }

                    }

                    return tbcount == 1;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    throw e;
                }
            }
        }

        public bool IsUpToDate
        {
            get
            {
                return !(CurrentVersion > InstalledVersion);
            }
        }

        public bool IsInitialised
        {
            get
            {
                try
                {
                    string query = "select * from App.tbOptions";
                    bool isInit = false;

                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        using (SqlCommand cmd = new SqlCommand(query, connection))
                        {
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                isInit = reader.HasRows;
                                reader.Close();
                            }
                        }
                    }

                    return isInit;

                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    throw e;
                }
            }
        }

        public bool IsConfigured
        {
            get
            {
                try
                {
                    string query = "select * from App.tbUom";
                    bool isConfigured = false;

                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        using (SqlCommand cmd = new SqlCommand(query, connection))
                        {
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                isConfigured = reader.HasRows;
                                reader.Close();
                            }
                        }
                    }

                    return isConfigured;

                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    throw e;
                }
            }
        }

        bool IsDeprecatedVersion
        {
            get
            {
                return InstalledVersion.Major != CurrentVersion.Major;
            }
        }
        #endregion

        #region Installation and Upgrades
        public bool InstallNode(BackgroundWorker sender)
        {
            try
            {
                RunScript(sender, Properties.Resources.ResourceManager.GetString(TCNodeCreationScript));
                UpgradeNode(sender);
                return true;
            }
            catch (Exception e)
            {
                ErrorLog(e);
                throw e;
            }
        }

        public void UpgradeNode(BackgroundWorker sender)
        {
            try
            {
                float percentComplete = 0;
                List<string> upgrades = new List<string>();

                if (!IsTCNode)
                    throw new Exception("Upgrade error: unrecognised database");
                else if (IsDeprecatedVersion)
                    throw new Exception($"Version {InstalledVersion.Major} is deprecated. Contact support to upgrade to version {CurrentVersion.Major}");

                foreach (string candidate in Properties.Settings.Default.SqlScripts)
                {
                    string[] parse = candidate.Split('_');
                    string versionString = string.Concat(parse[parse.Length - 3], '.', parse[parse.Length - 2], '.', parse[parse.Length - 1]);
                    SemVer candidateVersion = new SemVer();
                    candidateVersion.FromString(versionString);
                    if (candidateVersion > InstalledVersion)
                        upgrades.Add(candidate);
                }

                for (int query = 0; query < upgrades.Count; query++)
                {

                    RunScript(null, Properties.Resources.ResourceManager.GetString(upgrades[query]));
                    percentComplete = ((float)query / upgrades.Count) * 100;
                    sender?.ReportProgress((int)percentComplete);
                }

                RegisterRelease();
            }
            catch (Exception e)
            {
                ErrorLog(e);
                throw e;
            }
        }

        private void RegisterRelease()
        {
            try
            {
                string query = $"INSERT INTO App.tbInstall (SQLDataVersion, SQLRelease) VALUES ({CurrentVersion.VersionNumber},{CurrentVersion.ReleaseNumber})";

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = query;
                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                ErrorLog(e);
                throw e;
            }

        }

        private void RunScript(BackgroundWorker sender, string scriptString)
        {
            try
            {
                SqlScript script = new SqlScript(scriptString);
                float percentComplete = 0;

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    for (int query = 0; query < script.Count; query++)
                    {

                        using (SqlCommand command = connection.CreateCommand())
                        {
                            command.CommandText = script[query];
                            command.ExecuteNonQuery();
                        }

                        percentComplete = ((float)query / script.Count) * 100;
                        sender?.ReportProgress((int)percentComplete);
                    }

                    connection.Close();
                }


            }
            catch (Exception e)
            {
                ErrorLog(e);
                throw e;
            }
        }
        #endregion

        #region Configuration and Demos
        public void ConfigureNode(string subjectCode,
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
                string unitOfCharge = string.Empty;

                using (dbNodeNetworkDataContext db = new dbNodeNetworkDataContext(ConnectionString))
                {
                    unitOfCharge = (from tb in db.tbUocs where tb.UocName == uocName select tb.UnitOfCharge).FirstOrDefault();
                }

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "App.proc_NodeInitialisation";
                        command.CommandType = CommandType.StoredProcedure;

                        SqlParameter p1 = command.CreateParameter();
                        p1.DbType = DbType.String;
                        p1.ParameterName = "@SubjectCode";
                        p1.Value = subjectCode;
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

                        command.ExecuteNonQuery();

                    }
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                ErrorLog(e);
                throw e;
            }

        }

        public List<string> UnitOfChargeNames
        {
            get
            {
                try
                {
                    List<string> uocNames;
                    using (dbNodeNetworkDataContext db = new dbNodeNetworkDataContext(ConnectionString))
                    {
                        uocNames = (from tb in db.tbUocs orderby tb.UocName select tb.UocName).ToList<string>();
                    }

                    return uocNames;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    throw e;
                }
            }
        }

        public string UnitOfChargeDefault
        {
            get
            {
                try
                {
                    string uocName;

                    using (dbNodeNetworkDataContext db = new dbNodeNetworkDataContext(ConnectionString))
                    {
                        uocName = (from tb in db.tbUocs where tb.UnitOfCharge == "BTC" select tb.UocName).FirstOrDefault();
                    }

                    return uocName;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    throw e;
                }
            }       
        }

        public string UnitOfCharge
        {
            get
            {
                try
                {
                    string uoc;

                    using (dbNodeNetworkDataContext db = new dbNodeNetworkDataContext(ConnectionString))
                    {
                        uoc = (from tb in db.tbOptions select tb.UnitOfCharge).FirstOrDefault();
                    }

                    return uoc;

                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    throw e;
                }
            }
        }

        public List<string> TemplateNames
        {
            get
            {
                try
                {
                    List<string> tempateNames;
                    using (dbNodeNetworkDataContext db = new dbNodeNetworkDataContext(ConnectionString))
                    {
                        tempateNames = (from tb in db.tbTemplates orderby tb.TemplateName select tb.TemplateName).ToList<string>();
                    }

                    return tempateNames;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return new List<string>();
                }
            }
        }

        public void InstallBasicSetup(  string templateName,
                                        short financialMonth,
                                        CoinType coinType,
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
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
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

                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                ErrorLog(e);
                throw e;
            }
        }

        public void InstallServicesDemo()
        {
            InstallDemo("App.proc_DemoServices");
        }

        public void InstallManufacturingDemo()
        {
            InstallDemo("App.proc_DemoBom");
        }

        private void InstallDemo(string procName)
        {
            try
            {
                bool createOrders = false, InvoiceOrders = false, payInvoices = false;

                switch (InstallMode)
                {
                    case DemoInstallMode.Activities:
                        createOrders = false; InvoiceOrders = false; payInvoices = false;
                        break;
                    case DemoInstallMode.Orders:
                        createOrders = true; InvoiceOrders = false; payInvoices = false;
                        break;
                    case DemoInstallMode.Invoices:
                        createOrders = true; InvoiceOrders = true; payInvoices = false;
                        break;
                    case DemoInstallMode.Payments:
                        createOrders = true; InvoiceOrders = true; payInvoices = true;
                        break;

                }

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = procName;
                        command.CommandType = CommandType.StoredProcedure;

                        SqlParameter p1 = command.CreateParameter();
                        p1.DbType = DbType.Boolean;
                        p1.ParameterName = "@CreateOrders";
                        p1.Value = createOrders;
                        command.Parameters.Add(p1);

                        SqlParameter p2 = command.CreateParameter();
                        p2.DbType = DbType.Boolean;
                        p2.ParameterName = "@InvoiceOrders";
                        p2.Value = InvoiceOrders;
                        command.Parameters.Add(p2);

                        SqlParameter p3 = command.CreateParameter();
                        p3.DbType = DbType.Boolean;
                        p3.ParameterName = "@PayInvoices";
                        p3.Value = payInvoices;
                        command.Parameters.Add(p3);

                        command.CommandTimeout = CommandTimeout;
                        command.ExecuteNonQuery();

                    }
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                ErrorLog(e);
                throw e;
            }
        }
        #endregion

        #region Add Users
        public void AddUser(    string userName,
                                string passWord,
                                bool createLoginAccount,
                                string fullName,
                                string homeAddress,
                                string emailAddress,
                                string mobileNumber,
                                string calendarCode,
                                bool isAdminstrator)
        {
            try
            {
                if (passWord.Length > 0 && createLoginAccount)
                    CreateLogin(userName, passWord);

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = "Usr.proc_AddUser";
                        command.CommandType = CommandType.StoredProcedure;

                        SqlParameter p1 = command.CreateParameter();
                        p1.DbType = DbType.String;
                        p1.ParameterName = "@UserName";
                        p1.Value = userName;
                        command.Parameters.Add(p1);

                        SqlParameter p2 = command.CreateParameter();
                        p2.DbType = DbType.String;
                        p2.ParameterName = "@FullName";
                        p2.Value = fullName;
                        command.Parameters.Add(p2);

                        SqlParameter p3 = command.CreateParameter();
                        p3.DbType = DbType.String;
                        p3.ParameterName = "@HomeAddress";
                        p3.Value = homeAddress;
                        command.Parameters.Add(p3);

                        SqlParameter p4 = command.CreateParameter();
                        p4.DbType = DbType.String;
                        p4.ParameterName = "@EmailAddress";
                        p4.Value = emailAddress;
                        command.Parameters.Add(p4);

                        SqlParameter p5 = command.CreateParameter();
                        p5.DbType = DbType.String;
                        p5.ParameterName = "@MobileNumber";
                        p5.Value = mobileNumber;
                        command.Parameters.Add(p5);

                        SqlParameter p6 = command.CreateParameter();
                        p6.DbType = DbType.String;
                        p6.ParameterName = "@CalendarCode";
                        p6.Value = calendarCode;
                        command.Parameters.Add(p6);

                        SqlParameter p7 = command.CreateParameter();
                        p7.DbType = DbType.Boolean;
                        p7.ParameterName = "@IsAdministrator";
                        p7.Value = isAdminstrator;
                        command.Parameters.Add(p7);

                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                ErrorLog(e);
                throw e;
            }
        }

        private void CreateLogin(string userName, string passWord)
        {
            string commandText = $"CREATE LOGIN [{userName}] WITH PASSWORD=N'{passWord}'";

            using (SqlConnection connection = new SqlConnection(ServerConnectionString))
            {
                connection.Open();
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = commandText;
                    command.ExecuteNonQuery();
                }
            }

        }

        public List<string> CalendarCodes
        {
            get
            {
                try
                {
                    List<string> calendarCodes = new List<string>();

                    const string query = "select CalendarCode from App.tbCalendar";

                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();
                        using (SqlCommand cmd = new SqlCommand(query, connection))
                        {
                            SqlDataReader reader = cmd.ExecuteReader();

                            while (reader.Read())
                                calendarCodes.Add(reader["CalendarCode"].ToString());
                        }

                    }

                    return calendarCodes;
                }
                catch (Exception e)
                {
                    ErrorLog(e);
                    return new List<string>();
                }
            }
        }

        #endregion

        #region Error Log
        private async void ErrorLog(Exception e)
        {
            if (!IsInError)
            {
                IsInError = true;
                bool result = await ErrorLogAsync(e);
                if (!result)
                    throw new Exception(string.Format(Properties.Resources.ErrorLogFailure, ErrorLogFileName));
                
            }
        }

        private Task<bool> ErrorLogAsync(Exception e)
        {
            return Task.Run(() =>
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
                    string errorLogFile = ErrorLogFileName;
                    string line, innerException;
                    bool newFile = !File.Exists(errorLogFile);

                    using (StreamWriter stream = new StreamWriter(errorLogFile, true, Encoding.ASCII))
                    {
                        if (newFile)
                            stream.WriteLine(Properties.Resources.ErrorLogHeader);

                        innerException = e.InnerException != null ? e.InnerException.Message : string.Empty;
                        line = $"{DateTime.Now},{ParseString(e.Message)},{e.Source},{e.TargetSite.Name},{ParseString(innerException)}";
                        stream.WriteLine(line);
                    }

                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        private string ErrorLogFileName
        {
            get
            {
                const string fileName = "tcnodeconfig_errorlog.csv";
                string logFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Trade Control";

                if (!Directory.Exists(logFolder))
                    Directory.CreateDirectory(logFolder);

                return string.Concat(logFolder, '\\', fileName);
            }
        }
        #endregion
    }

    public class SqlScript : List<string>
    {
        private const string Delimiter = "go";

        public SqlScript(List<string> script) : base()
        {
            string statement = string.Empty;

            foreach (string line in script)
            {
                if (line.Trim().ToLower() == Delimiter)
                {
                    this.Add(statement);
                    statement = string.Empty;
                }
                else
                    statement += string.Concat(line, '\n');
            }
        }

        public SqlScript(string script) : base()
        {
            List<string> Lines = new List<string>();
            string s = string.Empty;
            string buff;

            ASCIIEncoding ascii = new ASCIIEncoding();
            Byte[] bytes = ascii.GetBytes(script);

            for (int i = 0; i < bytes.Length; i++)
            {
                switch (bytes[i])
                {
                    case 10:
                        break;
                    case 13:
                        if (s.Trim().ToLower() == Delimiter)
                        {
                            buff = string.Empty;
                            foreach (string line in Lines)
                            {
                                buff += String.Format("{0}\n", line);
                            }
                            this.Add(buff);
                            Lines.Clear();
                        }
                        else
                            Lines.Add(s);
                        s = string.Empty;
                        break;
                    default:
                        s += script[i];
                        break;
                }
            }

            if (string.IsNullOrEmpty(s.Trim()))
                Lines.Add(s);
        }
    }

}

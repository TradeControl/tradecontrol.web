using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using System.Diagnostics;
using System.Windows.Navigation;
using System.ComponentModel;
using System.Threading;

namespace TradeControl.Node
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Form Events
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            lbAssemblyVersion.Content = TCNodeConfig.CurrentVersion.ToString();

            using (TCNodeConfig config = new TCNodeConfig())
            {
                SqlServerName = config.SqlServerName;
                Authentication = config.Authentication;
                SqlUserName = config.SqlUserName;
                DatabaseName = config.DatabaseName;                
            }

            if (DatabaseName.Length > 0)
                TestConnection();

        }
        #endregion

        #region Properties
        AuthenticationMode Authentication
        {
            get
            {
                return (AuthenticationMode)cbAuthenticationMode.SelectedIndex;
            }
            set
            {
                cbAuthenticationMode.SelectedIndex = (int)value;
            }
        }

        private string SqlServerName
        {
            get
            {
                return cbSqlServerName.Text;
            }
            set
            {
                cbSqlServerName.Text = value;
            }
        }

        private string DatabaseName
        {
            get
            {
                return cbDatabaseName.Text;
            }
            set
            {
                cbDatabaseName.Text = value;
            }
        }
        private string SqlUserName
        {
            get
            {
                return tbSqlUserName.Text;
            }
            set
            {
                tbSqlUserName.Text = value;
            }
        }

        private string Password { get { return pbPassword.Password; } }

        #endregion

        #region Test Connection State
        private void TestConnection()
        {
            try
            {
                Cursor = Cursors.Wait;

                using (TCNodeConfig tcnode = new TCNodeConfig(
                    this.SqlServerName,
                    this.Authentication,
                    this.SqlUserName,
                    this.DatabaseName,
                    this.Password))
                {
                    if (tcnode.Authenticated)
                    {
                        if (tcnode.IsEmptyDatabase)
                        {
                            btnTestConnection.IsEnabled = false;
                            lbConnectionStatus.Foreground = new SolidColorBrush(Colors.Red);
                            lbConnectionStatus.Text = Properties.Resources.ExecutionInProgress;
                            DisableFunctions();
                            InstallNode(tcnode);
                        }
                        else
                        {
                            lbConnectionStatus.Foreground = new SolidColorBrush(Colors.Blue);
                            lbConnectionStatus.Text = Properties.Resources.ConnectionSucceeded;

                            if (tcnode.IsTCNode)
                            {
                                lbUpgrade.Content = string.Format(Properties.Resources.UpgradeHeader, tcnode.DatabaseName);                                

                                if (!tcnode.IsUpToDate)
                                {
                                    lbUpgradeStatus.Text = string.Format(Properties.Resources.InstanceNeedsUpgrading, tcnode.DatabaseName, tcnode.InstalledVersion.ToString(), TCNodeConfig.CurrentVersion.ToString());
                                    lbUpgradeStatus.Foreground = new SolidColorBrush(Colors.Red);
                                    btnUpgrade.IsEnabled = true;
                                    tabsMain.SelectedItem = pageUpgrades;
                                }
                                else
                                {
                                    lbUpgradeStatus.Text = string.Format(Properties.Resources.InstanceIsUpToDate, tcnode.DatabaseName, tcnode.InstalledVersion.ToString());
                                    lbUpgradeStatus.Foreground = new SolidColorBrush(Colors.Black);
                                    btnUpgrade.IsEnabled = false;

                                    if (!tcnode.IsInitialised)
                                    {                                        
                                        if (cbUocName.Items.Count == 0)
                                        {
                                            List<string> uocNames = tcnode.UnitOfChargeNames;
                                            foreach (string uocName in uocNames)
                                                cbUocName.Items.Add(uocName);
                                            cbUocName.Text = tcnode.UnitOfChargeDefault;
                                        }

                                        btnBusinessDetails.IsEnabled = true;
                                        lbBusinessStatus.Foreground = new SolidColorBrush(Colors.Blue);
                                        lbBusinessStatus.Text = Properties.Resources.ConfigureEnabled;
                                        tabsMain.SelectedItem = pageBusinessDetails;

                                    }
                                    else
                                    {
                                        btnBusinessDetails.IsEnabled = false;
                                        lbBusinessStatus.Foreground = new SolidColorBrush(Colors.Red);
                                        lbBusinessStatus.Text = string.Format(Properties.Resources.ConfigureDisabled, tcnode.DatabaseName);

                                        btnAddUser.IsEnabled = true;
                                        lbAddUserStatus.Foreground = new SolidColorBrush(Colors.Blue);

                                        switch (tcnode.Authentication)
                                        {
                                            case AuthenticationMode.SqlServer:
                                                tbUsrLoginName.Text = this.SqlUserName;
                                                lbAddUserStatus.Text = string.Format(Properties.Resources.AddSqlUserToDatabase, tcnode.DatabaseName, tcnode.SqlServerName);
                                                pbUsrPassword.Password = this.Password;
                                                cbCreateLogin.IsChecked = true;
                                                cbCreateLogin.IsEnabled = true;
                                                cbLoginAsUser.IsEnabled = true;
                                                break;
                                            case AuthenticationMode.Windows:
                                                tbUsrLoginName.Text = tcnode.WinUserName;
                                                lbAddUserStatus.Text = string.Format(Properties.Resources.AddWinUserToDatabase, tcnode.DatabaseName, tcnode.SqlServerName);
                                                pbUsrPassword.Password = string.Empty;
                                                cbCreateLogin.IsChecked = false;
                                                cbLoginAsUser.IsChecked = false;
                                                cbCreateLogin.IsEnabled = false;
                                                cbLoginAsUser.IsEnabled = false;
                                                break;
                                        }

                                        lbAddUserStatus.Foreground = new SolidColorBrush(Colors.Blue);

                                        var calendarCodes = tcnode.CalendarCodes;
                                        foreach (string calendarCode in calendarCodes)
                                            cbUsrCalendarCode.Items.Add(calendarCode);
                                        if (cbUsrCalendarCode.Items.Count > 0)
                                            cbUsrCalendarCode.SelectedIndex = 0;

                                        tabsMain.SelectedItem = pageAddUsers;

                                        if (tcnode.IsConfigured)
                                        {
                                            btnBasicSetup.IsEnabled = false;
                                            lbBasicSetupStatus.Foreground = new SolidColorBrush(Colors.Red);
                                            lbBasicSetupStatus.Text = string.Format(Properties.Resources.InstanceIsConfigured, tcnode.DatabaseName, tcnode.SqlServerName);

                                            btnServices.IsEnabled = true;
                                            lbServicesStatus.Foreground = new SolidColorBrush(Colors.Blue);
                                            lbServicesStatus.Text = string.Format(Properties.Resources.InstallDemoData, tcnode.SqlServerName, tcnode.DatabaseName);

                                            btnManufacturing.IsEnabled = true;
                                            lbManufacturingStatus.Foreground = new SolidColorBrush(Colors.Blue);
                                            lbManufacturingStatus.Text = string.Format(Properties.Resources.InstallDemoData, tcnode.SqlServerName, tcnode.DatabaseName);
                                        }
                                        else
                                        {
                                            cbTemplateName.ItemsSource = tcnode.TemplateNames;
                                            if (cbTemplateName.Items.Count > 0)
                                                cbTemplateName.SelectedIndex = cbTemplateName.Items.Count - 1;

                                            if (tcnode.UnitOfCharge != "BTC")
                                            {
                                                tbBankName.Text = "THE BANK PLC";
                                                tbBankAddress.Text = "BANK ADDRESS";
                                                cbCoinType.SelectedIndex = (int)CoinType.Fiat;
                                            }
                                            else
                                            {
                                                tbBankName.Text = "N/A";
                                                tbBankAddress.Text = "N/A";
                                                cbCoinType.SelectedIndex = (int)CoinType.Main;
                                                tbCurrentAccount.Text = "TRADE";
                                                tbCA_AccountNumber.Text = string.Empty;
                                                tbCA_SortCode.Text = string.Empty;
                                                tbReserveAccount.Text = string.Empty;
                                                tbRA_AccountNumber.Text = string.Empty;
                                                tbRA_SortCode.Text = string.Empty;
                                            }

                                            btnBasicSetup.IsEnabled = true;
                                            lbBasicSetupStatus.Foreground = new SolidColorBrush(Colors.Blue);
                                            lbBasicSetupStatus.Text = string.Format(Properties.Resources.InstallBasicSetupuration, tcnode.DatabaseName, tcnode.SqlServerName);

                                            btnServices.IsEnabled = false;
                                            lbServicesStatus.Foreground = new SolidColorBrush(Colors.Red);
                                            lbServicesStatus.Text = Properties.Resources.InstanceIsUnconfigured;

                                            btnManufacturing.IsEnabled = false;
                                            lbManufacturingStatus.Foreground = new SolidColorBrush(Colors.Red);
                                            lbManufacturingStatus.Text = Properties.Resources.InstanceIsUnconfigured;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                lbConnectionStatus.Foreground = new SolidColorBrush(Colors.Red);
                                lbConnectionStatus.Text = Properties.Resources.UnrecognisedDatasource;
                                DisableFunctions();
                            }
                        }
                    }
                    else
                    {
                        lbConnectionStatus.Foreground = new SolidColorBrush(Colors.Red);
                        lbConnectionStatus.Text = Properties.Resources.ConnectionFailed;

                        DisableFunctions();

                    }

                }
            }
            catch (Exception err)
            {
                lbConnectionStatus.Text = $"{err.Source}.{err.TargetSite.Name}: {err.Message}";
                lbConnectionStatus.Foreground = new SolidColorBrush(Colors.Red);

                btnBusinessDetails.IsEnabled = false;
                lbBusinessStatus.Foreground = new SolidColorBrush(Colors.Red);
                lbBusinessStatus.Text = Properties.Resources.DataSourceNotFound;

                tabsMain.SelectedItem = pageConnection;
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void DisableFunctions()
        {
            btnBusinessDetails.IsEnabled = false;
            lbBusinessStatus.Foreground = new SolidColorBrush(Colors.Red);
            lbBusinessStatus.Text = Properties.Resources.DataSourceNotFound;

            btnAddUser.IsEnabled = false;
            lbAddUserStatus.Foreground = new SolidColorBrush(Colors.Red);
            lbAddUserStatus.Text = Properties.Resources.DataSourceNotFound;

            btnBasicSetup.IsEnabled = false;
            lbBasicSetupStatus.Foreground = new SolidColorBrush(Colors.Red);
            lbBasicSetupStatus.Text = Properties.Resources.DataSourceNotFound;

            btnUpgrade.IsEnabled = false;
            lbUpgrade.Content = string.Format(Properties.Resources.UpgradeHeader, "-");
            lbUpgradeStatus.Foreground = new SolidColorBrush(Colors.Red);
            lbUpgradeStatus.Text = Properties.Resources.DataSourceNotFound;


            btnServices.IsEnabled = false;
            lbServicesStatus.Foreground = new SolidColorBrush(Colors.Red);
            lbServicesStatus.Text = Properties.Resources.DataSourceNotFound;

            btnManufacturing.IsEnabled = false;
            lbManufacturingStatus.Foreground = new SolidColorBrush(Colors.Red);
            lbManufacturingStatus.Text = Properties.Resources.DataSourceNotFound;
        }
        #endregion

        #region Installation and upgrades
        private void InstallNode(TCNodeConfig tcnode)
        {
            try
            {
                BackgroundWorker installer = new BackgroundWorker
                {
                    WorkerReportsProgress = true
                };


                installer.DoWork += Installer_DoWork;
                installer.ProgressChanged += Installer_ProgressChanged;
                installer.RunWorkerCompleted += Installer_RunWorkerCompleted;
                installer.RunWorkerAsync(tcnode);

            }
            catch (Exception err)
            {
                lbConnectionStatus.Text = $"{err.Source}.{err.TargetSite.Name}: {err.Message}";
                lbConnectionStatus.Foreground = new SolidColorBrush(Colors.Red);

                tabsMain.SelectedItem = pageConnection;
            }
            finally
            {
                progressBar.Value = 0;
            }
        }

        private void Installer_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            { 
                TCNodeConfig tcnode = (TCNodeConfig)e.Argument;
                tcnode.InstallNode(sender as BackgroundWorker);
            }
            catch (Exception err)
            {
                MessageBox.Show($"{err.Message}", $"{err.Source}.{err.TargetSite.Name}", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Installer_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void Installer_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar.Value = 0;
            lbBusinessStatus.Text = Properties.Resources.InstalledSchema;
            TestConnection();
            btnTestConnection.IsEnabled = true;
        }

        public void UpgradeNode()
        {
            try
            {
                TCNodeConfig tcnode = new TCNodeConfig(
                    this.SqlServerName,
                    this.Authentication,
                    this.SqlUserName,
                    this.DatabaseName,
                    this.Password
                    );

                BackgroundWorker upgrader = new BackgroundWorker
                {
                    WorkerReportsProgress = true
                };


                upgrader.DoWork += Upgrader_DoWork;
                upgrader.ProgressChanged += Installer_ProgressChanged;
                upgrader.RunWorkerCompleted += Upgrader_RunWorkerCompleted;
                upgrader.RunWorkerAsync(tcnode);

            }
            catch (Exception err)
            {
                lbUpgradeStatus.Text = $"{err.Source}.{err.TargetSite.Name}: {err.Message}";
                lbUpgradeStatus.Foreground = new SolidColorBrush(Colors.Red);

                tabsMain.SelectedItem = pageUpgrades;
            }
            finally
            {
                progressBar.Value = 0;
            }
        }

        private void Upgrader_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                TCNodeConfig tcnode = (TCNodeConfig)e.Argument;
                tcnode.UpgradeNode(sender as BackgroundWorker);
            }
            catch (Exception err)
            {
                MessageBox.Show($"{err.Message}", $"{err.Source}.{err.TargetSite.Name}", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Upgrader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar.Value = 0;
            TestConnection();
        }
        #endregion

        #region Configuration
        private void BusinessDetails()
        {
            try
            {
                Cursor = Cursors.Wait;

                TCNodeConfig tcnode = new TCNodeConfig(
                    this.SqlServerName,
                    this.Authentication,
                    this.SqlUserName,
                    this.DatabaseName,
                    this.Password
                    );

                tcnode.ConfigureNode(
                    subjectCode: tbSubjectCode.Text,
                    businessName: tbBusinessName.Text,
                    fullName: tbFullName.Text,
                    businessAddress: tbBusinessAddress.Text,
                    businessEmailAddress: tbEmailAddress.Text,
                    userEmailAddress: tbEmailAddress.Text,
                    phoneNumber: tbPhoneNumber.Text,
                    companyNumber: tbCompanyNumber.Text,
                    vatNumber: tbVatNumber.Text,
                    calendarCode: tbCalendarCode.Text,
                    uocName: cbUocName.Text
                    );

                lbBusinessStatus.Text = string.Format(Properties.Resources.ConfigurationSuccess, this.cbDatabaseName.Text);
                lbBusinessStatus.Foreground = new SolidColorBrush(Colors.Blue);
                TestConnection();

            }
            catch (Exception err)
            {
                lbBusinessStatus.Text = $"{err.Source}.{err.TargetSite.Name}: {err.Message}";
                lbBusinessStatus.Foreground = new SolidColorBrush(Colors.Red);

            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void AddUser()
        {
            try
            {
                Cursor = Cursors.Wait;

                TCNodeConfig tcnode = new TCNodeConfig(
                    this.SqlServerName,
                    this.Authentication,
                    this.SqlUserName,
                    this.DatabaseName,
                    this.Password
                    );

                bool createLogin = (bool)cbCreateLogin.IsChecked && (bool)cbCreateLogin.IsEnabled;
                bool isAdministrator = (bool)cbIsAdministrator.IsChecked;

                tcnode.AddUser(
                    tbUsrLoginName.Text,
                    pbUsrPassword.Password,
                    createLogin,
                    tbUsrFullName.Text,
                    tbUsrAddress.Text,
                    tbUsrEmailAddress.Text,
                    tbUsrMobile.Text,
                    cbUsrCalendarCode.Text,
                    isAdministrator
                    );

                if ((bool)cbLoginAsUser.IsChecked && (bool)cbLoginAsUser.IsEnabled)
                {
                    switch (tcnode.Authentication)
                    {
                        case AuthenticationMode.SqlServer:
                            this.SqlUserName = this.tbUsrLoginName.Text;
                            this.pbPassword.Password = this.pbUsrPassword.Password;
                            break;
                        case AuthenticationMode.Windows:
                            this.SqlUserName = this.tbUsrLoginName.Text;
                            break;
                    }

                    TestConnection();

                    tabsMain.SelectedItem = pageTutorials;
                    tabsTutorials.SelectedItem = pageBasicSetup;
                }
                else
                {
                    lbAddUserStatus.Text = string.Format(Properties.Resources.UserAddedSuccessfully, tbUsrLoginName.Text);
                }
            }
            catch (Exception err)
            {
                lbAddUserStatus.Text = $"{err.Source}.{err.TargetSite.Name}: {err.Message}";
                lbAddUserStatus.Foreground = new SolidColorBrush(Colors.Red);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void InstallBasicSetup()
        {
            try
            {
                Cursor = Cursors.Wait;

                TCNodeConfig tcnode = new TCNodeConfig(
                    this.SqlServerName,
                    this.Authentication,
                    this.SqlUserName,
                    this.DatabaseName,
                    this.Password
                    );

                if (!tcnode.IsConfigured)
                {
                    short financialMonth = (short)(cbFinancialYear.SelectedIndex + 1);

                    CoinType coinType = (CoinType)cbCoinType.SelectedIndex;
                    string templateName = (string)cbTemplateName.SelectedItem;

                    tcnode.InstallBasicSetup(
                        templateName,
                        financialMonth,
                        coinType,
                        tbGovAccountName.Text,
                        tbBankName.Text,
                        tbBankAddress.Text,
                        tbDummyAccount.Text,
                        tbCurrentAccount.Text,
                        tbCA_SortCode.Text,
                        tbCA_AccountNumber.Text,
                        tbReserveAccount.Text,
                        tbRA_SortCode.Text,
                        tbRA_AccountNumber.Text
                        );

                    btnBasicSetup.IsEnabled = false;
                    lbBasicSetupStatus.Text = string.Format(Properties.Resources.BasicSetupInstalled, tcnode.DatabaseName);

                    btnServices.IsEnabled = true;
                    lbServicesStatus.Foreground = new SolidColorBrush(Colors.Blue);
                    lbServicesStatus.Text = string.Format(Properties.Resources.InstallDemoData, tcnode.SqlServerName, tcnode.DatabaseName);

                    btnManufacturing.IsEnabled = true;
                    lbManufacturingStatus.Foreground = new SolidColorBrush(Colors.Blue);
                    lbManufacturingStatus.Text = string.Format(Properties.Resources.InstallDemoData, tcnode.SqlServerName, tcnode.DatabaseName);
                }
                else
                {
                    lbBasicSetupStatus.Foreground = new SolidColorBrush(Colors.Red);
                    lbBasicSetupStatus.Text = string.Format(Properties.Resources.InstanceIsConfigured, tcnode.DatabaseName, tcnode.SqlServerName);
                }


            }
            catch (Exception err)
            {
                lbBasicSetupStatus.Text = $"{err.Source}.{err.TargetSite.Name}: {err.Message}";
                lbBasicSetupStatus.Foreground = new SolidColorBrush(Colors.Red);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void InstallServicesDemo()
        {
            try
            {
                Cursor = Cursors.Wait;
                progressBar.IsIndeterminate = true;
                btnServices.IsEnabled = false;
                btnManufacturing.IsEnabled = false;

                TCNodeConfig tcnode = new TCNodeConfig(
                    this.SqlServerName,
                    this.Authentication,
                    this.SqlUserName,
                    this.DatabaseName,
                    this.Password
                    );

                tcnode.CommandTimeout = int.Parse(cbServDemoTimeout.Text);

                if ((bool)rbSrvDemoActivities.IsChecked)
                    tcnode.InstallMode = DemoInstallMode.Activities;
                else if ((bool)rbSrvDemoCreateOrders.IsChecked)
                    tcnode.InstallMode = DemoInstallMode.Orders;
                else if ((bool)rbSrvDemoInvoiceOrder.IsChecked)
                    tcnode.InstallMode = DemoInstallMode.Invoices;
                else if ((bool)rbSrvDemoPayInvoices.IsChecked)
                    tcnode.InstallMode = DemoInstallMode.Payments;

                BackgroundWorker srvDemo = new BackgroundWorker
                {
                    WorkerReportsProgress = false
                };

                srvDemo.DoWork += SrvDemo_DoWork;
                srvDemo.RunWorkerCompleted += SrvDemo_RunWorkerCompleted;
                srvDemo.RunWorkerAsync(tcnode);

                lbServicesStatus.Text = Properties.Resources.ExecutionInProgress;
            }
            catch (Exception err)
            {
                lbServicesStatus.Text = $"{err.Source}.{err.TargetSite.Name}: {err.Message}";
                lbServicesStatus.Foreground = new SolidColorBrush(Colors.Red);
                progressBar.IsIndeterminate = false;
                btnServices.IsEnabled = true;
                btnManufacturing.IsEnabled = true;
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void SrvDemo_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lbServicesStatus.Text = string.Format(Properties.Resources.InstalledServicesDemo, cbDatabaseName.Text);
            lbServicesStatus.Foreground = new SolidColorBrush(Colors.Blue);
            progressBar.IsIndeterminate = false;
            btnServices.IsEnabled = true;
            btnManufacturing.IsEnabled = true;
        }

        private void SrvDemo_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                TCNodeConfig tcnode = (TCNodeConfig)e.Argument;
                tcnode.InstallServicesDemo();
            }
            catch (Exception err)
            {
                MessageBox.Show($"{err.Message}", $"{err.Source}.{err.TargetSite.Name}", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void InstallManufacturingDemo()
        {
            try
            {
                Cursor = Cursors.Wait;
                progressBar.IsIndeterminate = true;
                btnServices.IsEnabled = false;
                btnManufacturing.IsEnabled = false;

                TCNodeConfig tcnode = new TCNodeConfig(
                    this.SqlServerName,
                    this.Authentication,
                    this.SqlUserName,
                    this.DatabaseName,
                    this.Password
                    );

                tcnode.CommandTimeout = int.Parse(cbManDemoTimeout.Text);

                if ((bool)rbManDemoActivities.IsChecked)
                    tcnode.InstallMode = DemoInstallMode.Activities;
                else if ((bool)rbManDemoCreateOrders.IsChecked)
                    tcnode.InstallMode = DemoInstallMode.Orders;
                else if ((bool)rbManDemoInvoiceOrder.IsChecked)
                    tcnode.InstallMode = DemoInstallMode.Invoices;
                else if ((bool)rbManDemoPayInvoices.IsChecked)
                    tcnode.InstallMode = DemoInstallMode.Payments;

                BackgroundWorker manDemo = new BackgroundWorker
                {
                    WorkerReportsProgress = false
                };

                manDemo.DoWork += ManDemo_DoWork; ;
                manDemo.RunWorkerCompleted += ManDemo_RunWorkerCompleted; ;
                manDemo.RunWorkerAsync(tcnode);

                lbManufacturingStatus.Text = Properties.Resources.ExecutionInProgress;

            }
            catch (Exception err)
            {
                lbManufacturingStatus.Text = $"{err.Source}.{err.TargetSite.Name}: {err.Message}";
                lbManufacturingStatus.Foreground = new SolidColorBrush(Colors.Red);
                progressBar.IsIndeterminate = false;
                btnServices.IsEnabled = true;
                btnManufacturing.IsEnabled = true;
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void ManDemo_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lbManufacturingStatus.Text = string.Format(Properties.Resources.InstalledManDemo, cbDatabaseName.Text);
            lbManufacturingStatus.Foreground = new SolidColorBrush(Colors.Blue);
            progressBar.IsIndeterminate = false;
            btnServices.IsEnabled = true;
            btnManufacturing.IsEnabled = true;
        }

        private void ManDemo_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                TCNodeConfig tcnode = (TCNodeConfig)e.Argument;
                tcnode.InstallManufacturingDemo();
            }
            catch (Exception err)
            {
                MessageBox.Show($"{err.Message}", $"{err.Source}.{err.TargetSite.Name}", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Events
        private void CbAuthenticationMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gridCredentials != null)
            {
                gridCredentials.IsEnabled = this.Authentication == AuthenticationMode.SqlServer;
                pbUsrPassword.IsEnabled = this.Authentication == AuthenticationMode.SqlServer;
                cbCreateLogin.IsChecked = this.Authentication == AuthenticationMode.SqlServer;

            }
        }

        private void BtnTestConnection_Click(object sender, RoutedEventArgs e)
        {
            TestConnection();
        }

        private void BtnServers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Cursor = Cursors.Wait;
                cbSqlServerName.Items.Clear();

                List<string> localServers = TCNodeConfig.SqlServers;

                if (localServers.Count > 0)
                {
                    foreach (string serverName in localServers)
                        cbSqlServerName.Items.Add(serverName);

                    if (cbSqlServerName.Text.Length == 0)
                        cbSqlServerName.Text = localServers[0];
                }
            }
            catch (Exception err)
            {
                lbConnectionStatus.Text = err.Message;
                lbConnectionStatus.Foreground = new SolidColorBrush(Colors.Red);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void CbDatabaseName_DropDownOpened(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.Wait;
                cbDatabaseName.Items.Clear();

                TCNodeConfig tcnode = new TCNodeConfig(
                    this.SqlServerName,
                    this.Authentication,
                    this.SqlUserName,
                    this.DatabaseName,
                    this.Password
                    );

                List<string> localDatabases = tcnode.SqlDatabases;

                foreach (string database in localDatabases)
                    cbDatabaseName.Items.Add(database);
            }
            catch (Exception err)
            {
                lbConnectionStatus.Text = err.Message;
                lbConnectionStatus.Foreground = new SolidColorBrush(Colors.Red);
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void btnBusinessDetails_Click(object sender, RoutedEventArgs e)
        {
            BusinessDetails();
        }

        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            AddUser();
        }

        private void PbUsrPassword_KeyUp(object sender, KeyEventArgs e)
        {
            cbCreateLogin.IsEnabled = pbUsrPassword.Password.Length > 0;
            cbLoginAsUser.IsEnabled = pbUsrPassword.Password.Length > 0;
        }

        private void BtnBasicSetup_Click(object sender, RoutedEventArgs e)
        {
            InstallBasicSetup();
        }

        private void BtnManufacturing_Click(object sender, RoutedEventArgs e)
        {
            InstallManufacturingDemo();
        }

        private void BtnServices_Click(object sender, RoutedEventArgs e)
        {
            InstallServicesDemo();
        }

        private void BtnUpgrade_Click(object sender, RoutedEventArgs e)
        {
            UpgradeNode();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
        #endregion



    }
}

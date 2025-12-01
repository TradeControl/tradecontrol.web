# Change Log

Changes to 3.33.1, released February 2021. Previous releases are logged in the [development history archive](https://github.com/iamonnox/tradecontrol).

## Version 4

The purpose of Version 4 is to bring the schema naming conventions into alignment with the apps underlying [Production Theory](https://tradecontrol.github.io/articles/tc_production/). In consequence, all associated repositories will need to be updated before it can be published. 

[Sql Server project tcNodeDb4](https://github.com/TradeControl/tradecontrol.web/tree/master/src/schema/tcNodeDb4)

### 4.1.1

Name changes:

V3 Name | V4 Name
 -- | --
Activity | Object
Organisation | Subject
Org | Subject
Task | Project
tbMode | tbPolarity
CashMode | CashPolarity
AccountCode | SubjectCode
AccountName | SubjectName
AccountSource | SubjectSource
DefaultAccountCode | DefaultSubjectCode
AccountLookup | SubjectLookup
CashAccountCode | AccountCode
CashAccountName | AccountName 
Task.proc_IsProject | Project.proc_IsProjected
Task.proc_Project | Project.proc_Root
Task.proc_Mode | Task.proc_Polarity

## Version 3

[Sql Server project tcNodeDb](https://github.com/TradeControl/sqlnode/tree/master/src/tcNodeDb)

### 3.34.1

Completion of the [costing system](https://tradecontrol.github.io/tutorials/manufacturing#job-costing)

- [x] [Task.tbCostSet](https://github.com/TradeControl/sqlnode/blob/master/src/tcNodeDb/Task/Tables/tbCostSet.sql) - active set of user quotes for costing
- [x] [Task.Task_tbTask_TriggerUpdate](https://github.com/TradeControl/sqlnode/blob/master/src/tcNodeDb/Task/Tables/tbTask.sql) - remove tasks from cost set when set to ordered 
- [x] [Task.vwQuotes](https://github.com/TradeControl/sqlnode/blob/master/src/tcNodeDb/Task/Views/vwQuotes.sql) - quotes available for selection
- [x] [Task.vwCostSet](https://github.com/TradeControl/sqlnode/blob/master/src/tcNodeDb/Task/Views/vwCostSet.sql) - current user's set of quotes 
- [x] [Task.proc_CostSetAdd](https://github.com/TradeControl/sqlnode/blob/master/src/tcNodeDb/Task/Stored%20Procedures/proc_CostSetAdd.sql) - include task in the set
- [x] [Cash.vwStatementBase](https://github.com/TradeControl/sqlnode/blob/master/src/tcNodeDb/Cash/Views/vwStatementBase.sql) - split out the live company statement from the balance projection
- [x] [Cash.vwStatement](https://github.com/TradeControl/sqlnode/blob/master/src/tcNodeDb/Cash/Views/vwStatement.sql) - derive the company statement from the base dataset
- [x] [Cash.vwStatementWhatIf](https://github.com/TradeControl/sqlnode/blob/master/src/tcNodeDb/Cash/Views/vwStatementWhatIf.sql) - integrate the quotes, vat and company tax into the company statement 

### 3.34.2

- [x] [Task.proc_Pay](https://github.com/TradeControl/sqlnode/blob/master/src/tcNodeDb/Task/Stored%20Procedures/proc_Pay.sql) - fix auto-invoice date not matching payment on date

### 3.34.3

Authorisation and authentication support for the [Asp.Net Core interface](https://github.com/tradecontrol/tradecontrol.web).

- [x] Standard Asp.Net Core schema design. Unfortunately it uses the default dbo schema instead of AspNet.TableName.

### 3.34.4 

Integrates setup templates into the [Node Configuration](https://tradecontrol.github.io/tutorials/installing-sqlnode#basic-setup) program. There are only two at this stage, but more can be added.

- [x] [App.tbTemplate](https://github.com/TradeControl/sqlnode/blob/master/src/tcNodeDb/App/Tables/tbTemplate.sql) - a list of configuration templates and the associated stored procedure name.
- [x] [App.proc_TemplateCompanyGeneral](https://github.com/TradeControl/sqlnode/blob/master/src/tcNodeDb/App/Stored%20Procedures/proc_TemplateCompanyGeneral.sql) - a new template that supports vat and capital accounts
- [x] [App.proc_TemplateTutorials](https://github.com/TradeControl/sqlnode/blob/master/src/tcNodeDb/App/Stored%20Procedures/proc_TemplateTutorials.sql) - the original configuration template used by [the tutorials](https://tradecontrol.github.io/tutorials/overview)
- [x] [App.proc_BasicSetup](https://github.com/TradeControl/sqlnode/blob/master/src/tcNodeDb/App/Stored%20Procedures/proc_BasicSetup.sql) - @TemplateName param to execute user selected configuration.

### 3.34.5

- [x] [Cash.vwBalanceSheetAccounts](https://github.com/TradeControl/sqlnode/blob/master/src/tcNodeDb/Cash/Views/vwBalanceSheetAccounts.sql) - fix negative m/e balances zeroised
- [x] [Org.vwStatement](https://github.com/TradeControl/sqlnode/blob/master/src/tcNodeDb/Org/Views/vwStatement.sql) - remove unposted payments

### 3.34.6

Full script for the [Asp.Net Core interface](https://github.com/tradecontrol/tradecontrol.web). It has no impact on the 365 implementation since it uses the same algorithms.

### 3.34.7

- [x] [App.proc_TemplateCompanyHMRC2021](https://github.com/TradeControl/sqlnode/blob/master/src/tcNodeDb/App/Stored%20Procedures/proc_TemplateCompanyHMRC2021.sql) - initialisation template that maps all the codes used by the HRMC Accounts and Tax Return portal
- [x] [Cash.vwTaxLossesCarriedForward](https://github.com/TradeControl/sqlnode/blob/master/src/tcNodeDb/Cash/Views/vwTaxLossesCarriedForward.sql) - calculates available losses that can be carried forward on a tax return
- [x] [Cash.vwProfitAndLossData](https://github.com/TradeControl/sqlnode/blob/master/src/tcNodeDb/Cash/Views/vwProfitAndLossData.sql) - add CashTypeCode for accessing Corporation Tax totals

### 3.34.8

- [x] [Cash.vwBalanceSheetAccounts](https://github.com/TradeControl/sqlnode/blob/master/src/tcNodeDb/Cash/Views/vwBalanceSheetAccounts.sql) - fix capital balance calculating incorrectly for companies trading with multiple current and reserve accounts

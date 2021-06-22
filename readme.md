# Trade Control - Web Interface

Using ASP.NET Core to web enable the [Accounts](https://tradecontrol.github.io/accounts) and [MIS](https://tradecontrol.github.io/mis) interface on desktops and mobiles.

## Implementation Plan

### Phase 1 - Data Models

- [x] Entity Framework Core scaffold
- [x] Test model - Calendars

### Phase 2 - Business Logic

- [x] Asynchronous connection to the [sqlnode](https://github.com/tradecontrol/sqlnode)  
- [x] Test logic - Payment Entry

The implmented EF Core scaffold and business logic support the functionality of sql node version 3.34.2 for Phases 3-6. 

### Phase 3 - Web Environment

Requires sql node version 3.34.3 to support the AspNetCore.Identity datastore and registration process.

- [x] Authentication
- [x] Register new users 
- [x] Authorisation
- [x] [Device Detection](https://github.com/wangkanai/Detection)
- [x] Layouts and Navigation
- [x] Session service

### Phase 4 - Accounts Mode

Requires [sql node 3.34.6](https://github.com/tradecontrol/sqlnode) or higher.

- [x] Accounts Mode menu
- [x] Basic home page
- [x] Error trapping
- [x] Error Log
- [x] Payment Entry - bank accounts and adjustments 
- [x] Create and lookup organisations, category, tax and cash codes
- [x] Asset Entry - capital accounts for long-term assets and liabilities 
- [x] Cash Account Statements - reporting and maintenance 
- [x] Interbank Transfers - accruals and payments
- [x] Cash Account Maintenance - cash, asset and dummy accounts
- [x] Organisation Maintenance - create, edit and delete organisations, contacts and addresses
- [x] Organisation Enquiries - details, invoices, payments and SvD statements
- [x] Debtors and Creditors - current and historical balance sheet audit
- [x] Raise invoices and credit notes
- [x] Cancel invoices
- [x] Modify and reshedule invoices
- [x] Invoice Register - sales and purchases by period
- [x] Cash Code invoice summary
- [x] Unpaid income and expense
- [x] Host settings
- [x] [Website email service](https://github.com/jstedfast/MailKit)
- [x] Html document templates
- [x] Template manager
- [x] Configure email document templates (images and attachments)
- [x] File transfer (upload/download templates)
- [x] Email invoices to contacts
- [x] Email preview
- [x] Vat Statement and quarterly totals
- [x] Company Tax Statement and period totals
- [x] Tax rates
- [x] Balance Sheet by period
- [x] Profit and Loss by period
- [x] Category and Cash Code maintenance
- [x] Cash Totals
- [x] Tax Settings
- [x] System settings 
- [x] Period End maintenance and closedown
- [x] Error reporting template and support submission
- [x] Company initialisation
- [x] [sql node](https://github.com/tradecontrol/sqlnode) release

### Phase 5 - MIS Mode

- [ ] Definitions
- [ ] Interface
- [ ] Documents

### Phase 6 - Extensions

- [ ] [Supply-Chain Network](https://github.com/tradecontrol/network)
- [ ] [Bitcoin Wallet](https://github.com/tradecontrol/bitcoin)

## Versioning

[SemVer](http://semver.org/)

## Donations

[![Donate](https://www.paypalobjects.com/en_US/i/btn/btn_donate_SM.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=C55YGUTBJ4N36)

## Licence

The Trade Control Code licence is issued by Trade Control Ltd under a [GNU General Public Licence v3.0](https://www.gnu.org/licenses/gpl-3.0.en.html) 
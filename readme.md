# Trade Control - Web Interface

Using ASP.NET Core to web enable the [Accounts](https://tradecontrol.github.io/accounts) and [MIS](https://tradecontrol.github.io/mis) interface on desktops and mobiles.

## Implementation Plan

Because app functionality is now fully available from [Office](https://github.com/tradecontrol/office), the web interface can be complimentary and we do not have to implement everything in one go. Implementing payments, organisations and invoicing in [Accounts Mode](https://tradecontrol.github.io/tutorials/cash-book) will be a milestone for the first release. The second milestone will be a stand-alone browser-based version of that mode.

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
- [x] Authorisation - [attributes](https://github.com/TradeControl/tradecontrol.web/blob/master/src/TCWeb/Pages/Admin/Calendar/Create.cshtml), [views](https://github.com/TradeControl/tradecontrol.web/blob/master/src/TCWeb/Pages/Admin/Users/Index.cshtml), [handlers](https://github.com/TradeControl/tradecontrol.web/blob/master/src/TCWeb/Authorisation/AspNetAuthorizationHandler.cs), [page base class](https://github.com/TradeControl/tradecontrol.web/blob/master/src/TCWeb/Pages/DI_BasePageModel.cs) and [models](https://github.com/TradeControl/tradecontrol.web/blob/master/src/TCWeb/Pages/Admin/Users/Confirm.cshtml.cs)
- [x] [Device Detection](https://github.com/wangkanai/Detection)
- [x] Layouts and Navigation
- [x] Session service

### Phase 4 - Accounts Mode

Apply [web interface script 1](scripts/tc_web_interface_script1.sql) to sql node version 3.34.4

- [x] Accounts Mode menu
- [x] Payment Entry - bank accounts and adjustments 
- [x] Create and lookup organisations, category, tax and cash codes
- [x] Asset Entry - capital accounts for long-term assets and liabilities 
- [x] Cash Account Statements - reporting and maintenance 
- [ ] Definitions
- [ ] Interface
- [ ] Documents

### Phase 5 - MIS Mode

- [ ] Definitions
- [ ] Interface
- [ ] Documents

### Phase 6 - Full Web Version

- [ ] Administration
- [ ] Custom Menus
- [ ] Commercial skin

### Phase 7 - Extensions

- [ ] Supply-Chain Network
- [ ] Bitcoin Wallet

## Versioning

[SemVer](http://semver.org/)

## Donations

[![Donate](https://www.paypalobjects.com/en_US/i/btn/btn_donate_SM.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=C55YGUTBJ4N36)

## Licence

The Trade Control Code licence is issued by Trade Control Ltd under a [GNU General Public Licence v3.0](https://www.gnu.org/licenses/gpl-3.0.en.html) 
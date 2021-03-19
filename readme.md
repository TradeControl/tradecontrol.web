# Trade Control - Web Interface

Using ASP.NET Core to web enable the [Accounts](https://tradecontrol.github.io/accounts) and [MIS](https://tradecontrol.github.io/mis) interface on desktops and mobiles.

## Implementation Plan

Because app functionality is now fully available from [Office](https://github.com/tradecontrol/office), the web interface can be complimentary and we do not have to implement everything in one go. Implementing payments, organisations and invoicing in [Accounts Mode](https://tradecontrol.github.io/tutorials/cash-book) will be a milestone for the first release.

### Phase 1 - Data Models

- [x] Entity Framework Core scaffold
- [x] Test model - Calendars

### Phase 2 - Business Logic

- [x] Asynchronous connection to the [sqlnode](https://github.com/tradecontrol/sqlnode)  
- [x] Test logic - Payment Entry

The implmented EF Core scaffold and business logic support the functionality of sql node version 3.34 for Phases 3-6. 

### Phase 3 - Web Environment

- [ ] Authentication 
- [x] [Device Detection](https://github.com/wangkanai/Detection)
- [ ] Mobile/Desktop Layouts and Navigation
- [ ] Error reporting

### Phase 4 - Accounts Mode

- [ ] Definitions
- [ ] Interface
- [ ] Documents

### Phase 5 - MIS Mode

- [ ] Definitions
- [ ] Interface
- [ ] Documents
- [ ] Supply-Chain Network

### Phase 6 - Full Web Version

- [ ] Administration
- [ ] Custom Menus
- [ ] Commercial skin

### Phase 7 - Extensions

- [ ] Company Accounts
- [ ] Bitcoin Wallet

## Versioning

[SemVer](http://semver.org/)

## Donations

[![Donate](https://www.paypalobjects.com/en_US/i/btn/btn_donate_SM.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=C55YGUTBJ4N36)

## Licence

The Trade Control Code licence is issued by Trade Control Ltd under a [GNU General Public Licence v3.0](https://www.gnu.org/licenses/gpl-3.0.en.html) 
# Trade Control - Web Interface

Using ASP.NET Core to web enable the [Accounts](https://tradecontrol.github.io/accounts) and [MIS](https://tradecontrol.github.io/mis) interface on desktops and mobiles.

## Implementation Plan

Because app functionality is now fully available from [Office](https://github.com/tradecontrol/office), the web interface can be complimentary and we do not have to implement everything in one go. Implementing payments, organisations and invoicing in [Accounts Mode](https://tradecontrol.github.io/tutorials/cash-book) will be a milestone for the first release.

### Phase 1

- [x] Entity Framework Core scaffold
- [ ] Data logic
- [x] Mobile/Desktop/Tablet [detection](https://github.com/wangkanai/Detection)
- [x] Test model
- [ ] Test data logic

### Phase 2

- [ ] Authentication and menus
- [ ] Accounts interface

### Phase 3

- [ ] MIS
- [ ] Definitions
- [ ] Administration

### Phase 4

- [ ] Company Accounts
- [ ] Bitcoin Wallet

## Versioning

[SemVer](http://semver.org/)

## Donations

[![Donate](https://www.paypalobjects.com/en_US/i/btn/btn_donate_SM.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=C55YGUTBJ4N36)

## Licence

The Trade Control Code licence is issued by Trade Control Ltd under a [GNU General Public Licence v3.0](https://www.gnu.org/licenses/gpl-3.0.en.html) 
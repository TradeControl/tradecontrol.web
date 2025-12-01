# Change Log

Changes to 1.1.0, first released 1 July 2021.

## 2.0.0

[Sql node version 4.0.0]

- Major schema migration to Version 4 (see [Schema/changelog.md](../src/schema/changelog.md) for details)
- [SqlNode](https://github.com/tradecontrol/sqlnode) absorbed into the current repository 
- Updated .NET and packages to latest versions
- Mapped to new schema conventions (eg. Organisation → Subject, Task → Project)
- Added support for new authentication methods
- New AI Category Tree component for cash classification. [Test Script](categorytree_test_script.md)
---

## 1.1.4

[Sql node version 3.34.8](https://github.com/tradecontrol/sqlnode/releases)

- Data.NodeContext.CompanyName() generating an error prior to initialisation

## 1.1.3

[Sql node version 3.34.8](https://github.com/tradecontrol/sqlnode/releases)

Some web hosting services do not support Sql Server security. This upgrade protects mail hosting service credentials by [encrypting passwords](../src/TCWeb/Mail/Encrypt.cs) in the data store. Because the app is Open Source, you need to change [the key and vector](../src/TCWeb/Data/NodeSettings.cs) bytes prior to compilation.

## 1.1.2

[Sql node version 3.34.8](https://github.com/tradecontrol/sqlnode/releases)

- Fix Web.Mail.MailInvoice.Send() - not filtering by selected email address
- Mobile event log delete button permissions

## 1.1.1

[Sql node version 3.34.7](https://github.com/tradecontrol/sqlnode/releases)

- Losses carried forward on Company Tax
- Corporation Tax totals on P&L
- Direct invoice deletion



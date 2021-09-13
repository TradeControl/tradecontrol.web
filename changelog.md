# Change Log

Changes to 1.1.0, released 1 July 2021.

## 1.1.1

[Sql node version 3.34.7](https://github.com/tradecontrol/sqlnode/releases)

- Losses carried forward on Company Tax
- Corporation Tax totals on P&L
- Direct invoice deletion

## 1.1.2

[Sql node version 3.34.8](https://github.com/tradecontrol/sqlnode/releases)

- Fix Web.Mail.MailInvoice.Send() - not filtering by selected email address
- Mobile event log delete button permissions

## 1.1.3

[Sql node version 3.34.8](https://github.com/tradecontrol/sqlnode/releases)

Some web hosting services do not support Sql Server security. This upgrade protects mail hosting service credentials by [encrypting passwords](src/TCWeb/Mail/Encrypt.cs) in the data store. Because the app is Open Source, you need to change [the key and vector](src/TCWeb/Data/NodeSettings.cs) bytes prior to compilation.

## 1.1.4

[Sql node version 3.34.8](https://github.com/tradecontrol/sqlnode/releases)

- Data.NodeContext.CompanyName() generating an error prior to initialisation


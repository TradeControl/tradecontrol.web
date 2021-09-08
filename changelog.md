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

``` csharp
/// <summary>
/// Modify key bytes to protect passwords in an unsecured Sql Server context
/// </summary>
public static byte[] SymmetricKey
{
    get
    {
        byte[] key = { 0x22, 0x5C, 0x53, 0x4B, 0x44, 0x2D, 0x6B, 0x6D, 0x51, 0xC, 0x58, 0x69, 0x4C, 0x56, 0x72, 0x15 };
        return key;
    }
}
```




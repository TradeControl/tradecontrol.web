# Categories and Cash Codes

Temporary markdown for Copilot knowledge base.

## Cash Polarity

Central to the Trade Control node is the function of cash polarity. For any given transaction, when quantity is positive, cash must be negative and vice versa. Therefore, if you know the polarity of the cash, you know the direction of the goods or service. That is why Trade Control does not have native customers and suppliers, sales and purchase orders or invoices. By assigning polarity to the transaction automatically identifies owner and document type.

The polarity of the money is called the _Cash Polarity_. The three polarities are incoming, outgoing or neutral. The following table shows a few ways in which this function can model traditional commercial entities:

Organisation | Polarity
-- | --
Customer | POS
Prospect | POS
Supplier | NEG
Government | OFF

Task | Polarity
-- | --
Sales Order | POS
Purchase Order | NEG
Works Order | -
Project | -

> By not assigning cash polarity to a task, it becomes an action if inside a workflow or a project if outside (as the parent).

Demand | Demand Invoice | Demand Payment | Mirror | Mirror Invoice | Mirror Payment
-- | -- | -- | -- | -- | --
Sales Invoice | POS | POS | Purchase Invoice | NEG | NEG
Debit Note | NEG | POS | Credit Note | POS | NEG


### Advantages

Using polarity, the node can simply switch the polarity to turn an input into an output. In so doing, nodes can be easily connected together into a **trading network** of supply-chains. Furthermore, simply adding up live tasks obtains the order book profit or adding up the entire payment entries yields the current balance. That means data integrity can be confirmed on a transaction-grained basis, as well as **business intelligence**being easily obtainable and aggregated.

## Categories and Cash Codes

Every cash transaction is associated with a Cash Code, describing what it is, how it fits into the business, its tax status and cash polarity.  However, in Trade Control, Cash Codes are not simply a list; they can form sets, called Categories, that are arranged in hierarchies of interdependence. 

Categories can be either:

- A set of Cash Codes of the same type and polarity.
- A collection of other Categories.
- An Expression that yields a value when applied to its associated transactions. 

Because categories can contain other categories, the structure is recursive, and the potential hierarchies are unbound. 

### Types

Categories define the type and polarity of its Cash Code members. Category type is either:

-	**TRADE**: the company balance is affected through an act of exchange
-	**MONEY**: for financial transactions that do not affect the overall balance
-	**EXTERNAL**: for taxes that affect the balance without a corresponding exchange

Tax Types are defined in the Tax page of the Administrator. There are three types, each with a corresponding tax rate: vat, income tax and general (e.g. corporation tax).  You can define as many tax rates as you need, but it is important to use the correct type when assigning them to transactions. A salary payment classified incorrectly as zero vat will turn up in the purchases of your Vat Statement. 

Tax Types are assigned to the Cash Code. For example, you could create a Category called _Sales_ of type TRADE with a positive polarity; then assign several Cash Codes with different default tax rates of types vat, such as Home, EU and Non-EU exports.

## Cash Totals

You can add any kind of Cash Total, but Net Profit is a legal obligation due to its role in calculating Corporation Tax. Vat also is required because the Vat Statement must exclude costs, like salaries, from your return to the government. The Basic Setup demonstrates how this is achieved. 

### Net Profit

In the Cash Total page of Definitions, Net and Gross Profit are defined. From the + button, the Gross Profit consists of one positive polarity category and two negatives. These categories are linked to Cash Codes, a few of which are shown in the diagram below. The Net Profit, however, has only two categories: Indirect Costs and the Gross Profit. The resulting hierarchy of Cash Codes is assigned to the Net Profit category in the main page of the Administrator.


To obtain the Net and Gross Profit, Trade Control firstly recurses over the hierarchy to find the set of Cash Codes it contains. It then gets all the transactions assigned to them; and because income is positive and expenditure is negative, it only needs to add them up to get the profit. In this way, since Corporation Tax is just a percentage of Net Profit, Trade Control can also calculate that tax for thousands of transactions almost instantaneously. 

### Vat

Specifying the Cash Codes for Vat should be straight forward. The Basic Setup adds the Category Codes for Sales, Direct and Indirect Cost. The total is then assigned to the Vat Statement in the options section of the Administrator main page.  It should be noted that the vat due is obtained by applying the transactions tax type. The results are then filtered by applying the hierarchy of Cash Codes specified in the Vat Total, resulting in the Vat Statement.

## Cash Expressions

Because there are no hard-coded values for profits, purchases and sales etc, information such as percentage profit can be provided by specifying a Cash Expression. A valid expression is any statement that conforms to an Excel formula, where ranges are replaced with the names of Category Codes. The Basic Setup provides a few examples, such as the percentage Gross Profit: 

``` IF([Sales]=0,0,([Gross Profit]/[Sales])) ```

The format can be any specifier that is recognised by Excel. In this case **0%** expresses the result as a percentage. The snapshot of the VSTO Cash Flow statement shows how both the Cash Totals and Expressions are rendered in the spreadsheet. The formula bar contains an example of a Gross Profit % cell derived from the above expression. The yellow column represents the current period, so you can see that your expressions can also apply to future accruals as well. 




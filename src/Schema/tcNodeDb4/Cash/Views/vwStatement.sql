CREATE VIEW Cash.vwStatement
AS
	WITH statement_base AS
	(
		SELECT ROW_NUMBER() OVER(ORDER BY TransactOn, CashEntryTypeCode DESC) AS RowNumber,
		 SubjectCode, EntryDescription, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut 
		 FROM Cash.vwStatementBase
	), opening_balance AS
	(	
		SELECT SUM( Subject.tbAccount.CurrentBalance) AS OpeningBalance
		FROM         Subject.tbAccount INNER JOIN
							  Cash.tbCode ON Subject.tbAccount.CashCode = Cash.tbCode.CashCode
		WHERE     ( Subject.tbAccount.AccountClosed = 0) AND (Subject.tbAccount.AccountTypeCode = 0)
	), statement_data AS
	(
		SELECT 
			0 AS RowNumber,
			(SELECT TOP (1) SubjectCode FROM App.tbOptions) AS SubjectCode,
			NULL AS EntryDescription,
			NULL AS TransactOn,    
			(SELECT CAST(Message AS NVARCHAR) FROM App.tbText WHERE TextId = 3013) AS ReferenceCode,	
			1 AS CashEntryTypeCode,
			PayIn = (SELECT OpeningBalance FROM opening_balance),
			0 AS PayOut
		UNION 
		SELECT RowNumber, SubjectCode, EntryDescription, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut 
		FROM statement_base
	), company_statement AS
	(
		SELECT RowNumber, SubjectCode, EntryDescription, TransactOn, ReferenceCode, CashEntryTypeCode, PayIn, PayOut,
			SUM(PayIn + (PayOut * -1)) OVER (ORDER BY RowNumber ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS Balance
		FROM statement_data
	)
	SELECT RowNumber, cs.SubjectCode, Subject.SubjectName, cs.EntryDescription,
			TransactOn, ReferenceCode, cs.CashEntryTypeCode, et.CashEntryType, CAST(PayIn AS decimal(18, 5)) PayIn, CAST(PayOut AS decimal(18, 5)) PayOut, CAST(Balance AS decimal(18, 5)) Balance
	FROM company_statement cs 
		JOIN Subject.tbSubject Subject ON cs.SubjectCode = Subject.SubjectCode
		JOIN Cash.tbEntryType et ON cs.CashEntryTypeCode = et.CashEntryTypeCode;

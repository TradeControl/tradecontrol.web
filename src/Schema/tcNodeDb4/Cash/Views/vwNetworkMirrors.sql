CREATE   VIEW Cash.vwNetworkMirrors
AS
	SELECT SubjectCode, CashCode, ChargeCode, TransmitStatusCode FROM Cash.tbMirror WHERE TransmitStatusCode BETWEEN 1 AND 2;

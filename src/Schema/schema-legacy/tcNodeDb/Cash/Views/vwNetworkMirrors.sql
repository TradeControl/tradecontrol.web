CREATE   VIEW Cash.vwNetworkMirrors
AS
	SELECT AccountCode, CashCode, ChargeCode, TransmitStatusCode FROM Cash.tbMirror WHERE TransmitStatusCode BETWEEN 1 AND 2;

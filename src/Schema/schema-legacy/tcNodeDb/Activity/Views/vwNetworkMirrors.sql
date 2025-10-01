CREATE   VIEW Activity.vwNetworkMirrors
AS
	SELECT AccountCode, ActivityCode, AllocationCode, TransmitStatusCode FROM Activity.tbMirror WHERE TransmitStatusCode BETWEEN 1 AND 2;

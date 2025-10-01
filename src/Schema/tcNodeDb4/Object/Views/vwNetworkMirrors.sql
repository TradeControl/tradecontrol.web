CREATE   VIEW Object.vwNetworkMirrors
AS
	SELECT SubjectCode, ObjectCode, AllocationCode, TransmitStatusCode FROM Object.tbMirror WHERE TransmitStatusCode BETWEEN 1 AND 2;

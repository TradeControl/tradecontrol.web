CREATE   PROCEDURE Invoice.proc_PostEntries
AS
	DECLARE @UserId nvarchar(10) = (SELECT UserId FROM Usr.vwCredentials);
	EXECUTE Invoice.proc_PostEntriesById @UserId;

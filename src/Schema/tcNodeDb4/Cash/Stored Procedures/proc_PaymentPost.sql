CREATE PROCEDURE Cash.proc_PaymentPost
AS
	DECLARE @UserId nvarchar(10) = (SELECT UserId FROM Usr.vwCredentials);
	EXECUTE Cash.proc_PaymentPostById @UserId;

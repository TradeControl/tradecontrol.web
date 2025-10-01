CREATE   VIEW Task.vwCostSet
AS
	SELECT TaskCode, UserId, InsertedBy, InsertedOn, RowVer
	FROM Task.tbCostSet
	WHERE (UserId = (SELECT UserId FROM Usr.vwCredentials));

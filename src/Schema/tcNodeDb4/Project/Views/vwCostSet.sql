CREATE   VIEW Project.vwCostSet
AS
	SELECT ProjectCode, UserId, InsertedBy, InsertedOn, RowVer
	FROM Project.tbCostSet
	WHERE (UserId = (SELECT UserId FROM Usr.vwCredentials));

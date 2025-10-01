CREATE   VIEW Usr.vwCredentials
  AS
SELECT     UserId, UserName, LogonName, IsAdministrator
FROM         Usr.tbUser
WHERE     (LogonName = SUSER_SNAME()) AND (IsEnabled <> 0)

CREATE VIEW Usr.vwUserMenus
AS
	SELECT Usr.tbMenuUser.MenuId, Usr.tbMenu.InterfaceCode
	FROM Usr.vwCredentials 
		JOIN Usr.tbMenuUser ON Usr.vwCredentials.UserId = Usr.tbMenuUser.UserId
		JOIN Usr.tbMenu ON Usr.tbMenuUser.MenuId = Usr.tbMenu.MenuId;

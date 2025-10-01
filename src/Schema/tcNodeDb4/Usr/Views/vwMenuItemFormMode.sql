
CREATE   VIEW Usr.vwMenuItemFormMode
AS
	SELECT        OpenMode, OpenModeDescription
	FROM            Usr.tbMenuOpenMode
	WHERE        (OpenMode < 2);

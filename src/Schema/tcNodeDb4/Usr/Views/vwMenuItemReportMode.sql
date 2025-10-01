
CREATE   VIEW Usr.vwMenuItemReportMode
AS
	SELECT        OpenMode, OpenModeDescription
	FROM            Usr.tbMenuOpenMode
	WHERE        (OpenMode > 1) AND (OpenMode < 5);

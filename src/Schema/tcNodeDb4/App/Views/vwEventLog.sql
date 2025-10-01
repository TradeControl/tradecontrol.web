
CREATE   VIEW App.vwEventLog
AS
	SELECT        App.tbEventLog.LogCode, App.tbEventLog.LoggedOn, App.tbEventLog.EventTypeCode, App.tbEventType.EventType, App.tbEventLog.EventMessage, App.tbEventLog.InsertedBy, App.tbEventLog.RowVer
	FROM            App.tbEventLog INNER JOIN
							 App.tbEventType ON App.tbEventLog.EventTypeCode = App.tbEventType.EventTypeCode

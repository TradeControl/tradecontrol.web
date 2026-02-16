CREATE   VIEW App.vwHost
AS
	SELECT App.tbHost.HostId, App.tbHost.HostDescription, App.tbHost.EmailAddress, App.tbHost.EmailPassword, App.tbHost.HostName, App.tbHost.HostPort, App.tbHost.IsSmtpAuth
	FROM App.tbOptions 
		JOIN App.tbHost ON App.tbOptions.HostId = App.tbHost.HostId;

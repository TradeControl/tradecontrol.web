CREATE   VIEW dbo.AspNetUserRegistrations
AS
	SELECT asp.Id, asp.UserName EmailAddress, u.UserName,
		asp.EmailConfirmed IsConfirmed, 
		CAST(CASE WHEN u.EmailAddress IS NULL THEN 0 ELSE 1 END as bit) IsRegistered,
		CAST(CASE WHEN 
			(SELECT COUNT(*) FROM AspNetUserRoles 
				JOIN AspNetRoles ON AspNetRoles.Id = AspNetUserRoles.RoleId 
				WHERE AspNetRoles.Name = 'Administrators' AND AspNetUserRoles.UserId = asp.Id) = 0 
		THEN 0 
		ELSE 1 
		END as bit) IsAdministrator,
		CAST(CASE WHEN 
			(SELECT COUNT(*) FROM AspNetUserRoles 
				JOIN AspNetRoles ON AspNetRoles.Id = AspNetUserRoles.RoleId 
				WHERE AspNetRoles.Name = 'Managers' AND AspNetUserRoles.UserId = asp.Id) = 0 
		THEN 0 
		ELSE 1 
		END as bit) IsManager
	FROM AspNetUsers asp
		LEFT OUTER JOIN Usr.tbUser u ON asp.Email = u.EmailAddress;

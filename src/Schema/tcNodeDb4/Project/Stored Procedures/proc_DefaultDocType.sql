
CREATE   PROCEDURE Project.proc_DefaultDocType
	(
		@ProjectCode nvarchar(20),
		@DocTypeCode smallint OUTPUT
	)
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@CashPolarityCode smallint
			, @ProjectStatusCode smallint

		IF EXISTS(SELECT     CashPolarityCode
				  FROM         Project.vwCashPolarity
				  WHERE     (ProjectCode = @ProjectCode))
			SELECT   @CashPolarityCode = CashPolarityCode
			FROM         Project.vwCashPolarity
			WHERE     (ProjectCode = @ProjectCode)			          
		ELSE
			SET @CashPolarityCode = 1

		SELECT  @ProjectStatusCode =ProjectStatusCode
		FROM         Project.tbProject
		WHERE     (ProjectCode = @ProjectCode)		
	
		IF @CashPolarityCode = 0
			SET @DocTypeCode = CASE @ProjectStatusCode WHEN 0 THEN 2 ELSE 3 END								
		ELSE
			SET @DocTypeCode = CASE @ProjectStatusCode WHEN 0 THEN 0 ELSE 1 END 
		 
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

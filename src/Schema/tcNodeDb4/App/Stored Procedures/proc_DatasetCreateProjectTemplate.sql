CREATE PROCEDURE App.proc_DatasetCreateProjectTemplate
(
	@ParentProjectCode nvarchar(20) OUTPUT,          -- container; created if null/empty
	@ParentProjectTitle nvarchar(100) = NULL,        -- defaults if container created
	@CustomerSubjectCode nvarchar(10),               -- customer for the sales/service project
	@ObjectCode nvarchar(50),                        -- the FG/service object being sold
	@ActionOn date,                                  -- dead-year date
	@Quantity decimal(18,4) = 100,

	@BoxSupplierSubjectCode nvarchar(10) = NULL,
	@PlasticSupplierSubjectCode nvarchar(10) = NULL,
	@InsertSupplierSubjectCode nvarchar(10) = NULL,
	@MouldingHaulierSubjectCode nvarchar(10) = NULL,
	@PrinterSubjectCode nvarchar(10) = NULL,
	@PrintHaulierSubjectCode nvarchar(10) = NULL,

	@ProjectCode nvarchar(20) OUTPUT                 -- master/template project created
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF NOT EXISTS (SELECT 1 FROM Usr.vwCredentials WHERE IsAdministrator <> 0)
		BEGIN
			DECLARE @Msg nvarchar(100) = CONCAT('Access Denied: User ', SUSER_SNAME(), ' is not an administrsator');
			RAISERROR ('%s', 13, 1, @Msg);
		END

		DECLARE
			@UserId nvarchar(10) = (SELECT UserId FROM Usr.vwCredentials),
			@DefaultSubjectCode nvarchar(10) = (SELECT TOP (1) SubjectCode FROM App.tbOptions),
			@ObjectDescription nvarchar(100),
			@ObjectCashCode nvarchar(50),
			@ObjectUnitCharge decimal(18,7),
			@ObjectPrinted bit;

		SELECT
			@ObjectDescription = o.ObjectDescription,
			@ObjectCashCode = o.CashCode,
			@ObjectUnitCharge = o.UnitCharge,
			@ObjectPrinted = o.Printed
		FROM Object.tbObject o
		WHERE o.ObjectCode = @ObjectCode;

		IF @ObjectDescription IS NULL
			THROW 51030, 'Dataset: @ObjectCode not found in Object.tbObject.', 1;

		BEGIN TRAN;

		-----------------------------------------------------------------
		-- Ensure container parent project
		-----------------------------------------------------------------
		IF ISNULL(@ParentProjectCode, N'') = N''
		BEGIN
			EXEC Project.proc_NextCode N'PROJECT', @ParentProjectCode OUTPUT;

			INSERT INTO Project.tbProject
			(
				ProjectCode,
				UserId,
				SubjectCode,
				ProjectTitle,
				ObjectCode,
				ProjectStatusCode,
				ActionById,
				ActionOn,
				ActionedOn
			)
			VALUES
			(
				@ParentProjectCode,
				@UserId,
				@CustomerSubjectCode,
				ISNULL(@ParentProjectTitle, CONCAT(N'Dataset Container ', @CustomerSubjectCode)),
				N'PROJECT',
				0,
				@UserId,
				@ActionOn,
				@ActionOn
			);
		END

		-----------------------------------------------------------------
		-- Create template/master project for the object
		-----------------------------------------------------------------
		EXEC Project.proc_NextCode @ObjectCode, @ProjectCode OUTPUT;

		INSERT INTO Project.tbProject
		(
			ProjectCode,
			UserId,
			SubjectCode,
			ProjectTitle,
			ContactName,
			ObjectCode,
			ProjectStatusCode,
			ActionById,
			ActionOn,
			ActionedOn,
			ProjectNotes,
			Quantity,
			CashCode,
			TaxCode,
			UnitCharge,
			TotalCharge,
			AddressCodeFrom,
			AddressCodeTo,
			Printed
		)
		SELECT
			@ProjectCode,
			@UserId,
			@CustomerSubjectCode,
			@ObjectDescription,
			(SELECT UserName FROM Usr.vwCredentials),
			@ObjectCode,
			1,
			@UserId,
			@ActionOn,
			@ActionOn,
			@ObjectDescription,
			@Quantity,
			@ObjectCashCode,
			s.TaxCode,
			@ObjectUnitCharge,
			@ObjectUnitCharge * @Quantity,
			s.AddressCode,
			s.AddressCode,
			CASE WHEN @ObjectPrinted = 0 THEN 1 ELSE 0 END
		FROM Subject.tbSubject s
		WHERE s.SubjectCode = @CustomerSubjectCode;

		EXEC Project.proc_Configure @ProjectCode;
		EXEC Project.proc_AssignToParent @ProjectCode, @ParentProjectCode;

		-----------------------------------------------------------------
		-- Normalize suppliers for exploded child projects (dataset object codes)
		-----------------------------------------------------------------
		-- Boxes + pallets (purchase orders)
		IF @BoxSupplierSubjectCode IS NOT NULL
		BEGIN
			UPDATE p
			SET
				p.SubjectCode = @BoxSupplierSubjectCode,
				p.AddressCodeFrom = s.AddressCode,
				p.AddressCodeTo = s.AddressCode
			FROM Project.tbProject p
			JOIN Subject.tbSubject s ON s.SubjectCode = @BoxSupplierSubjectCode
			WHERE p.ProjectTitle = (SELECT ProjectTitle FROM Project.tbProject WHERE ProjectCode = @ProjectCode)
			  AND p.ObjectCode LIKE N'DS/PRD/CMP/BOX/%';
		END

		-- Plastic material
		IF @PlasticSupplierSubjectCode IS NOT NULL
		BEGIN
			UPDATE p
			SET
				p.SubjectCode = @PlasticSupplierSubjectCode,
				p.AddressCodeFrom = s.AddressCode,
				p.AddressCodeTo = s.AddressCode
			FROM Project.tbProject p
			JOIN Subject.tbSubject s ON s.SubjectCode = @PlasticSupplierSubjectCode
			WHERE p.ProjectTitle = (SELECT ProjectTitle FROM Project.tbProject WHERE ProjectCode = @ProjectCode)
			  AND p.ObjectCode LIKE N'DS/PRD/MTR/%';
		END

		-- Inserts
		IF @InsertSupplierSubjectCode IS NOT NULL
		BEGIN
			UPDATE p
			SET
				p.SubjectCode = @InsertSupplierSubjectCode,
				p.AddressCodeFrom = s.AddressCode,
				p.AddressCodeTo = s.AddressCode
			FROM Project.tbProject p
			JOIN Subject.tbSubject s ON s.SubjectCode = @InsertSupplierSubjectCode
			WHERE p.ProjectTitle = (SELECT ProjectTitle FROM Project.tbProject WHERE ProjectCode = @ProjectCode)
			  AND p.ObjectCode LIKE N'DS/PRD/CMP/INS/%';
		END

		-- Delivery: choose print vs moulding haulier based on object family
		IF @ObjectCode LIKE N'DS/PRD/FG/%'
		BEGIN
			IF @MouldingHaulierSubjectCode IS NOT NULL
			BEGIN
				UPDATE p
				SET
					p.SubjectCode = @MouldingHaulierSubjectCode,
					p.AddressCodeFrom = (SELECT TOP (1) AddressCode FROM Subject.tbSubject WHERE SubjectCode = @DefaultSubjectCode),
					p.AddressCodeTo = (SELECT TOP (1) AddressCode FROM Subject.tbSubject WHERE SubjectCode = @CustomerSubjectCode)
				FROM Project.tbProject p
				WHERE p.ProjectTitle = (SELECT ProjectTitle FROM Project.tbProject WHERE ProjectCode = @ProjectCode)
				  AND p.ObjectCode LIKE N'DS/SRV/SHP/DELIVERY/%';
			END
		END
		ELSE
		BEGIN
			IF @PrintHaulierSubjectCode IS NOT NULL
			BEGIN
				UPDATE p
				SET
					p.SubjectCode = @PrintHaulierSubjectCode,
					p.AddressCodeFrom = (SELECT TOP (1) AddressCode FROM Subject.tbSubject WHERE SubjectCode = @DefaultSubjectCode),
					p.AddressCodeTo = (SELECT TOP (1) AddressCode FROM Subject.tbSubject WHERE SubjectCode = @CustomerSubjectCode)
				FROM Project.tbProject p
				WHERE p.ProjectTitle = (SELECT ProjectTitle FROM Project.tbProject WHERE ProjectCode = @ProjectCode)
				  AND p.ObjectCode LIKE N'DS/SRV/SHP/DELIVERY/%';
			END
		END

		-- Ensure internal works orders stay on node subject (as DemoBom does)
		UPDATE p
		SET
			p.SubjectCode = @DefaultSubjectCode,
			p.AddressCodeFrom = s.AddressCode,
			p.AddressCodeTo = s.AddressCode
		FROM Project.tbProject p
		JOIN Subject.tbSubject s ON s.SubjectCode = @DefaultSubjectCode
		WHERE p.ProjectTitle = (SELECT ProjectTitle FROM Project.tbProject WHERE ProjectCode = @ProjectCode)
		  AND p.CashCode IS NULL
		  AND p.SubjectCode <> @BoxSupplierSubjectCode;

		EXEC Project.proc_Schedule @ProjectCode;

		COMMIT TRAN;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRAN;
		EXEC App.proc_ErrorLog;
	END CATCH
GO

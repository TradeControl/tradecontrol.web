CREATE VIEW Activity.vwUnMirrored
AS
	WITH candidates AS
	(
		SELECT DISTINCT Task.tbAllocation.AccountCode, Org.tbOrg.AccountName, Task.tbAllocation.AllocationCode, Task.tbAllocation.AllocationDescription, Task.tbAllocation.CashModeCode, Cash.tbMode.CashMode, Task.tbAllocation.UnitCharge, Task.tbAllocation.UnitOfMeasure
		FROM            Task.tbAllocation 
			INNER JOIN Cash.tbMode ON Task.tbAllocation.CashModeCode = Cash.tbMode.CashModeCode 
			INNER JOIN Org.tbOrg ON Task.tbAllocation.AccountCode = Org.tbOrg.AccountCode 
			LEFT OUTER JOIN Activity.tbMirror ON Task.tbAllocation.AccountCode = Activity.tbMirror.AccountCode AND Task.tbAllocation.AllocationCode = Activity.tbMirror.AllocationCode
		WHERE        (Activity.tbMirror.ActivityCode IS NULL)
	)
	SELECT CAST(ROW_NUMBER() OVER (ORDER BY AccountCode, AllocationCode) AS int) CandidateId,
		candidates.AccountCode, candidates.AccountName, candidates.AllocationCode, candidates.AllocationDescription, candidates.CashModeCode, candidates.CashMode, candidates.UnitCharge, candidates.UnitOfMeasure,
		CASE WHEN act_code.ActivityCode IS NULL THEN 0 ELSE 1 END IsActivity
	FROM candidates LEFT OUTER JOIN Activity.tbActivity act_code ON candidates.AllocationCode = act_code.ActivityCode;

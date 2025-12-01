
CREATE   VIEW Project.vwOpBucket
AS
SELECT        op.ProjectCode, op.OperationNumber, op.EndOn, buckets.Period, buckets.BucketId
FROM            Project.tbOp AS op CROSS APPLY
			(	SELECT  buckets.Period, buckets.BucketId
				FROM        App.fnBuckets(CURRENT_TIMESTAMP) buckets 
				WHERE     (StartDate <= op.EndOn) AND (EndDate > op.EndOn)) AS buckets

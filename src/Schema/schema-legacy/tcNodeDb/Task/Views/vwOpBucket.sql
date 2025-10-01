
CREATE   VIEW Task.vwOpBucket
AS
SELECT        op.TaskCode, op.OperationNumber, op.EndOn, buckets.Period, buckets.BucketId
FROM            Task.tbOp AS op CROSS APPLY
			(	SELECT  buckets.Period, buckets.BucketId
				FROM        App.fnBuckets(CURRENT_TIMESTAMP) buckets 
				WHERE     (StartDate <= op.EndOn) AND (EndDate > op.EndOn)) AS buckets

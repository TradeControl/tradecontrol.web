CREATE   VIEW Task.vwBucket
AS
SELECT        task.TaskCode, task.ActionOn, buckets.Period, buckets.BucketId
FROM            Task.tbTask AS task CROSS APPLY
			(	SELECT  buckets.Period, buckets.BucketId
				FROM        App.fnBuckets(CURRENT_TIMESTAMP) buckets 
				WHERE     (StartDate <= task.ActionOn) AND (EndDate > task.ActionOn)) AS buckets

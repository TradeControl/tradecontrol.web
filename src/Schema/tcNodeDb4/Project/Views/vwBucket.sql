CREATE   VIEW Project.vwBucket
AS
SELECT        Project.ProjectCode, Project.ActionOn, buckets.Period, buckets.BucketId
FROM            Project.tbProject AS Project CROSS APPLY
			(	SELECT  buckets.Period, buckets.BucketId
				FROM        App.fnBuckets(CURRENT_TIMESTAMP) buckets 
				WHERE     (StartDate <= Project.ActionOn) AND (EndDate > Project.ActionOn)) AS buckets

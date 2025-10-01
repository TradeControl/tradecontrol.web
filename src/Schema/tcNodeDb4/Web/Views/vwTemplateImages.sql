CREATE   VIEW Web.vwTemplateImages
AS
	SELECT Web.tbTemplateImage.TemplateId, Web.tbTemplate.TemplateFileName, Web.tbTemplateImage.ImageTag, Web.tbImage.ImageFileName
	FROM Web.tbTemplateImage 
		JOIN Web.tbTemplate ON Web.tbTemplateImage.TemplateId = Web.tbTemplate.TemplateId 
		JOIN Web.tbImage ON Web.tbTemplateImage.ImageTag = Web.tbImage.ImageTag;

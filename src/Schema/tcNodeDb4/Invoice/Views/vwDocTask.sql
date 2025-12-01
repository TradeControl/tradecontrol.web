CREATE VIEW Invoice.vwDocProject
AS
SELECT        tbProjectInvoice.InvoiceNumber, tbProjectInvoice.ProjectCode, Project.tbProject.ProjectTitle, Project.tbProject.ObjectCode, tbProjectInvoice.CashCode, Cash.tbCode.CashDescription, Project.tbProject.ActionedOn, tbProjectInvoice.Quantity, 
                         Object.tbObject.UnitOfMeasure, tbProjectInvoice.InvoiceValue, tbProjectInvoice.TaxValue, tbProjectInvoice.TaxCode, Project.tbProject.SecondReference
FROM            Invoice.tbProject AS tbProjectInvoice INNER JOIN
                         Project.tbProject ON tbProjectInvoice.ProjectCode = Project.tbProject.ProjectCode AND tbProjectInvoice.ProjectCode = Project.tbProject.ProjectCode INNER JOIN
                         Cash.tbCode ON tbProjectInvoice.CashCode = Cash.tbCode.CashCode INNER JOIN
                         Object.tbObject ON Project.tbProject.ObjectCode = Object.tbObject.ObjectCode

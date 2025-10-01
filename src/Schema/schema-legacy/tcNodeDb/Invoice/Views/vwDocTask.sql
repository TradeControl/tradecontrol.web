CREATE VIEW Invoice.vwDocTask
AS
SELECT        tbTaskInvoice.InvoiceNumber, tbTaskInvoice.TaskCode, Task.tbTask.TaskTitle, Task.tbTask.ActivityCode, tbTaskInvoice.CashCode, Cash.tbCode.CashDescription, Task.tbTask.ActionedOn, tbTaskInvoice.Quantity, 
                         Activity.tbActivity.UnitOfMeasure, tbTaskInvoice.InvoiceValue, tbTaskInvoice.TaxValue, tbTaskInvoice.TaxCode, Task.tbTask.SecondReference
FROM            Invoice.tbTask AS tbTaskInvoice INNER JOIN
                         Task.tbTask ON tbTaskInvoice.TaskCode = Task.tbTask.TaskCode AND tbTaskInvoice.TaskCode = Task.tbTask.TaskCode INNER JOIN
                         Cash.tbCode ON tbTaskInvoice.CashCode = Cash.tbCode.CashCode INNER JOIN
                         Activity.tbActivity ON Task.tbTask.ActivityCode = Activity.tbActivity.ActivityCode

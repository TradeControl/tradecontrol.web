CREATE TABLE [Cash].[tbPaymentStatus] (
    [PaymentStatusCode] SMALLINT      NOT NULL,
    [PaymentStatus]     NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_Cash_tbPaymentStatus] PRIMARY KEY CLUSTERED ([PaymentStatusCode] ASC)
);


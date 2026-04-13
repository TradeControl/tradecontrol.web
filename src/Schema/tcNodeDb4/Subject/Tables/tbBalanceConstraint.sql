CREATE TABLE [Subject].[tbBalanceConstraint]
(
    [BalanceConstraintCode] TINYINT NOT NULL,
    [BalanceConstraint] NVARCHAR(50) NOT NULL,
    CONSTRAINT [PK_Subject_tbBalanceConstraint] PRIMARY KEY CLUSTERED ([BalanceConstraintCode] ASC)
);

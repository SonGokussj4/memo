-----------------------------------------------------------------------------------------
-- VYTVORIT TABULKU Contracts
IF OBJECT_ID('memo.Contracts', 'U') IS NOT NULL
  DROP TABLE [memo].[Contracts]
GO
-----------------------------------------------------------------------------------------
CREATE TABLE [memo].[Contracts]
(
    [ContractsId] int PRIMARY KEY IDENTITY(1, 1),
    [ReceiveDate] date NOT NULL,
    [Subject] nvarchar(max),
    [EveDivision] nvarchar(50) NOT NULL,
    [EveDepartment] nvarchar(50) NOT NULL,
    [EveCreatedUser] nvarchar(50) NOT NULL,
    [ContactId] int NOT NULL,
    [CompanyId] int NOT NULL,
    [Price] int,
    [PriceCzk] int,
    [CurrencyId] int,
    [ExchangeRate] decimal(18,3),
    [Notes] nvarchar(max),
    [Active] bit DEFAULT 1,
    [CreatedBy] nvarchar(50),
    [ModifiedBy] nvarchar(50),
    [CreatedDate] datetime,
    [ModifiedDate] datetime,
    CONSTRAINT [FK__memo.Contracts__memo.Contact__ContactId]
        FOREIGN KEY ([ContactId])
        REFERENCES [memo].[Contact] ([ContactId]),
    CONSTRAINT [FK__memo.Contracts__memo.Company__CompanyId]
        FOREIGN KEY ([CompanyId])
        REFERENCES [memo].[Company] ([CompanyId]),
    CONSTRAINT [FK__memo.Contracts__memo.Currency__CurrencyId]
        FOREIGN KEY ([CurrencyId])
        REFERENCES [memo].[Currency] ([CurrencyId])
    -- ON DELETE CASCADE
)
GO
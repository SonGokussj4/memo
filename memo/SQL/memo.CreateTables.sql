-- This sql query should create all tables and it's relations

-- Drop the table 'Company' in schema 'Memo'
IF EXISTS (
    SELECT *
        FROM sys.tables
        JOIN sys.schemas
            ON sys.tables.schema_id = sys.schemas.schema_id
    WHERE sys.schemas.name = N'memo'
        -- AND sys.tables.name = N'Company'
)
    DROP TABLE "memo.Company","memo.Contact","memo.Offer","memo.OfferStatus","memo.Order","memo.Currency"
GO

CREATE TABLE [memo].[Company] (
  [CompanyId] int PRIMARY KEY IDENTITY(1, 1),
  [Name] nvarchar(50),
  [City] nvarchar(50),
  [Address] nvarchar(50),
  [Phone] nvarchar(50),
  [Web] nvarchar(50),
  [CreateDate] date
)
GO

CREATE TABLE [memo].[Contact] (
  [ContactId] int PRIMARY KEY IDENTITY(1, 1),
  [PersonName] nvarchar(50),
  [Department] nvarchar(50),
  [Phone] nvarchar(50),
  [Email] Email,
  [CreateDate] date
)
GO

CREATE TABLE [memo].[Offer] (
  [OfferId] int PRIMARY KEY IDENTITY(1, 1),
  [OfferName] nvarchar(50),
  [ReceiveDate] date,
  [SentDate] date,
  [Subject] text,
  [ContactId] int,
  [CompanyId] int,
  [EveDivision] nvarchar(50) NOT NULL CHECK ([EveDivision] IN ('AD', 'ED')),
  [EveDepartment] nvarchar(50),
  [EveCreatedUser] nvarchar(50),
  [Price] int,
  [Currency] int,
  [ExchangeRate] float,
  [PriceCzk] int,
  [Status] int,
  [LostReason] text,
  [CreateDate] date
)
GO

CREATE TABLE [memo].[OfferStatus] (
  [OfferStatusId] int PRIMARY KEY IDENTITY(1, 1),
  [Status] string
)
GO

CREATE TABLE [memo].[Order] (
  [OrderId] int PRIMARY KEY IDENTITY(1, 1),
  [OfferId] int,
  [OrderName] nvarchar(50),
  [PriceFinal] int,
  [PriceDiscount] int,
  [OrderCode] nvarchar(50),
  [ContactId] int,
  [HourWage] float,
  [TotalHours] int,
  [InvoiceIssueDate] date,
  [InvoiceDueDate] date,
  [ExchangeRate] float,
  [PriceFinalCzk] int,
  [Notes] text,
  [CreateDate] date
)
GO

CREATE TABLE [memo].[Currency] (
  [CurrencyId] int PRIMARY KEY IDENTITY(1, 1),
  [Name] string
)
GO

ALTER TABLE [Offer] ADD FOREIGN KEY ([ContactId]) REFERENCES [Contact] ([ContactId])
GO

ALTER TABLE [Offer] ADD FOREIGN KEY ([CompanyId]) REFERENCES [Company] ([CompanyId])
GO

ALTER TABLE [Offer] ADD FOREIGN KEY ([Status]) REFERENCES [OfferStatus] ([OfferStatusId])
GO

ALTER TABLE [Offer] ADD FOREIGN KEY ([OfferId]) REFERENCES [Order] ([OfferId])
GO

ALTER TABLE [Order] ADD FOREIGN KEY ([ContactId]) REFERENCES [Contact] ([ContactId])
GO

ALTER TABLE [Currency] ADD FOREIGN KEY ([CurrencyId]) REFERENCES [Offer] ([Currency])
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'EVE_qui_2020_003_SKODA_EKX_',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Offer',
@level2type = N'Column', @level2name = 'OfferName';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Ongoing, Won, Lost',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'OfferStatus',
@level2type = N'Column', @level2name = 'Status';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'EVE-Quo/2020-003',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Order',
@level2type = N'Column', @level2name = 'OrderName';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'CZK, EUR, USD',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'Currency',
@level2type = N'Column', @level2name = 'Name';
GO

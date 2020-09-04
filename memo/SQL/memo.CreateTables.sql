-- This sql query should create all tables and it's relations

CREATE TABLE [memo].[Company] (
  [CompanyId] int PRIMARY KEY IDENTITY(1, 1),
  [Name] nvarchar(50),
  [City] nvarchar(50),
  [Address] nvarchar(50),
  [Phone] nvarchar(50),
  [Web] nvarchar(50),
  [CreateDate] date DEFAULT GETDATE(),
  [InvoiceDueDays] int,
  [Notes] ntext,
  [Active] bit DEFAULT 1
)
GO

CREATE TABLE [memo].[Contact] (
  [ContactId] int PRIMARY KEY IDENTITY(1, 1),
  [PersonName] nvarchar(50),
  [PersonLastName] nvarchar(50),
  [PersonTitle] nvarchar(20),
  [CompanyId] int DEFAULT 1,
  [Department] nvarchar(50),
  [Phone] nvarchar(50),
  [Email] nvarchar(255),
  [Notes] ntext,
  [CreateDate] date DEFAULT GETDATE(),
  [Active] bit DEFAULT 1
)
GO

CREATE TABLE [memo].[Offer] (
  [OfferId] int PRIMARY KEY IDENTITY(1, 1),
  [OfferName] nvarchar(50) UNIQUE,
  [ReceiveDate] date,
  [SentDate] date,
  [Subject] ntext,
  [ContactId] int,
  [CompanyId] int,
  [EveDivision] nvarchar(50) NOT NULL CHECK ([EveDivision] IN ('AD', 'ED')),
  [EveDepartment] nvarchar(50),
  [EveCreatedUser] nvarchar(50),
  [Price] int,
  [CurrencyId] int,
  [ExchangeRate] decimal(18,3),
  [PriceCzk] int,
  [OfferStatusId] int,
  [Notes] ntext,
  [LostReason] ntext,
  [CreateDate] date DEFAULT GETDATE(),
  [Active] bit DEFAULT 1
)
GO

--ALTER TABLE [memo].[Offer]
--  ADD CONSTRAINT UQ_OfferName UNIQUE (OfferName);

CREATE TABLE [memo].[OfferStatus] (
  [OfferStatusId] int PRIMARY KEY IDENTITY(1, 1),
  [Name] nvarchar(20)
)
GO

CREATE TABLE [memo].[Order] (
  [OrderId] int PRIMARY KEY IDENTITY(1, 1),
  [OfferId] int,
  [OrderName] nvarchar(50) UNIQUE,
  [PriceFinal] int,
  [PriceDiscount] int,
  [OrderCode] nvarchar(50),
  [ContactId] int,
  [EveContactName] nvarchar(50),
  [BillOfDelivery] nvarchar(255),
  [HourWage] float,
  [TotalHours] int,
  [InvoiceIssueDate] date,
  [InvoiceDueDate] date,
  [ExchangeRate] decimal(18,3),
  [PriceFinalCzk] int,
  [Notes] ntext,
  [CreateDate] date DEFAULT GETDATE(),
  [OtherCosts] int DEFAULT 0,
  [Active] bit DEFAULT 1
)
GO


--ALTER TABLE [memo].[Order]
--  ADD CONSTRAINT UQ_OrderName UNIQUE (OrderName);

CREATE TABLE [memo].[Currency] (
  [CurrencyId] int PRIMARY KEY IDENTITY(1, 1),
  [Name] nvarchar(10),
  [CultureCode] nvarchar(10)
)
GO

ALTER TABLE [memo].[Offer] ADD FOREIGN KEY ([ContactId]) REFERENCES [memo].[Contact] ([ContactId])
GO

ALTER TABLE [memo].[Offer] ADD FOREIGN KEY ([CompanyId]) REFERENCES [memo].[Company] ([CompanyId])
GO

ALTER TABLE [memo].[Offer] ADD FOREIGN KEY ([OfferStatusId]) REFERENCES [memo].[OfferStatus] ([OfferStatusId])
GO

ALTER TABLE [memo].[Offer] ADD FOREIGN KEY ([CurrencyId]) REFERENCES [memo].[Currency] ([CurrencyId])
GO

-- ALTER TABLE [memo].[Order] ADD FOREIGN KEY ([OfferId]) REFERENCES [memo].[Offer] ([OfferId])
-- GO

ALTER TABLE [memo].[Order] ADD FOREIGN KEY ([ContactId]) REFERENCES [memo].[Contact] ([ContactId])
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'EVE_qui_2020_003_SKODA_EKX_',
@level0type = N'Schema', @level0name = 'memo',
@level1type = N'Table',  @level1name = 'Offer',
@level2type = N'Column', @level2name = 'OfferName';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Ongoing, Won, Lost',
@level0type = N'Schema', @level0name = 'memo',
@level1type = N'Table',  @level1name = 'OfferStatus',
@level2type = N'Column', @level2name = 'Status';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'EVE-Quo/2020-003',
@level0type = N'Schema', @level0name = 'memo',
@level1type = N'Table',  @level1name = 'Order',
@level2type = N'Column', @level2name = 'OrderName';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'CZK, EUR, USD',
@level0type = N'Schema', @level0name = 'memo',
@level1type = N'Table',  @level1name = 'Currency',
@level2type = N'Column', @level2name = 'Name';
GO

-- ALTER TABLE [memo].[Company] ADD CONSTRAINT DF_Company DEFAULT GETDATE() FOR CreateDate
-- GO

-- ALTER TABLE [memo].[Contact] ADD CONSTRAINT DF_Contact DEFAULT GETDATE() FOR CreateDate
-- GO

-- ALTER TABLE [memo].[Offer] ADD CONSTRAINT DF_Offer DEFAULT GETDATE() FOR CreateDate
-- GO

-- ALTER TABLE [memo].[Order] ADD CONSTRAINT DF_Order DEFAULT GETDATE() FOR CreateDate
-- GO


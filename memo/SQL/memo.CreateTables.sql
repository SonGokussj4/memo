-- This sql query should create all tables and it's relations

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-----------------------------------------------------------------------------------------
IF OBJECT_ID('memo.Company', 'U') IS NOT NULL
  DROP TABLE [memo].[Company]
GO
-----------------------------------------------------------------------------------------
CREATE TABLE [memo].[Company] (
  [CompanyId] int PRIMARY KEY IDENTITY(1, 1),
  [Name] nvarchar(50),
  [City] nvarchar(50),
  [Address] nvarchar(50),
  [Phone] nvarchar(50),
  [Web] nvarchar(50),
  [InvoiceDueDays] int,
  [Notes] nvarchar(max),
  [Active] bit DEFAULT 1,
  [CreatedBy] nvarchar(50),
  [ModifiedBy] nvarchar(50),
  [CreatedDate] datetime,
  [ModifiedDate] datetime
)
GO


-----------------------------------------------------------------------------------------
IF OBJECT_ID('memo.Contact', 'U') IS NOT NULL
  DROP TABLE [memo].[Contact]
GO
-----------------------------------------------------------------------------------------
CREATE TABLE [memo].[Contact] (
  [ContactId] int PRIMARY KEY IDENTITY(1, 1),
  [PersonName] nvarchar(50),
  [PersonLastName] nvarchar(50),
  [PersonTitle] nvarchar(20),
  [CompanyId] int DEFAULT 1,
  [Department] nvarchar(50),
  [Phone] nvarchar(50),
  [Email] nvarchar(255),
  [Notes] nvarchar(max),
  [Active] bit DEFAULT 1,
  [CreatedBy] nvarchar(50),
  [ModifiedBy] nvarchar(50),
  [CreatedDate] datetime,
  [ModifiedDate] datetime
)
GO


-----------------------------------------------------------------------------------------
IF OBJECT_ID('memo.OfferStatus', 'U') IS NOT NULL
  DROP TABLE [memo].[OfferStatus]
GO
-----------------------------------------------------------------------------------------
CREATE TABLE [memo].[OfferStatus] (
  [OfferStatusId] int PRIMARY KEY IDENTITY(1, 1),
  [Name] nvarchar(20)
)
GO


-----------------------------------------------------------------------------------------
IF OBJECT_ID('memo.Currency', 'U') IS NOT NULL
  DROP TABLE [memo].[Currency]
GO
-----------------------------------------------------------------------------------------
CREATE TABLE [memo].[Currency] (
  [CurrencyId] int PRIMARY KEY IDENTITY(1, 1),
  [Name] nvarchar(10),
  [CultureCode] nvarchar(10)
)
GO


-----------------------------------------------------------------------------------------
IF OBJECT_ID('memo.Offer', 'U') IS NOT NULL
  DROP TABLE [memo].[Offer]
GO
-----------------------------------------------------------------------------------------
CREATE TABLE [memo].[Offer] (
  [OfferId] int PRIMARY KEY IDENTITY(1, 1),
  [OfferName] nvarchar(50) UNIQUE,
  [ReceiveDate] date,
  [SentDate] date,
  [Subject] nvarchar(max),
  [ContactId] int,
  [CompanyId] int,
  [EveDivision] nvarchar(50),
  [EveDepartment] nvarchar(50),
  [EveCreatedUser] nvarchar(50),
  [Price] int,
  [CurrencyId] int,
  [ExchangeRate] decimal(18,3),
  [PriceCzk] int,
  [OfferStatusId] int,
  [LostReason] nvarchar(max),
  [EstimatedFinishDate] date,
  [Notes] nvarchar(max),
  [Active] bit DEFAULT 1,
  [CreatedBy] nvarchar(50),
  [ModifiedBy] nvarchar(50),
  [CreatedDate] datetime,
  [ModifiedDate] datetime,
  CONSTRAINT [FK_memo.Offer_memo_Contact_ContactId] FOREIGN KEY ([ContactId])
    REFERENCES [memo].[Contact] ([ContactId]),
  CONSTRAINT [FK_memo.Offer_memo_Company_CompanyId] FOREIGN KEY ([CompanyId])
    REFERENCES [memo].[Company] ([CompanyId]),
  CONSTRAINT [FK_memo.Offer_memo_OfferStatus_OfferStatusId] FOREIGN KEY ([OfferStatusId])
    REFERENCES [memo].[OfferStatus] ([OfferStatusId]),
  CONSTRAINT [FK_memo.Offer_memo_Currency_CurrencyId] FOREIGN KEY ([CurrencyId])
    REFERENCES [memo].[Currency] ([CurrencyId])  -- ON DELETE CASCADE
)
GO


-----------------------------------------------------------------------------------------
IF OBJECT_ID('memo.Order', 'U') IS NOT NULL
  DROP TABLE [memo].[Order]
GO
-----------------------------------------------------------------------------------------
CREATE TABLE [memo].[Order] (
  [OrderId] int PRIMARY KEY IDENTITY(1, 1),
  [OfferId] int,
  [OrderName] nvarchar(50) UNIQUE,
  [NegotiatedPrice] int,
  [PriceFinal] int,
  [PriceDiscount] int,
  [OrderCode] nvarchar(50),
  [EveContactName] nvarchar(50),
  [TotalHours] int,
  [ExchangeRate] decimal(18,3),
  [PriceFinalCzk] int,
  [Notes] nvarchar(max),
  [Active] bit DEFAULT 1,
  [CreatedBy] nvarchar(50),
  [ModifiedBy] nvarchar(50),
  [CreatedDate] datetime,
  [ModifiedDate] datetime
  CONSTRAINT [FK_memo.Order_memo_Offer_OfferId] FOREIGN KEY ([OfferId])
    REFERENCES [memo].[Offer] ([OfferId])
)
GO


-----------------------------------------------------------------------------------------
IF OBJECT_ID('memo.Invoice', 'U') IS NOT NULL
  DROP TABLE [memo].[Invoice]
GO
-----------------------------------------------------------------------------------------
CREATE TABLE [memo].[Invoice] (
  [InvoiceId] int PRIMARY KEY IDENTITY(1, 1),
  [OrderId] int,
  [InvoiceDueDate] date,
  [InvoiceIssueDate] date,
  [Cost] decimal(18,3),
  [CostCzk] decimal(18,3),
  [DeliveryNote] nvarchar(255),
  CONSTRAINT [FK__memo.Invoice__memo.Order__OrderId] FOREIGN KEY ([OrderId])
    REFERENCES [memo].[Order] ([OrderId]) ON DELETE CASCADE
)
GO


-----------------------------------------------------------------------------------------
IF OBJECT_ID('memo.OtherCost', 'U') IS NOT NULL
  DROP TABLE [memo].[OtherCost]
GO
-----------------------------------------------------------------------------------------
CREATE TABLE [memo].[OtherCost]
(
  [OtherCostId] int IDENTITY(1, 1),
  [OrderId] int,
  [Subject] nvarchar(max) NOT NULL,
  [Cost] decimal(18,3) NOT NULL,
  [CostCzk] decimal(18,3) NOT NULL,
  CONSTRAINT [PK__OtherCostId] PRIMARY KEY ([OtherCostId]),
  CONSTRAINT [FK__memo.OtherCost__memo.Order__OrderId] FOREIGN KEY ([OrderId])
    REFERENCES [memo].[Order] ([OrderId])
)
GO


-----------------------------------------------------------------------------------------
IF OBJECT_ID('memo.HourWages', 'U') IS NOT NULL
  DROP TABLE [memo].[HourWages]
GO
-----------------------------------------------------------------------------------------
CREATE TABLE [memo].[HourWages]
(
  [HourWagesId] int IDENTITY(1, 1),
  [OrderId] int,
  [Subject] nvarchar(max),
  [Cost] decimal(18,3) NOT NULL,
  [CostCzk] decimal(18,3) NOT NULL,
  CONSTRAINT [PK__HourWagesId] PRIMARY KEY ([HourWagesId]),
  CONSTRAINT [FK__memo.HourWages__memo.Order__OrderId] FOREIGN KEY ([OrderId])
    REFERENCES [memo].[Order] ([OrderId])
)
GO


-----------------------------------------------------------------------------------------
IF OBJECT_ID('memo.BugReport', 'U') IS NOT NULL
  DROP TABLE [memo].[BugReport]
GO
-----------------------------------------------------------------------------------------
CREATE TABLE [memo].[BugReport] (
  [BugReportId] int PRIMARY KEY IDENTITY(1, 1),
  [Subject] nvarchar(255),
  [Details] nvarchar(max),
  [Priority] nvarchar(25),
  [Category] nvarchar(25),
  [Resolved] bit DEFAULT 0,
  [CreatedBy] nvarchar(50),
  [ModifiedBy] nvarchar(50),
  [CreatedDate] datetime,
  [ModifiedDate] datetime
)
GO



-- AUDITING INSERT, MODIFY, DELETE
-----------------------------------------------------------------------------------------
IF OBJECT_ID('memo.Audit', 'U') IS NOT NULL
  DROP TABLE [memo].[Audit]
GO
-----------------------------------------------------------------------------------------
CREATE TABLE [memo].[Audits]
  (
    AuditId INT IDENTITY PRIMARY KEY,
    Type CHAR(1),
    TableName NVARCHAR(128),
    PK NVARCHAR(1000),
    FieldName NVARCHAR(128),
    OldValue NVARCHAR(1000),
    NewValue NVARCHAR(1000),
    UpdateDate DATETIME,
    UserName NVARCHAR(128),
    UpdateBy NVARCHAR(128)
  )
GO

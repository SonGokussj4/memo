

DELETE FROM [memo].[Company];

INSERT INTO [memo].[Company]
  (Name, City, Address, Phone, Web)
VALUES
  ('Škoda Auto, a.s.', 'Mladá Boleslav', NULL, '+420 356 815 354', 'www.skoda-auto.cz'),
  ('Andreas Stihl', 'Waiblingen', NULL, '+49 607 130 553 58', 'www.stihl.de'),
  ('Cybex', 'Vídeň / Bayreuth', NULL, '+49 921 785 114 80', 'www.cybex-online.com'),
  ('Varroc', 'Nový Jičín', NULL, '+420 556 623 111', 'www.varroclighting.com')


DELETE FROM [memo].[Contact];

INSERT INTO [memo].[Contact]
  (PersonName, Department, Phone, Email)
VALUES
  ('Jan Verner', 'ITC', '+420 603 430 091', 'jverner@evektor.cz'),
  ('Ivo Grác', 'C2', '123', 'igrac@evektor.cz')


DELETE FROM [memo].[Offer];

-- CREATE TABLE [memo].[Offer] (
--   [OfferId] int PRIMARY KEY IDENTITY(1, 1),
--   [OfferName] nvarchar(50),
--   [ReceiveDate] date,
--   [SentDate] date,
--   [Subject] text,
--   [ContactId] int,
--   [CompanyId] int,
--   [EveDivision] nvarchar(50) NOT NULL CHECK ([EveDivision] IN ('AD', 'ED')),
--   [EveDepartment] nvarchar(50),
--   [EveCreatedUser] nvarchar(50),
--   [Price] int,
--   [CurrencyId] int,
--   [ExchangeRate] float,
--   [PriceCzk] int,
--   [Status] int,
--   [LostReason] text,
--   [CreateDate] date
-- )
-- GO

DELETE FROM [memo].[OfferStatus];

INSERT INTO [memo].[OfferStatus]
  (Status)
VALUES
  ('In Progress'),
  ('Won'),
  ('Lost')


-- CREATE TABLE [memo].[Order] (
--   [OrderId] int PRIMARY KEY IDENTITY(1, 1),
--   [OfferId] int,
--   [OrderName] nvarchar(50),
--   [PriceFinal] int,
--   [PriceDiscount] int,
--   [OrderCode] nvarchar(50),
--   [ContactId] int,
--   [HourWage] float,
--   [TotalHours] int,
--   [InvoiceIssueDate] date,
--   [InvoiceDueDate] date,
--   [ExchangeRate] float,
--   [PriceFinalCzk] int,
--   [Notes] text,
--   [CreateDate] date
-- )
-- GO

DELETE FROM [memo].[Currency];

INSERT INTO [memo].[Currency]
  (Name)
VALUES
  ('CZK'),
  ('EUR'),
  ('USD')

-- CREATE TABLE [memo].[Currency] (
--   [CurrencyId] int PRIMARY KEY IDENTITY(1, 1),
--   [Name] nvarchar(10)
-- )
-- GO



-- Zmenit jmeno constrain za jine
-- ##################################
--EXEC sp_rename N'memo.UQ_OrderName', N'memo.UQ_OfferName', N'OBJECT'


-- Vynulovat pocitadlo novych zaznamu
-- ##################################
-- DBCC CHECKIDENT ('memo.Company', RESEED, 0)


-- Zmenit typ sloupce z [text] na [ntext]
-- ##################################
-- ALTER TABLE [memo].[Offer]
-- ALTER COLUMN [LostReason] [varchar](max) NULL;
-- ALTER TABLE [memo].[Offer]
-- ALTER COLUMN [LostReason] [ntext] NULL;


-- Upravit  sloupec/sloupce tabulky při konkrétní podmínce
-- ##################################
-- UPDATE [dbo].[cOrders]
-- SET
--     Planned = 666
-- WHERE
--     OrderCode = '927.0040'
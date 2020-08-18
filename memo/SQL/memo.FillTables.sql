

DELETE FROM [memo].[Company];

--INSERT INTO [memo].[Company]
--  (CompanyId, Name, City, Address, Phone, Web, InvoiceDueDays)
--VALUES
--  ('5', 'Škoda Auto, a.s.', 'Mladá Boleslav', NULL, '+420 356 815 354', 'www.skoda-auto.cz', '30'),
--  ('13', 'Andreas Stihl', 'Waiblingen', NULL, '+49 607 130 553 58', 'www.stihl.de', '66'),
--  ('14', 'Cybex', 'Vídeň / Bayreuth', NULL, '+49 921 785 114 80', 'www.cybex-online.com', '15'),
--  ('15', 'Varroc', 'Nový Jičín', NULL, '+420 556 623 111', 'www.varroclighting.com', '45')

INSERT INTO [memo].[Company]
  ([Name], [City], [Address], [Phone], [Web], [InvoiceDueDays])
VALUES
  ('Acasias', '', '', '', '', '0'),
  ('Bosport', '', '', '', '', '0'),
  ('Cross', '', '', '', '', '0'),
  ('Cybex', '', '', '', '', '0'),
  ('EVAT / IFE', '', '', '', '', '0'),
  ('Gubesch', '', '', '', '', '0'),
  ('HAUK', '', '', '', '', '0'),
  ('HELLA', '', '', '', '', '0'),
  ('Keramtech', '', '', '', '', '0'),
  ('Mercedes-AMG GmbH', '', '', '', '', '0'),
  ('Plastika', '', '', '', '', '0'),
  ('Promens', '', '', '', '', '0'),
  ('S.A.B. Aerospace ', '', '', '', '', '0'),
  ('SAFRAN', '', '', '', '', '0'),
  ('SAS Autosystemtechnik', '', '', '', '', '0'),
  ('Siemens Mobility', '', '', '', '', '0'),
  ('Škoda Auto', '', '', '', '', '30'),
  ('Škoda Auto ', '', '', '', '', '30'),
  ('Škoda Auto - EKC', '', '', '', '', '30'),
  ('Škoda Auto - EKS', '', '', '', '', '30'),
  ('Škoda Auto - EKT', '', '', '', '', '30'),
  ('Škoda Auto - EKX', '', '', '', '', '30'),
  ('Škoda Auto EBW', '', '', '', '', '30'),
  ('Škoda Auto EKF', '', '', '', '', '30'),
  ('Škoda Auto EKZ', '', '', '', '', '30'),
  ('ŠKODA_TRANS', '', '', '', '', '30'),
  ('STIHL', '', '', '', '', '0'),
  ('Telene', '', '', '', '', '0')



DELETE FROM [memo].[Contact];

INSERT INTO [memo].[Contact]
  ([PersonName], [Department], [Phone], [Email])
VALUES
  ('bosport', '', '', 'bosport@bosport.eu'),
  ('georg.ude', '', '', 'georg.ude@cybex-online.com'),
  ('stefan.aschinger', '', '', 'stefan.aschinger@cybex-online.com'),
  ('J.Nauhardt', '', '', 'J.Nauhardt@gubesch.de'),
  ('tomas.rehor', '', '', 'tomas.rehor@hauk.cz'),
  ('juraj.chovanec', '', '', 'juraj.chovanec@hella.com'),
  ('zdenek.bures', '', '', 'zdenek.bures@hella.com'),
  ('patrik.coufal', '', '', 'patrik.coufal@mbtool.cz'),
  ('hrosova', '', '', 'hrosova@plastika.cz'),
  ('hebnarova', '', '', 'hebnarova@plastika.cz'),
  ('petr.marsalek', '', '', 'petr.marsalek@safrangoup.com'),
  ('petr.sedlacek', '', '', 'petr.sedlacek@safrangroup.com'),
  ('michal.dufek', '', '', 'michal.dufek@safrangroup.com'),
  ('jaroslav.brodsky', '', '', 'jaroslav.brodsky@skoda.cz'),
  ('Jan.Tymich', '', '', 'Jan.Tymich@skoda-auto.cz'),
  ('Jaroslav.Havel', '', '', 'Jaroslav.Havel@skoda-auto.cz'),
  ('Robert.Prochyra2', '', '', 'Robert.Prochyra2@skoda-auto.cz'),
  ('Zdenek.Drapak', '', '', 'Zdenek.Drapak@skoda-auto.cz'),
  ('Pavel.Sevela', '', '', 'Pavel.Sevela@skoda-auto.cz'),
  ('Martin.Bradac', '', '', 'Martin.Bradac@skoda-auto.cz'),
  ('Jan.Hrncir', '', '', 'Jan.Hrncir@skoda-auto.cz'),
  ('TOMAS.HORNICEK', '', '', 'TOMAS.HORNICEK@SKODA-AUTO.CZ'),
  ('Vladimir.Sestak', '', '', 'Vladimir.Sestak@skoda-auto.cz'),
  ('David.Dvorak', '', '', 'David.Dvorak@skoda-auto.cz'),
  ('Michaela.Kabelkova', '', '', 'Michaela.Kabelkova@skoda-auto.cz'),
  ('Jirina.Cabelkova', '', '', 'Jirina.Cabelkova@skoda-auto.cz'),
  ('ext.Iveta.Sebestova2', '', '', 'Iveta.Sebestova2@skoda-auto.cz'),
  ('Milos.Jambor', '', '', 'Milos.Jambor@skoda-auto.cz'),
  ('Jiri.Holoubek', '', '', 'Jiri.Holoubek@skoda-auto.cz'),
  ('Janos.Barsony', '', '', 'Janos.Barsony@skoda-auto.cz'),
  ('Martin.Stastny', '', '', 'Martin.Stastny@skoda-auto.cz'),
  ('Martin.Cerny2', '', '', 'Martin.Cerny2@skoda-auto.cz'),
  ('Gilles.Recher', '', '', 'Gilles.Recher@telene.com')


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
  (Name)
VALUES
  ('Čeká'),
  ('Výhra'),
  ('Prohra')


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
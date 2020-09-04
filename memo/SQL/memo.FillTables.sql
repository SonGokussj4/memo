

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
  ('ŠKODA AUTO a.s.', '', '', '', '', '0'),
  ('Andreas Stihl  AG  & Co.  KG', '', '', '', '', '0'),
  ('SAFRAN CABIN CZ, s.r.o.', '', '', '', '', '0'),
  ('EVEKTOR-AEROTECHNIK, a.s.', '', '', '', '', '0'),
  ('HAUK  s.r.o.', '', '', '', '', '0'),
  ('ŠKODA TRANSPORTATION a.s.', '', '', '', '', '0'),
  ('HELLA  AUTOTECHNIK NOVA, s.r.o.', '', '', '', '', '0'),
  ('Aircraft Industries, a.s.', '', '', '', '', '0'),
  ('CYBEX GmbH', '', '', '', '', '0'),
  ('Mercedes-AMG GmbH', '', '', '', '', '0'),
  ('Gubesch Engineering & Production GmbH', '', '', '', '', '0'),
  ('Siemens Mobility, s.r.o.', '', '', '', '', '0'),
  ('AIR TEAM service, s.r.o.', '', '', '', '', '0'),
  ('AERO Vodochody  AEROSPACE a.s.', '', '', '', '', '0'),
  ('Varroc Lighting Systems, s.r.o.', '', '', '', '', '0'),
  ('seele pilsen s.r.o.', '', '', '', '', '0'),
  ('STIHL Tirol GmbH', '', '', '', '', '0'),
  ('S.A.B. Aerospace s.r.o.', '', '', '', '', '0'),
  ('PLASTIKA a.s.', '', '', '', '', '0'),
  ('Promens a.s.', '', '', '', '', '0'),
  ('ERA a.s.', '', '', '', '', '0'),
  ('UNIS   a.s.', '', '', '', '', '0'),
  ('IMS - Drašnar s.r.o.', '', '', '', '', '0'),
  ('TELENE SAS', '', '', '', '', '0'),
  ('Letecké opravovne Trenčín, a.s.', '', '', '', '', '0'),
  ('KERAMTECH s.r.o.', '', '', '', '', '0'),
  ('ARAVER CZ, s.r.o.', '', '', '', '', '0'),
  ('Blanik Aircraft CZ s.r.o.', '', '', '', '', '0'),
  ('ZLIN  AIRCRAFT a.s.', '', '', '', '', '0'),
  ('BRM AERO, s.r.o.', '', '', '', '', '0'),
  ('D4.CZ s.r.o.', '', '', '', '', '0'),
  ('MB iDesign Group a.s.', '', '', '', '', '0'),
  ('SAZ  Aerospace s.r.o.', '', '', '', '', '0'),
  ('HM PARTNERS s.r.o.', '', '', '', '', '0'),
  ('KOVOPlast, výrobní družstvo', '', '', '', '', '0'),
  ('Univerzita Tomáše Bati ve Zlíně', '', '', '', '', '0'),
  ('Česká zbrojovka, a.s.', '', '', '', '', '0'),
  ('EBZ Hoffmann s.r.o.', '', '', '', '', '0'),
  ('David Bartoš', '', '', '', '', '0'),
  ('Michal Tomala', '', '', '', '', '0'),
  ('BOSPORT s.r.o.', '', '', '', '', '0'),
  ('Petr  Havlík', '', '', '', '', '0'),
  ('GUMOTEX Automotive Břeclav, s.r.o.', '', '', '', '', '0'),
  ('protechnik consulting s.r.o.', '', '', '', '', '0'),
  ('Okresní soud v Novém Jičíně', '', '', '', '', '0'),
  ('SAS Autosystemtechnik S.R.O.', '', '', '', '', '0'),
  ('Euro VAT Reclaim, s.r.o.', '', '', '', '', '0'),
  ('Okresní soud v Šumperku', '', '', '', '', '0'),
  ('Město Solnice', '', '', '', '', '0'),
  ('ZŠ Kvasiny', '', '', '', '', '0'),
  ('Ing. Marek Ambros', '', '', '', '', '0'),
  ('Falcon Aircraft s.r.o.', '', '', '', '', '0'),
  ('Drobný odběratel', '', '', '', '', '0'),
  ('Air Jihlava - service s.r.o.', '', '', '', '', '0'),
  ('ZK  AIRCRAFT SERVICE  s.r.o.', '', '', '', '', '0')





DELETE FROM [memo].[Contact];

INSERT INTO [memo].[Contact]
  ([PersonName], [Department], [Phone], [Email], [PersonLastName], [PersonTitle])
VALUES
  ('bosport', '', '', 'bosport@bosport.eu', '', ''),
  ('georg.ude', '', '', 'georg.ude@cybex-online.com', '', 'Ing'),
  ('stefan.aschinger', '', '', 'stefan.aschinger@cybex-online.com', '', 'Bc'),
  ('J.Nauhardt', '', '', 'J.Nauhardt@gubesch.de', '', ''),
  ('tomas.rehor', '', '', 'tomas.rehor@hauk.cz', '', ''),
  ('juraj.chovanec', '', '', 'juraj.chovanec@hella.com', '', ''),
  ('zdenek.bures', '', '', 'zdenek.bures@hella.com', '', ''),
  ('patrik.coufal', '', '', 'patrik.coufal@mbtool.cz', '', ''),
  ('hrosova', '', '', 'hrosova@plastika.cz', '', ''),
  ('hebnarova', '', '', 'hebnarova@plastika.cz', '', ''),
  ('petr.marsalek', '', '', 'petr.marsalek@safrangoup.com', '', ''),
  ('petr.sedlacek', '', '', 'petr.sedlacek@safrangroup.com', '', ''),
  ('michal.dufek', '', '', 'michal.dufek@safrangroup.com', '', ''),
  ('jaroslav.brodsky', '', '', 'jaroslav.brodsky@skoda.cz', '', ''),
  ('Jan.Tymich', '', '', 'Jan.Tymich@skoda-auto.cz', '', ''),
  ('Jaroslav.Havel', '', '', 'Jaroslav.Havel@skoda-auto.cz', '', ''),
  ('Robert.Prochyra2', '', '', 'Robert.Prochyra2@skoda-auto.cz', '', ''),
  ('Zdenek.Drapak', '', '', 'Zdenek.Drapak@skoda-auto.cz', '', ''),
  ('Pavel.Sevela', '', '', 'Pavel.Sevela@skoda-auto.cz', '', ''),
  ('Martin.Bradac', '', '', 'Martin.Bradac@skoda-auto.cz', '', ''),
  ('Jan.Hrncir', '', '', 'Jan.Hrncir@skoda-auto.cz', '', ''),
  ('TOMAS.HORNICEK', '', '', 'TOMAS.HORNICEK@SKODA-AUTO.CZ', '', ''),
  ('Vladimir.Sestak', '', '', 'Vladimir.Sestak@skoda-auto.cz', '', ''),
  ('David.Dvorak', '', '', 'David.Dvorak@skoda-auto.cz', '', ''),
  ('Michaela.Kabelkova', '', '', 'Michaela.Kabelkova@skoda-auto.cz', '', ''),
  ('Jirina.Cabelkova', '', '', 'Jirina.Cabelkova@skoda-auto.cz', '', ''),
  ('ext.Iveta.Sebestova2', '', '', 'Iveta.Sebestova2@skoda-auto.cz', '', ''),
  ('Milos.Jambor', '', '', 'Milos.Jambor@skoda-auto.cz', '', ''),
  ('Jiri.Holoubek', '', '', 'Jiri.Holoubek@skoda-auto.cz', '', ''),
  ('Janos.Barsony', '', '', 'Janos.Barsony@skoda-auto.cz', '', ''),
  ('Martin.Stastny', '', '', 'Martin.Stastny@skoda-auto.cz', '', ''),
  ('Martin.Cerny2', '', '', 'Martin.Cerny2@skoda-auto.cz', '', ''),
  ('Gilles.Recher', '', '', 'Gilles.Recher@telene.com', '', '')


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
  ([Name], [CultureCode])
VALUES
  ('CZK', 'cs-CZ'),
  ('EUR', 'de-DE'),
  ('USD', 'en-US')

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


-- Změnit sloupec double na decimal

--SELECT
--	ExchangeRate,
--	CAST([ExchangeRate] as decimal(18,3)) exchangerate_decimal
--from [memo].[Order]

-- UPDATE
-- 	[memo].[Order]
-- SET
-- 	[ExchangeRate] = CAST([ExchangeRate] as decimal(18,3));

--  ALTER TABLE
--  	[memo].[Order]
--  ALTER COLUMN
--  	[ExchangeRate] DECIMAL(18,3);
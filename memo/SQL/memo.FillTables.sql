
DELETE FROM [memo].[Company];
INSERT INTO [memo].[Company]
  ([Name], [InvoiceDueDays])
VALUES
  ('5M LEVIT s.r.o.', '14'),
  ('Aircraft Industries, a.s.', '30'),
  ('Andreas Stihl  AG  & Co.  KG', '30'),
  ('ARAVER CZ, s.r.o', '3'),
  ('BOSPORT s.r.o.', '30'),
  ('CYBEX GmbH', '30'),
  ('Česká agentura pro standardizaci', '14'),
  ('D4.CZ s.r.o.', '30'),
  ('David Bartoš', '30'),
  ('EBZ Hoffmann s.r.o.', '30'),
  ('EDAG Engineering CZ spol. s r.o.', '30'),
  ('ERA a.s.', '30'),
  ('EVEKTOR-AEROTECHNIK, a.s.', '30'),
  ('Gubesch Engineering & Production GmbH', '30'),
  ('Gumárny  Zubří, akciová  společnost', '30'),
  ('GUMOTEX Automotive Břeclav, s.r.o.', '30'),
  ('HAUK  s.r.o.', '30'),
  ('IMS - Drašnar s.r.o.', '30'),
  ('Ing. Marek Ambros', '30'),
  ('Letecké opravovne Trenčín, a.s.', '30'),
  ('LUKOV Plast spol. s r.o.', '30'),
  ('Mercedes AMG', '30'),
  ('Michal Tomala', '30'),
  ('New Space Technologies s.r.o.', '30'),
  ('Petr  Havlík', '30'),
  ('PLASTIKA a.s.', '30'),
  ('Promens a.s.', '30'),
  ('SAFRAN CABIN CZ, s.r.o.', '60'),
  ('SAFRAN CABIN GAYLLEYS US', '60'),
  ('SAZ Aerospace s.r.o.', '14'),
  ('Seele Pilsen s.r.o.', '30'),
  ('Siemens Mobility', '30'),
  ('STIHL Tirol GmbH', '30'),
  ('ŠKODA AUTO a.s.', '30'),
  ('ŠKODA TRANSPORTATION a.s.', '60'),
  ('TELENE SAS', '60'),
  ('Varroc Lighting Systems, s.r.o.', '75'),
  ('ZLIN  AIRCRAFT a.s.', '30')



DELETE FROM [memo].[Contact];
INSERT INTO [memo].[Contact]
  ([PersonName], [PersonLastName], [Email], [PersonTitle])
VALUES
  ('bosport', '', 'bosport@bosport.eu', ''),
  ('georg', 'ude', 'georg.ude@cybex-online.com', 'Ing'),
  ('stefan', 'aschinger', 'stefan.aschinger@cybex-online.com', 'Bc'),
  ('Juraj', 'Nauhardt', 'J.Nauhardt@gubesch.de', ''),
  ('tomas', 'rehor', 'tomas.rehor@hauk.cz', ''),
  ('juraj', 'chovanec', 'juraj.chovanec@hella.com', ''),
  ('zdenek', 'bures', 'zdenek.bures@hella.com', ''),
  ('patrik', 'coufal', 'patrik.coufal@mbtool.cz', ''),
  ('hrosova', '', 'hrosova@plastika.cz', ''),
  ('hebnarova', '', 'hebnarova@plastika.cz', ''),
  ('petr', 'marsalek', 'petr.marsalek@safrangoup.com', ''),
  ('petr', 'sedlacek', 'petr.sedlacek@safrangroup.com', ''),
  ('michal', 'dufek', 'michal.dufek@safrangroup.com', ''),
  ('jaroslav', 'brodsky', 'jaroslav.brodsky@skoda.cz', ''),
  ('Jan', 'Tymich', 'Jan.Tymich@skoda-auto.cz', ''),
  ('Jaroslav', 'Havel', 'Jaroslav.Havel@skoda-auto.cz', ''),
  ('Robert', 'Prochyra2', 'Robert.Prochyra2@skoda-auto.cz', ''),
  ('Zdenek', 'Drapak', 'Zdenek.Drapak@skoda-auto.cz', ''),
  ('Pavel', 'Sevela', 'Pavel.Sevela@skoda-auto.cz', ''),
  ('Martin', 'Bradac', 'Martin.Bradac@skoda-auto.cz', ''),
  ('Jan', 'Hrncir', 'Jan.Hrncir@skoda-auto.cz', ''),
  ('TOMAS', 'HORNICEK', 'TOMAS.HORNICEK@SKODA-AUTO.CZ', ''),
  ('Vladimir', 'Sestak', 'Vladimir.Sestak@skoda-auto.cz', ''),
  ('David', 'Dvorak', 'David.Dvorak@skoda-auto.cz', ''),
  ('Michaela', 'Kabelkova', 'Michaela.Kabelkova@skoda-auto.cz', ''),
  ('Jirina', 'Cabelkova', 'Jirina.Cabelkova@skoda-auto.cz', ''),
  ('ext.Iveta', 'Sebestova2', 'Iveta.Sebestova2@skoda-auto.cz', ''),
  ('Milos', 'Jambor', 'Milos.Jambor@skoda-auto.cz', ''),
  ('Jiri', 'Holoubek', 'Jiri.Holoubek@skoda-auto.cz', ''),
  ('Janos', 'Barsony', 'Janos.Barsony@skoda-auto.cz', ''),
  ('Martin', 'Stastny', 'Martin.Stastny@skoda-auto.cz', ''),
  ('Martin', 'Cerny2', 'Martin.Cerny2@skoda-auto.cz', ''),
  ('Gilles', 'Recher', 'Gilles.Recher@telene.com', '')



DELETE FROM [memo].[Offer];



DELETE FROM [memo].[OfferStatus];
INSERT INTO [memo].[OfferStatus]
  (Name)
VALUES
  ('Čeká'),
  ('Výhra'),
  ('Prohra')



DELETE FROM [memo].[Currency];
INSERT INTO [memo].[Currency]
  ([Name], [CultureCode])
VALUES
  ('CZK', 'cs-CZ'),
  ('EUR', 'de-DE'),
  ('USD', 'en-US')



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
-- ExchangeRate,
-- CAST([ExchangeRate] as decimal(18,3)) exchangerate_decimal
--from [memo].[Order]

-- UPDATE
--  [memo].[Order]
-- SET
--  [ExchangeRate] = CAST([ExchangeRate] as decimal(18,3));

--  ALTER TABLE
--   [memo].[Order]
--  ALTER COLUMN
--   [ExchangeRate] DECIMAL(18,3);


-- Presunout dbo.spProcedure do memo.spProcedure
-- ##################################
-- ALTER SCHEMA memo TRANSFER dbo.spProcedure;



-- UPDATE
-- 	[memo].[Order]
-- SET
-- 	[memo].[Order].NegotiatedPrice = [memo].[Offer].Price
-- FROM
-- 	[memo].[Order]
-- INNER JOIN
-- 	[memo].[Offer]
-- ON
-- 	[memo].[Order].[OfferId] = [memo].[Offer].[OfferId]


------------------- ADD PRIMARY KEY to existing table --------------
-- ALTER TABLE memo.Audit
-- ADD AuditId INT IDENTITY PRIMARY KEY;




--------------------------------------------------------------------
-----------------   DROPOVANI RUZNYCH VECI   -----------------------
--------------------------------------------------------------------
-- IF OBJECT_ID('[memo].TR__Company__AUDIT', 'TR') IS NOT NULL
--     DROP TRIGGER [memo].TR__Company__AUDIT
-- GO

-- AF = Aggregate function (CLR)
-- C = CHECK constraint
-- D = DEFAULT (constraint or stand-alone)
-- F = FOREIGN KEY constraint
-- FN = SQL scalar function
-- FS = Assembly (CLR) scalar-function
-- FT = Assembly (CLR) table-valued function
-- IF = SQL inline table-valued function
-- IT = Internal table
-- P = SQL Stored Procedure
-- PC = Assembly (CLR) stored-procedure
-- PG = Plan guide
-- PK = PRIMARY KEY constraint
-- R = Rule (old-style, stand-alone)
-- RF = Replication-filter-procedure
-- S = System base table
-- SN = Synonym
-- SO = Sequence object
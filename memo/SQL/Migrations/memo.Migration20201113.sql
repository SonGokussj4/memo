-----------------------------------------------------------------------------------------
-- PRIDAT DO OtherCodes SLOUPCE HourWageCost, HourWageCostCzk, HourWageSubject
ALTER TABLE [memo].[OrderCodes]
ADD HourWageCost DECIMAL(18,3);

ALTER TABLE [memo].[OrderCodes]
ADD HourWageCostCzk DECIMAL(18,3);

ALTER TABLE [memo].[OrderCodes]
ADD HourWageSubject NVARCHAR(50);


-----------------------------------------------------------------------------------------
-- NAPLNIT OtherCodes SLOUPCE HourWageCost, HourWageCostCzk, HourWageSubject DLE TABULKY HourWages
UPDATE [memo].[OrderCodes]
SET
    HourWageCost = y.Cost,
    HourWageCostCzk = y.CostCzk,
    HourWageSubject = y.Subject
FROM [memo].[OrderCodes] x, [memo].[HourWages] y
WHERE x.OrderId = y.OrderId;


-----------------------------------------------------------------------------------------
-- NAPLNIT OtherCodes SLOUPCE HourWageCost, HourWageCostCzk, HourWageSubject DLE TABULKY HourWages
ALTER TABLE [memo].[OrderCodes]
ALTER COLUMN OrderCode NVARCHAR(50) NULL;


-----------------------------------------------------------------------------------------
-- VYMAZAT TABULKU HourWages
DROP TABLE [memo].[HourWages];


-----------------------------------------------------------------------------------------
-- VYMAZAT HourWages reference z Auditu
DELETE FROM [memo].[Audit]
WHERE
    TableName = 'HourWages';


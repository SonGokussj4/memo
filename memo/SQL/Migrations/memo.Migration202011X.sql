-----------------------------------------------------------------------------------------
-- VYTVORIT TABULKU Contracts
IF OBJECT_ID('memo.Contracts', 'U') IS NOT NULL
  DROP TABLE [memo].[Contracts]
GO
-----------------------------------------------------------------------------------------
CREATE TABLE [memo].[Contracts]
(
    [ContractsId] INT PRIMARY KEY IDENTITY(1, 1),
    [ContractName] NVARCHAR(50) NOT NULL,
    [ReceiveDate] DATE NOT NULL,
    [Subject] NVARCHAR(max),
    [EveDivision] NVARCHAR(50) NOT NULL,
    [EveDepartment] NVARCHAR(50) NOT NULL,
    [EveCreatedUser] NVARCHAR(50) NOT NULL,
    [ContactId] INT NOT NULL,
    [CompanyId] INT NOT NULL,
    [Price] INT,
    [PriceCzk] INT,
    [CurrencyId] INT,
    [ExchangeRate] DECIMAL(18,3),
    [Notes] NVARCHAR(max),
    [Active] BIT DEFAULT 1,
    [CreatedBy] NVARCHAR(50),
    [ModifiedBy] NVARCHAR(50),
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


-----------------------------------------------------------------------------------------
-- VYTVORIT TRIGGER PRO TABULKU Contracts
-----------------------------------------------------------------------------------------
IF OBJECT_ID('[memo].TR__Contracts__AUDIT', 'TR') IS NOT NULL
    DROP TRIGGER [memo].TR__Contracts__AUDIT
GO
-----------------------------------------------------------------------------------------
CREATE TRIGGER [memo].[TR__Contracts__AUDIT]
ON [memo].[Contracts]
AFTER UPDATE, INSERT, DELETE
AS
DECLARE @bit            INT,
        @field          INT,
        @maxfield       INT,
        @char           INT,
        @fieldname      NVARCHAR(128),
        @TableName      NVARCHAR(128),
        @PKCols         NVARCHAR(1000),
        @sql            NVARCHAR(2000),
        @UpdateDate     NVARCHAR(21),
        @UpdateBy       NVARCHAR(128),
        @UserName       NVARCHAR(128),
        @Type           CHAR(1),
        @PKSelect       NVARCHAR(1000)

-- You will need to change @TableName to match the table to be audited.
-- Here we made GUESTS for your example.
SELECT @TableName = 'Contracts'

SELECT @UserName = SUSER_SNAME(),
  @UpdateDate = CONVERT(NVARCHAR(30), GETDATE(), 126)

SELECT @UpdateBy = memo.GetUserContext()

-- Action
IF EXISTS (SELECT *
FROM INSERTED)
  IF EXISTS (SELECT *
FROM DELETED)
    SELECT @Type = 'U'
  ELSE
    SELECT @Type = 'I'
ELSE
  SELECT @Type = 'D'

-- Get list of columns
SELECT *
INTO #ins
FROM INSERTED

SELECT *
INTO #del
FROM DELETED

-- Get primary key columns for full outer join
SELECT @PKCols = COALESCE(@PKCols + ' and', ' on') + ' i.[' + c.COLUMN_NAME + '] = d.[' + c.COLUMN_NAME + ']'
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk, INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
WHERE pk.TABLE_NAME = @TableName
  AND CONSTRAINT_TYPE = 'PRIMARY KEY'
  AND c.TABLE_NAME = pk.TABLE_NAME
  AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

-- Get primary key select for insert
SELECT @PKSelect = COALESCE(@PKSelect + '+', '') + '''<[' + COLUMN_NAME + ']=''+convert(nvarchar(100), coalesce(i.[' + COLUMN_NAME + '],d.[' + COLUMN_NAME + ']))+''>'''
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk, INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
WHERE pk.TABLE_NAME = @TableName
  AND CONSTRAINT_TYPE = 'PRIMARY KEY'
  AND c.TABLE_NAME = pk.TABLE_NAME
  AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

IF @PKCols IS NULL
BEGIN
  RAISERROR('no PK on table %s', 16, -1, @TableName)
  RETURN
END

-- FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName
-- @maxfield = MAX(COLUMN_NAME)
SELECT @field = 0, @maxfield = MAX(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + @TableName), COLUMN_NAME, 'ColumnID'))
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = @TableName

WHILE @field < @maxfield
BEGIN
  SELECT @field = MIN(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + @TableName), COLUMN_NAME, 'ColumnID' ))
  FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_NAME = @TableName
    AND COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + @TableName), COLUMN_NAME, 'ColumnID' ) > @field
  SELECT @bit = (@field - 1)% 8 + 1
  SELECT @bit = POWER(2, @bit - 1)
  SELECT @char = ((@field - 1) / 8) + 1
  IF SUBSTRING(COLUMNS_UPDATED(), @char, 1) & @bit > 0
    OR @Type IN ('I', 'D')
  BEGIN
    SELECT @fieldname = COLUMN_NAME
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = @TableName
      AND COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + @TableName), COLUMN_NAME, 'ColumnID') = @field
    SELECT @sql ='
            insert into memo.Audit (
              Type,
              TableName,
              PK,
              FieldName,
              OldValue,
              NewValue,
              UpdateDate,
              UserName,
              UpdateBy
           )
           select ''' + @Type + ''','''
                      + @TableName + ''',' + @PKSelect
                      + ',''' + @fieldname + ''''
                      + ',convert(nvarchar(1000),d.' + @fieldname + ')'
                      + ',convert(nvarchar(1000),i.' + @fieldname + ')'
                      + ',''' + @UpdateDate + ''''
                      + ',''' + @UserName + ''''
                      + ',''' + @UpdateBy + ''''
                      + ' from #ins i full outer join #del d'
                      + @PKCols
                      + ' where i.' + @fieldname + ' <> d.' + @fieldname
                      + ' or (i.' + @fieldname + ' is null and  d.'
                      + @fieldname
                      + ' is not null)'
                      + ' or (i.' + @fieldname + ' is not null and  d.'
                      + @fieldname
                      + ' is null)'
    EXEC (@sql)
  END
END
GO


-----------------------------------------------------------------------------------------
-- PRIDAT DO Order SLOUPEC FromType jako CHAR(1), bude tam buď N, R nebo -
ALTER TABLE [memo].[Order]
ADD FromType CHAR(1) NULL;
GO


-----------------------------------------------------------------------------------------
-- NAPLNIT Order SLOUPEC FromType == 'N'
UPDATE [memo].[Order]
SET FromType = 'N';
GO


-----------------------------------------------------------------------------------------
-- ZMENIT Order SLOUPEC FromType Z Null NA Not Null
ALTER TABLE [memo].[Order]
ALTER COLUMN FromType CHAR(1) NULL;
GO


-----------------------------------------------------------------------------------------
-- PRIDAT Order SLOUPEC ContractId
ALTER TABLE [memo].[Order]
ADD ContractId INT NULL;
GO

-- 25.11.2020 --


-- -----------------------------------------------------------------------------------------
-- -- VYTVORIT TABULKU EveUsers
-- IF OBJECT_ID('memo.EveUsers', 'U') IS NOT NULL
--   DROP TABLE [memo].[EveUsers]
-- GO
-- -----------------------------------------------------------------------------------------
-- CREATE TABLE [memo].[EveUsers]
-- (
--     [EveUserId] INT PRIMARY KEY IDENTITY(1, 1),
--     [Division] NVARCHAR(50) NOT NULL,
--     [Department] NVARCHAR(50) NOT NULL,
--     [FullName] NVARCHAR(50) NOT NULL,
--     [UserId] INT NULL,
-- )
-- GO


-----------------------------------------------------------------------------------------
-- VYTVORIT TABULKU SharedInfo
IF OBJECT_ID('memo.SharedInfo', 'U') IS NOT NULL
  DROP TABLE [memo].[SharedInfo]
GO
-----------------------------------------------------------------------------------------
CREATE TABLE [memo].[SharedInfo]
(
    [SharedInfoId] INT PRIMARY KEY IDENTITY(1, 1),
    [ReceiveDate] DATE NULL,
    [Subject] NVARCHAR(255) NULL,
    [ContactId] INT NOT NULL,
    [CompanyId] INT NOT NULL,
    [CurrencyId] INT NULL,
    [EveDivision] NVARCHAR(50) NOT NULL,
    [EveDepartment] NVARCHAR(50) NOT NULL,
    [EveCreatedUser] NVARCHAR(50) NOT NULL,
    [Price] INT NULL,
    [PriceCzk] INT NULL,
    [ExchangeRate] DECIMAL(18,3),
    [EstimatedFinishDate] DATE NULL,
    CONSTRAINT [FK__memo.SharedInfo__memo.Contact__ContactId]
        FOREIGN KEY ([ContactId])
        REFERENCES [memo].[Contact] ([ContactId]),
    CONSTRAINT [FK__memo.SharedInfo__memo.Company__CompanyId]
        FOREIGN KEY ([CompanyId])
        REFERENCES [memo].[Company] ([CompanyId]),
    CONSTRAINT [FK__memo.SharedInfo__memo.Currency__CurrencyId]
        FOREIGN KEY ([CurrencyId])
        REFERENCES [memo].[Currency] ([CurrencyId]),
)
GO


-----------------------------------------------------------------------------------------
-- VYTVORIT TRIGGER PRO TABULKU SharedInfo
-----------------------------------------------------------------------------------------
IF OBJECT_ID('[memo].TR__SharedInfo__AUDIT', 'TR') IS NOT NULL
    DROP TRIGGER [memo].TR__SharedInfo__AUDIT
GO
-----------------------------------------------------------------------------------------
CREATE TRIGGER [memo].[TR__SharedInfo__AUDIT]
ON [memo].[SharedInfo]
AFTER UPDATE, INSERT, DELETE
AS
DECLARE @bit            INT,
        @field          INT,
        @maxfield       INT,
        @char           INT,
        @fieldname      NVARCHAR(128),
        @TableName      NVARCHAR(128),
        @PKCols         NVARCHAR(1000),
        @sql            NVARCHAR(2000),
        @UpdateDate     NVARCHAR(21),
        @UpdateBy       NVARCHAR(128),
        @UserName       NVARCHAR(128),
        @Type           CHAR(1),
        @PKSelect       NVARCHAR(1000)

-- You will need to change @TableName to match the table to be audited.
-- Here we made GUESTS for your example.
SELECT @TableName = 'SharedInfo'

SELECT @UserName = SUSER_SNAME(),
  @UpdateDate = CONVERT(NVARCHAR(30), GETDATE(), 126)

SELECT @UpdateBy = memo.GetUserContext()

-- Action
IF EXISTS (SELECT *
FROM INSERTED)
  IF EXISTS (SELECT *
FROM DELETED)
    SELECT @Type = 'U'
  ELSE
    SELECT @Type = 'I'
ELSE
  SELECT @Type = 'D'

-- Get list of columns
SELECT *
INTO #ins
FROM INSERTED

SELECT *
INTO #del
FROM DELETED

-- Get primary key columns for full outer join
SELECT @PKCols = COALESCE(@PKCols + ' and', ' on') + ' i.[' + c.COLUMN_NAME + '] = d.[' + c.COLUMN_NAME + ']'
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk, INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
WHERE pk.TABLE_NAME = @TableName
  AND CONSTRAINT_TYPE = 'PRIMARY KEY'
  AND c.TABLE_NAME = pk.TABLE_NAME
  AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

-- Get primary key select for insert
SELECT @PKSelect = COALESCE(@PKSelect + '+', '') + '''<[' + COLUMN_NAME + ']=''+convert(nvarchar(100), coalesce(i.[' + COLUMN_NAME + '],d.[' + COLUMN_NAME + ']))+''>'''
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk, INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
WHERE pk.TABLE_NAME = @TableName
  AND CONSTRAINT_TYPE = 'PRIMARY KEY'
  AND c.TABLE_NAME = pk.TABLE_NAME
  AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

IF @PKCols IS NULL
BEGIN
  RAISERROR('no PK on table %s', 16, -1, @TableName)
  RETURN
END

-- FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName
-- @maxfield = MAX(COLUMN_NAME)
SELECT @field = 0, @maxfield = MAX(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + @TableName), COLUMN_NAME, 'ColumnID'))
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = @TableName

WHILE @field < @maxfield
BEGIN
  SELECT @field = MIN(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + @TableName), COLUMN_NAME, 'ColumnID' ))
  FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_NAME = @TableName
    AND COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + @TableName), COLUMN_NAME, 'ColumnID' ) > @field
  SELECT @bit = (@field - 1)% 8 + 1
  SELECT @bit = POWER(2, @bit - 1)
  SELECT @char = ((@field - 1) / 8) + 1
  IF SUBSTRING(COLUMNS_UPDATED(), @char, 1) & @bit > 0
    OR @Type IN ('I', 'D')
  BEGIN
    SELECT @fieldname = COLUMN_NAME
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = @TableName
      AND COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + @TableName), COLUMN_NAME, 'ColumnID') = @field
    SELECT @sql ='
            insert into memo.Audit (
              Type,
              TableName,
              PK,
              FieldName,
              OldValue,
              NewValue,
              UpdateDate,
              UserName,
              UpdateBy
           )
           select ''' + @Type + ''','''
                      + @TableName + ''',' + @PKSelect
                      + ',''' + @fieldname + ''''
                      + ',convert(nvarchar(1000),d.' + @fieldname + ')'
                      + ',convert(nvarchar(1000),i.' + @fieldname + ')'
                      + ',''' + @UpdateDate + ''''
                      + ',''' + @UserName + ''''
                      + ',''' + @UpdateBy + ''''
                      + ' from #ins i full outer join #del d'
                      + @PKCols
                      + ' where i.' + @fieldname + ' <> d.' + @fieldname
                      + ' or (i.' + @fieldname + ' is null and  d.'
                      + @fieldname
                      + ' is not null)'
                      + ' or (i.' + @fieldname + ' is not null and  d.'
                      + @fieldname
                      + ' is null)'
    EXEC (@sql)
  END
END
GO


-----------------------------------------------------------------------------------------
-- PRIDAT DO TABULKY Contracts SLOUPEC SharedInfoId
ALTER TABLE [memo].[Contracts]
ADD SharedInfoId INT NULL;
GO


-----------------------------------------------------------------------------------------
-- NAVÁZAT Contracts.SharedInfoId JAKO FOREIGN KEY NA SharedInfo.SharedInfoId
ALTER TABLE [memo].[Contracts]
ADD
    CONSTRAINT [FK__memo.Contracts__memo.SharedInfo__SharedInfoId]
    FOREIGN KEY ([SharedInfoId])
    REFERENCES [memo].[SharedInfo] (SharedInfoId);


-----------------------------------------------------------------------------------------
-- VLOZIT DO TABULKY SharedInfo SLOUPCE Z TABULKY Contracts
INSERT INTO [memo].[SharedInfo](ReceiveDate, Subject, ContactId, CompanyId, CurrencyId, EveDivision, EveDepartment, EveCreatedUser, Price, PriceCzk, ExchangeRate)
SELECT ReceiveDate, Subject, ContactId, CompanyId, CurrencyId, EveDivision, EveDepartment, EveCreatedUser, Price, PriceCzk, ExchangeRate
FROM [memo].[Contracts]


-----------------------------------------------------------------------------------------
-- SPÁROVAT SharedInfoId Z TABULKY SharedInfo S TABULKOU Contracts
UPDATE [memo].[Contracts]
SET
    SharedInfoId = x.SharedInfoId
-- SELECT x.SharedInfoId
FROM
    [memo].[SharedInfo] x,
    [memo].[Contracts] y
WHERE
    x.ReceiveDate = y.ReceiveDate AND
    x.Subject = y.Subject AND
    x.ContactId = y.ContactId AND
    x.CompanyId = y.CompanyId AND
    x.EveDivision = y.EveDivision AND
    x.EveDepartment = y.EveDepartment AND
    x.EveCreatedUser = y.EveCreatedUser AND
    (x.PriceCzk = y.PriceCzk OR (ISNULL(x.PriceCzk, y.PriceCzk) IS NULL)) AND
    (x.Price = y.Price OR (ISNULL(x.Price, y.Price) IS NULL))


-----------------------------------------------------------------------------------------
-- SMAZAT NEPOTŘEBNÉ SLOUPCE Z Contracts KTERÉ JSOU NAVÁZÁNY NA SharedInfo
ALTER TABLE [memo].[Contracts]
    DROP CONSTRAINT
        [FK__memo.Contracts__memo.Company__CompanyId],
        [FK__memo.Contracts__memo.Contact__ContactId],
        [FK__memo.Contracts__memo.Currency__CurrencyId],
        [FK__memo.Contracts__memo.SharedInfo__SharedInfoId]
GO

ALTER TABLE [memo].[Contracts]
    DROP COLUMN ReceiveDate, Subject, ContactId, CompanyId, CurrencyId, EveDivision, EveDepartment, EveCreatedUser, Price, PriceCzk, ExchangeRate;
GO


-- 03.12.2020 --


-----------------------------------------------------------------------------------------
-- PRIDAT DO TABULKY Order SLOUPEC SharedInfoId
ALTER TABLE [memo].[Order]
ADD SharedInfoId INT NULL;
GO


-----------------------------------------------------------------------------------------
-- NAVÁZAT Order.SharedInfoId JAKO FOREIGN KEY NA SharedInfo.SharedInfoId
ALTER TABLE [memo].[Order]
ADD
    CONSTRAINT [FK__memo.Order__memo.SharedInfo__SharedInfoId]
    FOREIGN KEY ([SharedInfoId])
    REFERENCES [memo].[SharedInfo] (SharedInfoId);
GO

-----------------------------------------------------------------------------------------
-- VLOZIT DO TABULKY SharedInfo SLOUPCE Z TABULKY Offer
INSERT INTO [memo].[SharedInfo](ReceiveDate, Subject, ContactId, CompanyId, CurrencyId, EveDivision, EveDepartment, EveCreatedUser, Price, PriceCzk, ExchangeRate)
SELECT ReceiveDate, Subject, ContactId, CompanyId, CurrencyId, EveDivision, EveDepartment, EveCreatedUser, Price, PriceCzk, ExchangeRate
FROM [memo].[Offer]
GO

-----------------------------------------------------------------------------------------
-- SPÁROVAT SharedInfoId Z TABULKY SharedInfo S TABULKOU Offer
UPDATE [memo].[Order]
SET
    SharedInfoId = si.SharedInfoId
-- SELECT si.SharedInfoId
FROM
    [memo].[SharedInfo] si,
	[memo].[Order] r
LEFT JOIN [memo].[Offer] o
	ON r.[OfferId] = o.[OfferId]
WHERE
    si.ReceiveDate = o.ReceiveDate AND
    si.Subject = o.Subject AND
    si.ContactId = o.ContactId AND
    si.CompanyId = o.CompanyId AND
    si.EveDivision = o.EveDivision AND
    si.EveDepartment = o.EveDepartment AND
    si.EveCreatedUser = o.EveCreatedUser AND
    (si.PriceCzk = o.PriceCzk OR (ISNULL(si.PriceCzk, o.PriceCzk) IS NULL)) AND
    (si.Price = o.Price OR (ISNULL(si.Price, o.Price) IS NULL))
GO


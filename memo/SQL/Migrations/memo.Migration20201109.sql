-----------------------------------------------------------------------------------------
-- VYTVORIT TABULKU HOUR WAGES
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
    CONSTRAINT [FK__memo.HourWages__memo.Order__OrderId] FOREIGN KEY ([OrderId]) REFERENCES [memo].[Order] ([OrderId])
)
GO


-----------------------------------------------------------------------------------------
-- VZIT HOURWAGE Z ORDER A NAPLNIT HOURWAGES DLE ORDER ID, NA MISTO SUBJ DAT ???
INSERT INTO [memo].[HourWages]
    (OrderId, Subject, Cost, CostCzk)
SELECT x.OrderId, '???', x.HourWage, x.HourWage*x.ExchangeRate
FROM [memo].[Order] x;
GO


-----------------------------------------------------------------------------------------
-- VYMAZAT HOUR WAGE SLOUPEC Z ORDER
ALTER TABLE [memo].[Order] DROP COLUMN HourWage;
GO


-----------------------------------------------------------------------------------------
-- VYCISTIT HOUR WAGES Z AUDIT TABULKY
DELETE FROM [memo].[Audit]
WHERE FieldName = 'HourWages'


-----------------------------------------------------------------------------------------
-- VYTVORIT AUDIT TRIGGER PRO HOUR WAGES
IF OBJECT_ID('[memo].TR__HourWages__AUDIT', 'TR') IS NOT NULL
    DROP TRIGGER [memo].TR__HourWages__AUDIT
GO
-----------------------------------------------------------------------------------------
CREATE TRIGGER [memo].[TR__HourWages__AUDIT]
ON [memo].[HourWages]
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
SELECT @TableName = 'HourWages'

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
-- VYTVORIT TABULKU ORDERCODES
IF OBJECT_ID('memo.OrderCodes', 'U') IS NOT NULL
  DROP TABLE [memo].[OrderCodes]
GO
-----------------------------------------------------------------------------------------
CREATE TABLE [memo].[OrderCodes]
(
  [OrderCodeId] int IDENTITY(1, 1),
  [OrderId] int NOT NULL,
  [OrderCode] nvarchar(50) NOT NULL,
  CONSTRAINT [PK__OrderCodeId] PRIMARY KEY ([OrderCodeId]),
)
GO


-----------------------------------------------------------------------------------------
-- VZIT ORDERID Z ORDER A NAPLNIT TABULKU ORDERCODE HODNOTAMI DLE ORDER ID
INSERT INTO [memo].[OrderCodes]
    (OrderId, OrderCode)
SELECT x.OrderId, x.OrderCode
FROM [memo].[Order] x;
GO

--TODO
-----------------------------------------------------------------------------------------
-- VYMAZAT ORDERCODE SLOUPEC Z ORDER
ALTER TABLE [memo].[Order] DROP COLUMN OrderCode;
GO

--TODO
-----------------------------------------------------------------------------------------
-- VYCISTIT ORDER CODE Z AUDIT TABULKY
DELETE FROM [memo].[Audit]
WHERE FieldName = 'OrderCode'

--TODO
-----------------------------------------------------------------------------------------
-- VYTVORIT AUDIT TRIGGER PRO ORDERCODES
IF OBJECT_ID('[memo].TR__OrderCodes__AUDIT', 'TR') IS NOT NULL
    DROP TRIGGER [memo].TR__OrderCodes__AUDIT
GO
-----------------------------------------------------------------------------------------
CREATE TRIGGER [memo].[TR__OrderCodes__AUDIT]
ON [memo].[OrderCodes]
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
SELECT @TableName = 'OrderCodes'

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


-----------------------------------------------------------------------------------------
-- VYTVORIT TABULKU Contracts
IF OBJECT_ID('memo.Contracts', 'U') IS NOT NULL
  DROP TABLE [memo].[Contracts]
GO
-----------------------------------------------------------------------------------------
CREATE TABLE [memo].[Contracts]
(
    [ContractsId] int PRIMARY KEY IDENTITY(1, 1),
    [ContractName] nvarchar(50) NOT NULL,
    [ReceiveDate] date NOT NULL,
    [Subject] nvarchar(max),
    [EveDivision] nvarchar(50) NOT NULL,
    [EveDepartment] nvarchar(50) NOT NULL,
    [EveCreatedUser] nvarchar(50) NOT NULL,
    [ContactId] int NOT NULL,
    [CompanyId] int NOT NULL,
    [Price] int,
    [PriceCzk] int,
    [CurrencyId] int,
    [ExchangeRate] decimal(18,3),
    [Notes] nvarchar(max),
    [Active] bit DEFAULT 1,
    [CreatedBy] nvarchar(50),
    [ModifiedBy] nvarchar(50),
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
ALTER COLUMN FromType CHAR(1) NOT NULL;
GO


-----------------------------------------------------------------------------------------
-- PRIDAT Order SLOUPEC ContractId
ALTER TABLE [memo].[Order]
ADD ContractId INT NULL;
GO
-- ----------------------------------------------------------------------------------------------------------------------
-- Audit: Update rows of 'FieldName' column from 'CreateDate' to 'CreatedDate'
-- ----------------------------------------------------------------------------------------------------------------------
UPDATE [memo].[Audit]
SET [memo].[Audit].FieldName = 'CreatedDate'
-- SELECT *
FROM [memo].[Audit]
WHERE FieldName = 'CreateDate' OR FieldName = 'Created' AND
    (
        TableName = 'Company' OR
        TableName = 'Contact' OR
        TableName = 'Offer' OR
        TableName = 'Order' OR
        TableName = 'BugReport'
    )
GO

UPDATE [memo].[Audit]
SET [memo].[Audit].FieldName = 'CreatedBy'
-- SELECT *
FROM [memo].[Audit]
WHERE FieldName = 'Username' AND
    (
        TableName = 'Company' OR
        TableName = 'Contact' OR
        TableName = 'Offer' OR
        TableName = 'Order' OR
        TableName = 'BugReport'
    )
GO

-- ----------------------------------------------------------------------------------------------------------------------
-- BugReport: Rename column names, Add two more, Fill them with default values
-- ----------------------------------------------------------------------------------------------------------------------
EXEC sp_rename 'memo.BugReport.Username', 'CreatedBy', 'COLUMN';
GO

EXEC sp_rename 'memo.BugReport.Created', 'CreatedDate', 'COLUMN';
GO

IF COL_LENGTH ('memo.BugReport', 'ModifiedBy') IS NULL
BEGIN
    ALTER TABLE [memo].[BugReport]
    ADD ModifiedBy NVARCHAR(50) NULL
END;
GO

IF COL_LENGTH ('memo.BugReport', 'ModifiedDate') IS NULL
BEGIN
    ALTER TABLE [memo].[BugReport]
    ADD ModifiedDate DATETIME NULL
END;
GO

UPDATE [memo].[BugReport]
SET [memo].[BugReport].ModifiedBy = [memo].[BugReport].CreatedBy
FROM [memo].[BugReport]
GO

UPDATE [memo].[BugReport]
SET [memo].[BugReport].ModifiedDate = [memo].[BugReport].CreatedDate
FROM [memo].[BugReport]
GO

-- ----------------------------------------------------------------------------------------------------------------------
-- Offer: Rename column names, Add, Fill them with default values
-- ----------------------------------------------------------------------------------------------------------------------
EXEC sp_rename 'memo.Offer.CreateDate', 'CreatedDate', 'COLUMN';
GO

IF COL_LENGTH ('memo.Offer', 'CreatedBy') IS NULL
BEGIN
    ALTER TABLE [memo].[Offer]
    ADD CreatedBy NVARCHAR(50) NULL
END;
GO

IF COL_LENGTH ('memo.Offer', 'ModifiedBy') IS NULL
BEGIN
    ALTER TABLE [memo].[Offer]
    ADD ModifiedBy NVARCHAR(50) NULL
END;
GO

IF COL_LENGTH ('memo.Offer', 'ModifiedDate') IS NULL
BEGIN
    ALTER TABLE [memo].[Offer]
    ADD ModifiedDate DATETIME NULL
END;
GO

UPDATE [memo].[Offer]
SET [memo].[Offer].CreatedBy = 'jverner@evektor.cz'
FROM [memo].[Offer];
GO

UPDATE [memo].[Offer]
SET [memo].[Offer].ModifiedBy = [memo].[Offer].CreatedBy
FROM [memo].[Offer];
GO

UPDATE [memo].[Offer]
SET [memo].[Offer].ModifiedDate = [memo].[Offer].CreatedDate
FROM [memo].[Offer];
GO

-- ----------------------------------------------------------------------------------------------------------------------
-- Contact: Rename column names, Add, Fill them with default values
-- ----------------------------------------------------------------------------------------------------------------------
EXEC sp_rename 'memo.Contact.CreateDate', 'CreatedDate', 'COLUMN';
GO

IF COL_LENGTH ('memo.Contact', 'CreatedBy') IS NULL
BEGIN
    ALTER TABLE [memo].[Contact]
    ADD CreatedBy NVARCHAR(50) NULL
END;
GO

IF COL_LENGTH ('memo.Contact', 'ModifiedBy') IS NULL
BEGIN
    ALTER TABLE [memo].[Contact]
    ADD ModifiedBy NVARCHAR(50) NULL
END;
GO

IF COL_LENGTH ('memo.Contact', 'ModifiedDate') IS NULL
BEGIN
    ALTER TABLE [memo].[Contact]
    ADD ModifiedDate DATETIME NULL
END;
GO

UPDATE [memo].[Contact]
SET [memo].[Contact].CreatedBy = 'jverner@evektor.cz'
FROM [memo].[Contact];
GO

UPDATE [memo].[Contact]
SET [memo].[Contact].ModifiedBy = [memo].[Contact].CreatedBy
FROM [memo].[Contact];
GO

UPDATE [memo].[Contact]
SET [memo].[Contact].ModifiedDate = [memo].[Contact].CreatedDate
FROM [memo].[Contact];
GO

ALTER TABLE [memo].[Contact]
ALTER COLUMN CreatedDate DATETIME NULL;
GO

-- ----------------------------------------------------------------------------------------------------------------------
-- Company: Rename column names, Add, Fill them with default values
-- ----------------------------------------------------------------------------------------------------------------------
EXEC sp_rename 'memo.Company.CreateDate', 'CreatedDate', 'COLUMN';
GO

ALTER TABLE [memo].[Company]
ALTER COLUMN CreatedDate DATETIME NULL;
GO

IF COL_LENGTH ('memo.Company', 'CreatedBy') IS NULL
BEGIN
    ALTER TABLE [memo].[Company]
    ADD CreatedBy NVARCHAR(50) NULL
END;
GO

IF COL_LENGTH ('memo.Company', 'ModifiedBy') IS NULL
BEGIN
    ALTER TABLE [memo].[Company]
    ADD ModifiedBy NVARCHAR(50) NULL
END;
GO

IF COL_LENGTH ('memo.Company', 'ModifiedDate') IS NULL
BEGIN
    ALTER TABLE [memo].[Company]
    ADD ModifiedDate DATETIME NULL
END;
GO

UPDATE [memo].[Company]
SET [memo].[Company].CreatedBy = 'jverner@evektor.cz'
FROM [memo].[Company];
GO

UPDATE [memo].[Company]
SET [memo].[Company].ModifiedBy = [memo].[Company].CreatedBy
FROM [memo].[Company];
GO

UPDATE [memo].[Company]
SET [memo].[Company].ModifiedDate = [memo].[Company].CreatedDate
FROM [memo].[Company];
GO

-- ----------------------------------------------------------------------------------------------------------------------
-- Order: Rename column names, Add, Fill them with default values
-- ----------------------------------------------------------------------------------------------------------------------
EXEC sp_rename 'memo.Order.CreateDate', 'CreatedDate', 'COLUMN';
GO

EXEC sp_rename 'memo.Order.Username', 'CreatedBy', 'COLUMN';
GO

ALTER TABLE [memo].[Order]
ALTER COLUMN CreatedBy NVARCHAR(50);
GO

UPDATE [memo].[Order]
SET [memo].[Order].CreatedBy = 'jverner@evektor.cz'
select *
FROM [memo].[Order]
WHERE CreatedBy IS NULL or CreatedBy = '';
GO

IF COL_LENGTH ('memo.Order', 'ModifiedBy') IS NULL
BEGIN
    ALTER TABLE [memo].[Order]
    ADD ModifiedBy NVARCHAR(50) NULL
END;
GO

UPDATE [memo].[Order]
SET [memo].[Order].ModifiedBy = [memo].[Order].CreatedBy
FROM [memo].[Order];
GO

IF COL_LENGTH ('memo.Order', 'ModifiedDate') IS NULL
BEGIN
    ALTER TABLE [memo].[Order]
    ADD ModifiedDate DATETIME NULL
END;
GO

UPDATE [memo].[Order]
SET [memo].[Order].ModifiedDate = [memo].[Order].CreatedDate
FROM [memo].[Order];
GO

ALTER TABLE [memo].[Order]
ALTER COLUMN CreatedDate DATETIME NULL;
GO


--/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
--/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
-- 03.11.2020

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
    CONSTRAINT [FK__memo.HourWages__memo.Order__OrderId] FOREIGN KEY ([OrderId])
    REFERENCES [memo].[Order] ([OrderId])
)
GO


-----------------------------------------------------------------------------------------
-- VZIT HOURWAGE Z ORDER A NAPLNIT HOURWAGES DLE ORDER ID, NA MISTO SUBJ DAT ???
INSERT INTO [memo].[HourWages]
    (OrderId, Subject, Cost, CostCzk)
SELECT x.OrderId, '???', x.HourWage, x.HourWage*x.ExchangeRate
FROM [memo].[Order] x;


-----------------------------------------------------------------------------------------
-- VYMAZAT HOUR WAGE SLOUPEC Z ORDER
ALTER TABLE [memo].[Order] DROP COLUMN HourWage;


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


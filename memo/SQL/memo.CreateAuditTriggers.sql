-- AUDITING TRIGGER for each table I want to use that

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-----------------------------------------------------------------------------------------
IF OBJECT_ID('[memo].TR__Company__AUDIT', 'TR') IS NOT NULL
    DROP TRIGGER [memo].TR__Company__AUDIT
GO
-----------------------------------------------------------------------------------------
CREATE TRIGGER [memo].[TR__Company__AUDIT]
ON [memo].[Company]
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
SELECT @TableName = 'Company'

SELECT @UserName = SUSER_SNAME(),
       @UpdateDate = CONVERT(NVARCHAR(30), GETDATE(), 126)

SELECT @UpdateBy = memo.GetUserContext()

-- Action
IF EXISTS (SELECT * FROM INSERTED)
  IF EXISTS (SELECT * FROM DELETED)
    SELECT @Type = 'U'
  ELSE
    SELECT @Type = 'I'
ELSE
  SELECT @Type = 'D'

-- Get list of columns
SELECT * INTO #ins
FROM INSERTED

SELECT * INTO #del
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
IF OBJECT_ID('[memo].TR__Contact__AUDIT', 'TR') IS NOT NULL
    DROP TRIGGER [memo].TR__Contact__AUDIT
GO
-----------------------------------------------------------------------------------------
CREATE TRIGGER [memo].[TR__Contact__AUDIT]
ON [memo].[Contact]
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
SELECT @TableName = 'Contact'

SELECT @UserName = SUSER_SNAME(),
       @UpdateDate = CONVERT(NVARCHAR(30), GETDATE(), 126)

SELECT @UpdateBy = memo.GetUserContext()

-- Action
IF EXISTS (SELECT * FROM INSERTED)
  IF EXISTS (SELECT * FROM DELETED)
    SELECT @Type = 'U'
  ELSE
    SELECT @Type = 'I'
ELSE
  SELECT @Type = 'D'

-- Get list of columns
SELECT * INTO #ins
FROM INSERTED

SELECT * INTO #del
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
IF OBJECT_ID('[memo].TR__Invoice__AUDIT', 'TR') IS NOT NULL
    DROP TRIGGER [memo].TR__Invoice__AUDIT
GO
-----------------------------------------------------------------------------------------
CREATE TRIGGER [memo].[TR__Invoice__AUDIT]
ON [memo].[Invoice]
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
SELECT @TableName = 'Invoice'

SELECT @UserName = SUSER_SNAME(),
       @UpdateDate = CONVERT(NVARCHAR(30), GETDATE(), 126)

SELECT @UpdateBy = memo.GetUserContext()

-- Action
IF EXISTS (SELECT * FROM INSERTED)
  IF EXISTS (SELECT * FROM DELETED)
    SELECT @Type = 'U'
  ELSE
    SELECT @Type = 'I'
ELSE
  SELECT @Type = 'D'

-- Get list of columns
SELECT * INTO #ins
FROM INSERTED

SELECT * INTO #del
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
IF OBJECT_ID('[memo].TR__BugReport__AUDIT', 'TR') IS NOT NULL
    DROP TRIGGER [memo].TR__BugReport__AUDIT
GO
-----------------------------------------------------------------------------------------
CREATE TRIGGER [memo].[TR__BugReport__AUDIT]
ON [memo].[BugReport]
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
SELECT @TableName = 'BugReport'

SELECT @UserName = SUSER_SNAME(),
       @UpdateDate = CONVERT(NVARCHAR(30), GETDATE(), 126)

SELECT @UpdateBy = memo.GetUserContext()

-- Action
IF EXISTS (SELECT * FROM INSERTED)
  IF EXISTS (SELECT * FROM DELETED)
    SELECT @Type = 'U'
  ELSE
    SELECT @Type = 'I'
ELSE
  SELECT @Type = 'D'

-- Get list of columns
SELECT * INTO #ins
FROM INSERTED

SELECT * INTO #del
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
IF OBJECT_ID('[memo].TR__Order__AUDIT', 'TR') IS NOT NULL
    DROP TRIGGER [memo].TR__Order__AUDIT
GO
-----------------------------------------------------------------------------------------
CREATE TRIGGER [memo].[TR__Order__AUDIT]
ON [memo].[Order]
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
SELECT @TableName = 'Order'

SELECT @UserName = SUSER_SNAME(),
       @UpdateDate = CONVERT(NVARCHAR(30), GETDATE(), 126)

SELECT @UpdateBy = memo.GetUserContext()

-- Action
IF EXISTS (SELECT * FROM INSERTED)
  IF EXISTS (SELECT * FROM DELETED)
    SELECT @Type = 'U'
  ELSE
    SELECT @Type = 'I'
ELSE
  SELECT @Type = 'D'

-- Get list of columns
SELECT * INTO #ins
FROM INSERTED

SELECT * INTO #del
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
IF OBJECT_ID('[memo].TR__Offer__AUDIT', 'TR') IS NOT NULL
    DROP TRIGGER [memo].TR__Offer__AUDIT
GO
-----------------------------------------------------------------------------------------
CREATE TRIGGER [memo].[TR__Offer__AUDIT]
ON [memo].[Offer]
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
SELECT @TableName = 'Offer'

SELECT @UserName = SUSER_SNAME(),
       @UpdateDate = CONVERT(NVARCHAR(30), GETDATE(), 126)

SELECT @UpdateBy = memo.GetUserContext()

-- Action
IF EXISTS (SELECT * FROM INSERTED)
  IF EXISTS (SELECT * FROM DELETED)
    SELECT @Type = 'U'
  ELSE
    SELECT @Type = 'I'
ELSE
  SELECT @Type = 'D'

-- Get list of columns
SELECT * INTO #ins
FROM INSERTED

SELECT * INTO #del
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
IF OBJECT_ID('[memo].TR__OtherCost__AUDIT', 'TR') IS NOT NULL
    DROP TRIGGER [memo].TR__OtherCost__AUDIT
GO
-----------------------------------------------------------------------------------------
CREATE TRIGGER [memo].[TR__OtherCost__AUDIT]
ON [memo].[OtherCost]
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
SELECT @TableName = 'OtherCost'

SELECT @UserName = SUSER_SNAME(),
       @UpdateDate = CONVERT(NVARCHAR(30), GETDATE(), 126)

SELECT @UpdateBy = memo.GetUserContext()

-- Action
IF EXISTS (SELECT * FROM INSERTED)
  IF EXISTS (SELECT * FROM DELETED)
    SELECT @Type = 'U'
  ELSE
    SELECT @Type = 'I'
ELSE
  SELECT @Type = 'D'

-- Get list of columns
SELECT * INTO #ins
FROM INSERTED

SELECT * INTO #del
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
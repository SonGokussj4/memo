-- This sql query should create all tables and it's relations

CREATE TABLE [memo].[Company] (
  [CompanyId] int PRIMARY KEY IDENTITY(1, 1),
  [Name] nvarchar(50),
  [City] nvarchar(50),
  [Address] nvarchar(50),
  [Phone] nvarchar(50),
  [Web] nvarchar(50),
  [InvoiceDueDays] int,
  [Notes] nvarchar(max),
  [Active] bit DEFAULT 1,
  [CreateDate] date DEFAULT GETDATE()
)
GO


-----------------------------------------------------------------------------------------
CREATE TABLE [memo].[Contact] (
  [ContactId] int PRIMARY KEY IDENTITY(1, 1),
  [PersonName] nvarchar(50),
  [PersonLastName] nvarchar(50),
  [PersonTitle] nvarchar(20),
  [CompanyId] int DEFAULT 1,
  [Department] nvarchar(50),
  [Phone] nvarchar(50),
  [Email] nvarchar(255),
  [Notes] nvarchar(max),
  [Active] bit DEFAULT 1,
  [CreateDate] date DEFAULT GETDATE()
)
GO


-----------------------------------------------------------------------------------------
CREATE TABLE [memo].[OfferStatus] (
  [OfferStatusId] int PRIMARY KEY IDENTITY(1, 1),
  [Name] nvarchar(20)
)
GO


-----------------------------------------------------------------------------------------
CREATE TABLE [memo].[Currency] (
  [CurrencyId] int PRIMARY KEY IDENTITY(1, 1),
  [Name] nvarchar(10),
  [CultureCode] nvarchar(10)
)
GO


-----------------------------------------------------------------------------------------
CREATE TABLE [memo].[Offer] (
  [OfferId] int PRIMARY KEY IDENTITY(1, 1),
  [OfferName] nvarchar(50) UNIQUE,
  [ReceiveDate] date,
  [SentDate] date,
  [Subject] nvarchar(max),
  [ContactId] int,
  [CompanyId] int,
  [EveDivision] nvarchar(50),
  [EveDepartment] nvarchar(50),
  [EveCreatedUser] nvarchar(50),
  [Price] int,
  [CurrencyId] int,
  [ExchangeRate] decimal(18,3),
  [PriceCzk] int,
  [OfferStatusId] int,
  [LostReason] nvarchar(max),
  [EstimatedFinishDate] date,
  [Notes] nvarchar(max),
  [Active] bit DEFAULT 1,
  [CreateDate] date DEFAULT GETDATE(),
  CONSTRAINT [FK_memo.Offer_memo_Contact_ContactId] FOREIGN KEY ([ContactId])
    REFERENCES [memo].[Contact] ([ContactId]),
  CONSTRAINT [FK_memo.Offer_memo_Company_CompanyId] FOREIGN KEY ([CompanyId])
    REFERENCES [memo].[Company] ([CompanyId]),
  CONSTRAINT [FK_memo.Offer_memo_OfferStatus_OfferStatusId] FOREIGN KEY ([OfferStatusId])
    REFERENCES [memo].[OfferStatus] ([OfferStatusId]),
  CONSTRAINT [FK_memo.Offer_memo_Currency_CurrencyId] FOREIGN KEY ([CurrencyId])
    REFERENCES [memo].[Currency] ([CurrencyId])  -- ON DELETE CASCADE
)
GO


-----------------------------------------------------------------------------------------
CREATE TABLE [memo].[Order] (
  [OrderId] int PRIMARY KEY IDENTITY(1, 1),
  [OfferId] int,
  [OrderName] nvarchar(50) UNIQUE,
  [PriceFinal] int,
  [PriceDiscount] int,
  [OrderCode] nvarchar(50),
  [EveContactName] nvarchar(50),
  [HourWage] float,
  [TotalHours] int,
  [ExchangeRate] decimal(18,3),
  [PriceFinalCzk] int,
  [Username] nvarchar(30),
  [Notes] nvarchar(max),
  [Active] bit DEFAULT 1,
  [CreateDate] date DEFAULT GETDATE()
  CONSTRAINT [FK_memo.Order_memo_Offer_OfferId] FOREIGN KEY ([OfferId])
    REFERENCES [memo].[Offer] ([OfferId])
)
GO


-----------------------------------------------------------------------------------------
CREATE TABLE [memo].[Invoice] (
  [InvoiceId] int PRIMARY KEY IDENTITY(1, 1),
  [OrderId] int,
  [InvoiceDueDate] date,
  [InvoiceIssueDate] date,
  [Cost] decimal(18,3),
  [CostCzk] decimal(18,3),
  [DeliveryNote] nvarchar(255),
  CONSTRAINT [FK__memo.Invoice__memo.Order__OrderId] FOREIGN KEY ([OrderId])
    REFERENCES [memo].[Order] ([OrderId]) ON DELETE CASCADE
)
GO


-----------------------------------------------------------------------------------------
CREATE TABLE [memo].[OtherCost]
(
  [OtherCostId] int IDENTITY(1, 1),
  [OrderId] int,
  [Subject] nvarchar(max),
  [Cost] decimal(18,3),
  [CostCzk] decimal(18,3),
  CONSTRAINT [PK__OtherCostId] PRIMARY KEY ([OtherCostId]),
  CONSTRAINT [FK__memo.OtherCost__memo.Order__OrderId] FOREIGN KEY ([OrderId])
    REFERENCES [memo].[Order] ([OrderId])
)
GO





-- AUDITING INSERT, MODIFY, DELETE
------------------------------------
IF NOT EXISTS
  (
    SELECT *
    FROM sysobjects
    WHERE id = OBJECT_ID(N'[dbo].[Audit]')
    AND OBJECTPROPERTY(id, N'IsUserTable') = 1
  )
CREATE TABLE [dbo].[Audit]
  (
    Type CHAR(1),
    TableName VARCHAR(128),
    PK VARCHAR(1000),
    FieldName VARCHAR(128),
    OldValue VARCHAR(1000),
    NewValue VARCHAR(1000),
    UpdateDate datetime,
    UserName VARCHAR(128)
  )
GO


-- AUDITING TRIGGER for each table I want to use that
---------------------------------------------------------
CREATE TRIGGER [memo].[TR__Company__AUDIT]
ON [memo].[Company]
FOR UPDATE
AS
DECLARE @bit            INT,
        @field          INT,
        @maxfield       INT,
        @char           INT,
        @fieldname      VARCHAR(128),
        @TableName      VARCHAR(128),
        @PKCols         VARCHAR(1000),
        @sql            VARCHAR(2000),
        @UpdateDate     VARCHAR(21),
        @UserName       VARCHAR(128),
        @Type           CHAR(1),
        @PKSelect       VARCHAR(1000)

-- You will need to change @TableName to match the table to be audited.
-- Here we made GUESTS for your example.
SELECT @TableName = 'Company'

SELECT @UserName = SYSTEM_USER,
       @UpdateDate = CONVERT(NVARCHAR(30), GETDATE(), 126)

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
WHERE  pk.TABLE_NAME = @TableName
  AND CONSTRAINT_TYPE = 'PRIMARY KEY'
  AND c.TABLE_NAME = pk.TABLE_NAME
  AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

-- Get primary key select for insert
SELECT @PKSelect = COALESCE(@PKSelect + '+', '') + '''<[' + COLUMN_NAME + ']=''+convert(varchar(100), coalesce(i.[' + COLUMN_NAME + '],d.[' + COLUMN_NAME + ']))+''>'''
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk, INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
WHERE  pk.TABLE_NAME = @TableName
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
WHERE  TABLE_NAME = @TableName

WHILE @field < @maxfield
BEGIN
  SELECT @field = MIN(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + @TableName), COLUMN_NAME, 'ColumnID' ))
  FROM INFORMATION_SCHEMA.COLUMNS
  WHERE  TABLE_NAME = @TableName
    AND COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + @TableName), COLUMN_NAME, 'ColumnID' ) > @field
  SELECT @bit = (@field - 1)% 8 + 1
  SELECT @bit = POWER(2, @bit - 1)
  SELECT @char = ((@field - 1) / 8) + 1
  IF SUBSTRING(COLUMNS_UPDATED(), @char, 1) & @bit > 0
  OR @Type
  IN ('I', 'D')
  BEGIN
    SELECT @fieldname = COLUMN_NAME
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE  TABLE_NAME = @TableName
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
           UserName
           )
           select ''' + @Type + ''','''
                      + @TableName + ''',' + @PKSelect
                      + ',''' + @fieldname + ''''
                      + ',convert(varchar(1000),d.' + @fieldname + ')'
                      + ',convert(varchar(1000),i.' + @fieldname + ')'
                      + ',''' + @UpdateDate + ''''
                      + ',''' + @UserName + ''''
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




-- ------------------------------------------------------------------------------------------------------------

-- DECLARE @MyList TABLE (Value NVARCHAR(50))
-- INSERT INTO @MyList VALUES ('Company')
-- INSERT INTO @MyList VALUES ('Contact')
-- INSERT INTO @MyList VALUES ('Invoice')
-- INSERT INTO @MyList VALUES ('Offer')
-- INSERT INTO @MyList VALUES ('Order')

-- DECLARE @value VARCHAR(50)

-- DECLARE db_cursor CURSOR FOR
-- SELECT Value FROM @MyList
-- OPEN db_cursor
-- FETCH NEXT FROM db_cursor INTO @value

-- WHILE @@FETCH_STATUS = 0
-- BEGIN
--        PRINT @value


--         -- AUDITING TRIGGER for each table I want to use that
--         ---------------------------------------------------------
--         CREATE TRIGGER [memo].[TR__Company__AUDIT]
--         ON [memo].[Company]
--         FOR UPDATE
--         AS
--         DECLARE @bit            INT,
--                 @field          INT,
--                 @maxfield       INT,
--                 @char           INT,
--                 @fieldname      VARCHAR(128),
--                 @TableName      VARCHAR(128),
--                 @PKCols         VARCHAR(1000),
--                 @sql            VARCHAR(2000),
--                 @UpdateDate     VARCHAR(21),
--                 @UserName       VARCHAR(128),
--                 @Type           CHAR(1),
--                 @PKSelect       VARCHAR(1000)

--         -- You will need to change @TableName to match the table to be audited.
--         -- Here we made GUESTS for your example.
--         SELECT @TableName = 'Company'

--         SELECT @UserName = SYSTEM_USER,
--               @UpdateDate = CONVERT(NVARCHAR(30), GETDATE(), 126)

--         -- Action
--         IF EXISTS (SELECT * FROM INSERTED)
--           IF EXISTS (SELECT * FROM DELETED)
--             SELECT @Type = 'U'
--           ELSE
--             SELECT @Type = 'I'
--         ELSE
--           SELECT @Type = 'D'

--         -- Get list of columns
--         SELECT * INTO #ins
--         FROM INSERTED

--         SELECT * INTO #del
--         FROM DELETED

--         -- Get primary key columns for full outer join
--         SELECT @PKCols = COALESCE(@PKCols + ' and', ' on') + ' i.[' + c.COLUMN_NAME + '] = d.[' + c.COLUMN_NAME + ']'
--         FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk, INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
--         WHERE  pk.TABLE_NAME = @TableName
--           AND CONSTRAINT_TYPE = 'PRIMARY KEY'
--           AND c.TABLE_NAME = pk.TABLE_NAME
--           AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

--         -- Get primary key select for insert
--         SELECT @PKSelect = COALESCE(@PKSelect + '+', '') + '''<[' + COLUMN_NAME + ']=''+convert(varchar(100), coalesce(i.[' + COLUMN_NAME + '],d.[' + COLUMN_NAME + ']))+''>'''
--         FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk, INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
--         WHERE  pk.TABLE_NAME = @TableName
--           AND CONSTRAINT_TYPE = 'PRIMARY KEY'
--           AND c.TABLE_NAME = pk.TABLE_NAME
--           AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

--         IF @PKCols IS NULL
--         BEGIN
--           RAISERROR('no PK on table %s', 16, -1, @TableName)
--           RETURN
--         END

--         -- FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName
--         -- @maxfield = MAX(COLUMN_NAME)
--         SELECT @field = 0, @maxfield = MAX(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + @TableName), COLUMN_NAME, 'ColumnID'))
--         FROM INFORMATION_SCHEMA.COLUMNS
--         WHERE  TABLE_NAME = @TableName

--         WHILE @field < @maxfield
--         BEGIN
--           SELECT @field = MIN(COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + @TableName), COLUMN_NAME, 'ColumnID' ))
--           FROM INFORMATION_SCHEMA.COLUMNS
--           WHERE  TABLE_NAME = @TableName
--             AND COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + @TableName), COLUMN_NAME, 'ColumnID' ) > @field
--           SELECT @bit = (@field - 1)% 8 + 1
--           SELECT @bit = POWER(2, @bit - 1)
--           SELECT @char = ((@field - 1) / 8) + 1
--           IF SUBSTRING(COLUMNS_UPDATED(), @char, 1) & @bit > 0
--           OR @Type
--           IN ('I', 'D')
--           BEGIN
--             SELECT @fieldname = COLUMN_NAME
--             FROM INFORMATION_SCHEMA.COLUMNS
--             WHERE  TABLE_NAME = @TableName
--               AND COLUMNPROPERTY(OBJECT_ID(TABLE_SCHEMA + '.' + @TableName), COLUMN_NAME, 'ColumnID') = @field
--             SELECT @sql ='
--                   insert into memo.Audit (
--                   Type,
--                   TableName,
--                   PK,
--                   FieldName,
--                   OldValue,
--                   NewValue,
--                   UpdateDate,
--                   UserName
--                   )
--                   select ''' + @Type + ''','''
--                               + @TableName + ''',' + @PKSelect
--                               + ',''' + @fieldname + ''''
--                               + ',convert(varchar(1000),d.' + @fieldname + ')'
--                               + ',convert(varchar(1000),i.' + @fieldname + ')'
--                               + ',''' + @UpdateDate + ''''
--                               + ',''' + @UserName + ''''
--                               + ' from #ins i full outer join #del d'
--                               + @PKCols
--                               + ' where i.' + @fieldname + ' <> d.' + @fieldname
--                               + ' or (i.' + @fieldname + ' is null and  d.'
--                               + @fieldname
--                               + ' is not null)'
--                               + ' or (i.' + @fieldname + ' is not null and  d.'
--                               + @fieldname
--                               + ' is null)'
--             EXEC (@sql)
--           END
--         END






--        -- PUT YOUR LOGIC HERE
--        -- MAKE USE OR VARIABLE @value wich is Data1, Data2, etc...

--        FETCH NEXT FROM db_cursor INTO @value
-- END

-- CLOSE db_cursor
-- DEALLOCATE db_cursor

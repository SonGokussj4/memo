-- This sql query should create all tables and it's relations

CREATE TABLE [memo].[Company] (
  [CompanyId] int PRIMARY KEY IDENTITY(1, 1),
  [Name] nvarchar(50),
  [City] nvarchar(50),
  [Address] nvarchar(50),
  [Phone] nvarchar(50),
  [Web] nvarchar(50),
  [CreateDate] date DEFAULT GETDATE(),
  [InvoiceDueDays] int,
  [Notes] ntext,
  [Active] bit DEFAULT 1
)
GO

CREATE TABLE [memo].[Contact] (
  [ContactId] int PRIMARY KEY IDENTITY(1, 1),
  [PersonName] nvarchar(50),
  [PersonLastName] nvarchar(50),
  [PersonTitle] nvarchar(20),
  [CompanyId] int DEFAULT 1,
  [Department] nvarchar(50),
  [Phone] nvarchar(50),
  [Email] nvarchar(255),
  [Notes] ntext,
  [CreateDate] date DEFAULT GETDATE(),
  [Active] bit DEFAULT 1
)
GO

CREATE TABLE [memo].[Offer] (
  [OfferId] int PRIMARY KEY IDENTITY(1, 1),
  [OfferName] nvarchar(50) UNIQUE,
  [ReceiveDate] date,
  [SentDate] date,
  [Subject] ntext,
  [ContactId] int,
  [CompanyId] int,
  [EveDivision] nvarchar(50) NOT NULL CHECK ([EveDivision] IN ('AD', 'ED')),
  [EveDepartment] nvarchar(50),
  [EveCreatedUser] nvarchar(50),
  [Price] int,
  [CurrencyId] int,
  [ExchangeRate] decimal(18,3),
  [PriceCzk] int,
  [OfferStatusId] int,
  [Notes] ntext,
  [LostReason] ntext,
  [CreateDate] date DEFAULT GETDATE(),
  [Active] bit DEFAULT 1
)
GO

ALTER TABLE [memo].[Offer] ADD FOREIGN KEY ([ContactId]) REFERENCES [memo].[Contact] ([ContactId])
GO

ALTER TABLE [memo].[Offer] ADD FOREIGN KEY ([CompanyId]) REFERENCES [memo].[Company] ([CompanyId])
GO

ALTER TABLE [memo].[Offer] ADD FOREIGN KEY ([OfferStatusId]) REFERENCES [memo].[OfferStatus] ([OfferStatusId])
GO

ALTER TABLE [memo].[Offer] ADD FOREIGN KEY ([CurrencyId]) REFERENCES [memo].[Currency] ([CurrencyId])
GO

--ALTER TABLE [memo].[Offer]
--  ADD CONSTRAINT UQ_OfferName UNIQUE (OfferName);

CREATE TABLE [memo].[Invoice] (
  [InvoiceId] int PRIMARY KEY IDENTITY(1, 1),
  [OrderId] int,
  [InvoiceDueDate] date,
  [InvoiceIssueDate] date,
  [Cost] decimal(18,3),
  [CostCzk] decimal(18,3)
)
GO
ALTER TABLE [memo].[Invoice] ADD FOREIGN KEY ([OrderId]) REFERENCES [memo].[Order] ([OrderId])
ON DELETE CASCADE
GO

CREATE TABLE [memo].[OtherCost]
(
  [OtherCostId] int PRIMARY KEY IDENTITY(1, 1),
  [OrderId] int,
  [Subject] ntext,
  [Cost] decimal(18,3),
  [CostCzk] decimal(18,3)
)
GO
ALTER TABLE [memo].[OtherCost] ADD FOREIGN KEY ([OrderId]) REFERENCES [memo].[Order] ([OrderId])
ON DELETE CASCADE
GO


-- ON DELETE CASCADE
-----------------------
-- ALTER TABLE [memo].[OtherCost]  WITH CHECK ADD  CONSTRAINT [FK__OtherCost__Order__3429BB53] FOREIGN KEY([OrderId])
-- REFERENCES [memo].[Order] ([OrderId])
-- ON DELETE CASCADE
-- GO
-- ALTER TABLE [memo].[OtherCost] CHECK CONSTRAINT [FK__OtherCost__Order__3429BB53]
-- GO

CREATE TABLE [memo].[OfferStatus] (
  [OfferStatusId] int PRIMARY KEY IDENTITY(1, 1),
  [Name] nvarchar(20)
)
GO

CREATE TABLE [memo].[Order] (
  [OrderId] int PRIMARY KEY IDENTITY(1, 1),
  [OfferId] int,
  [OrderName] nvarchar(50) UNIQUE,
  [PriceFinal] int,
  [PriceDiscount] int,
  [OrderCode] nvarchar(50),
  [EveContactName] nvarchar(50),
  [BillOfDelivery] nvarchar(255),
  [HourWage] float,
  [TotalHours] int,
  [ExchangeRate] decimal(18,3),
  [PriceFinalCzk] int,
  [Notes] ntext,
  [CreateDate] date DEFAULT GETDATE(),
  [Active] bit DEFAULT 1,
  [Username] nvarchar(30)
)
GO

ALTER TABLE [memo].[Order] ADD FOREIGN KEY ([ContactId]) REFERENCES [memo].[Contact] ([ContactId])
GO

--ALTER TABLE [memo].[Order]
--  ADD CONSTRAINT UQ_OrderName UNIQUE (OrderName);

CREATE TABLE [memo].[Currency] (
  [CurrencyId] int PRIMARY KEY IDENTITY(1, 1),
  [Name] nvarchar(10),
  [CultureCode] nvarchar(10)
)
GO

-- ALTER TABLE [memo].[Order] ADD FOREIGN KEY ([OfferId]) REFERENCES [memo].[Offer] ([OfferId])
-- GO


EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'EVE_qui_2020_003_SKODA_EKX_',
@level0type = N'Schema', @level0name = 'memo',
@level1type = N'Table',  @level1name = 'Offer',
@level2type = N'Column', @level2name = 'OfferName';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'Ongoing, Won, Lost',
@level0type = N'Schema', @level0name = 'memo',
@level1type = N'Table',  @level1name = 'OfferStatus',
@level2type = N'Column', @level2name = 'Status';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'EVE-Quo/2020-003',
@level0type = N'Schema', @level0name = 'memo',
@level1type = N'Table',  @level1name = 'Order',
@level2type = N'Column', @level2name = 'OrderName';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'CZK, EUR, USD',
@level0type = N'Schema', @level0name = 'memo',
@level1type = N'Table',  @level1name = 'Currency',
@level2type = N'Column', @level2name = 'Name';
GO

-- ALTER TABLE [memo].[Company] ADD CONSTRAINT DF_Company DEFAULT GETDATE() FOR CreateDate
-- GO

-- ALTER TABLE [memo].[Contact] ADD CONSTRAINT DF_Contact DEFAULT GETDATE() FOR CreateDate
-- GO

-- ALTER TABLE [memo].[Offer] ADD CONSTRAINT DF_Offer DEFAULT GETDATE() FOR CreateDate
-- GO

-- ALTER TABLE [memo].[Order] ADD CONSTRAINT DF_Order DEFAULT GETDATE() FOR CreateDate
-- GO



-- AUDITING INSERT, MODIFY, DELETE
------------------------------------
IF NOT EXISTS
  (
    SELECT *
    FROM sysobjects
    WHERE id = OBJECT_ID(N'[memo].[Audit]')
    AND OBJECTPROPERTY(id, N'IsUserTable') = 1
  )
CREATE TABLE [memo].[Audit]
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
CREATE TRIGGER [memo].[TR_Contact_AUDIT]
ON [memo].[Contact]
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


           --You will need to change @TableName to match the table to be audited.
           -- Here we made GUESTS for your example.
           SELECT @TableName = 'Contact'

           SELECT @UserName = SYSTEM_USER,
  @UpdateDate = CONVERT(NVARCHAR(30), GETDATE(), 126)

           -- Action
           IF EXISTS (
                  SELECT *
FROM INSERTED
              )
               IF EXISTS (
                      SELECT *
FROM DELETED
                  )
                   SELECT @Type = 'U'
               ELSE
                   SELECT @Type = 'I'
           ELSE
               SELECT @Type = 'D'

           -- get list of columns
           SELECT *
INTO #ins
FROM INSERTED

           SELECT *
INTO #del
FROM DELETED

           -- Get primary key columns for full outer join
           SELECT @PKCols = COALESCE(@PKCols + ' and', ' on')
                  + ' i.[' + c.COLUMN_NAME + '] = d.[' + c.COLUMN_NAME + ']'
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk,
  INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
WHERE  pk.TABLE_NAME = @TableName
  AND CONSTRAINT_TYPE = 'PRIMARY KEY'
  AND c.TABLE_NAME = pk.TABLE_NAME
  AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

           -- Get primary key select for insert
           SELECT @PKSelect = COALESCE(@PKSelect + '+', '')
                  + '''<[' + COLUMN_NAME
                  + ']=''+convert(varchar(100),
           coalesce(i.[' + COLUMN_NAME + '],d.[' + COLUMN_NAME + ']))+''>'''
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk,
  INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
WHERE  pk.TABLE_NAME = @TableName
  AND CONSTRAINT_TYPE = 'PRIMARY KEY'
  AND c.TABLE_NAME = pk.TABLE_NAME
  AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME

           IF @PKCols IS NULL
           BEGIN
  RAISERROR('no PK on table %s', 16, -1, @TableName)

  RETURN
END

           SELECT @field = 0,
  -- @maxfield = MAX(COLUMN_NAME)
  @maxfield = -- FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName


                  MAX(
                      COLUMNPROPERTY(
                          OBJECT_ID(TABLE_SCHEMA + '.' + @TableName),
                          COLUMN_NAME,
                          'ColumnID'
                      )
                  )
FROM INFORMATION_SCHEMA.COLUMNS
WHERE  TABLE_NAME = @TableName






           WHILE @field < @maxfield
           BEGIN
  SELECT @field = MIN(
                          COLUMNPROPERTY(
                              OBJECT_ID(TABLE_SCHEMA + '.' + @TableName),
                              COLUMN_NAME,
                              'ColumnID'
                          )
                      )
  FROM INFORMATION_SCHEMA.COLUMNS
  WHERE  TABLE_NAME = @TableName
    AND COLUMNPROPERTY(
                              OBJECT_ID(TABLE_SCHEMA + '.' + @TableName),
                              COLUMN_NAME,
                              'ColumnID'
                          ) > @field

  SELECT @bit = (@field - 1)% 8 + 1

  SELECT @bit = POWER(2, @bit - 1)

  SELECT @char = ((@field - 1) / 8) + 1





  IF SUBSTRING(COLUMNS_UPDATED(), @char, 1) & @bit > 0
    OR @Type IN ('I', 'D')
               BEGIN
    SELECT @fieldname = COLUMN_NAME
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE  TABLE_NAME = @TableName
      AND COLUMNPROPERTY(
                                  OBJECT_ID(TABLE_SCHEMA + '.' + @TableName),
                                  COLUMN_NAME,
                                  'ColumnID'
                              ) = @field



    SELECT @sql =
                          '
           insert into Audit (    Type,
           TableName,
           PK,
           FieldName,
           OldValue,
           NewValue,
           UpdateDate,
           UserName)
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
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

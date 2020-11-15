-- This sql query should create all procedures

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-----------------------------------------------------------------------------------------
IF OBJECT_ID('[memo].spSumMinutesByOrderName', 'P') IS NOT NULL
    DROP PROCEDURE [memo].spSumMinutesByOrderName
GO
-----------------------------------------------------------------------------------------
CREATE PROCEDURE [memo].[spSumMinutesByOrderName]
	@OrderName nvarchar(50)
AS
BEGIN
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT cOrders.OrderCode, SUM(tWorks.Minutes) as SumMinutes
	FROM cOrders
	INNER JOIN tWorks
	ON cOrders.IDOrder = tWorks.IDOrder
	WHERE cOrders.OrderCode = @OrderName
	GROUP BY cOrders.OrderCode

END
GO



-----------------------------------------------------------------------------------------
IF OBJECT_ID('[memo].SetUserContext', 'P') IS NOT NULL
    DROP PROCEDURE [memo].SetUserContext
GO
-----------------------------------------------------------------------------------------
CREATE PROCEDURE [memo].[SetUserContext]
  @userId NVARCHAR (128)
AS
BEGIN
  SET NOCOUNT ON;

  DECLARE @context VARBINARY(128)
  SET @context = CONVERT(VARBINARY(128), @userId)

  SET CONTEXT_INFO @context

END
GO



-----------------------------------------------------------------------------------------
-- DROP FUNCTION [memo].[GetUserContext]
-- GO
IF OBJECT_ID('[memo].GetUserContext', 'FN') IS NOT NULL
    DROP FUNCTION [memo].GetUserContext
GO
-----------------------------------------------------------------------------------------
CREATE FUNCTION [memo].[GetUserContext] ()
RETURNS NVARCHAR (128)
AS
BEGIN

  DECLARE @idToReturn NVARCHAR(128)

  IF CONTEXT_INFO() IS NOT NULL
    --SELECT @IdToReturn = CONVERT(NVARCHAR (128), CONTEXT_INFO())
    SELECT @IdToReturn = REPLACE(CAST(CAST(CONTEXT_INFO() AS VARCHAR(128)) COLLATE SQL_Latin1_General_CP1_CI_AS AS VARCHAR(128)), CHAR(0), '')
  ELSE
    SELECT @IdToReturn = suser_name()

  RETURN @IdToReturn

END
GO



-- SELECT name
-- FROM master..spt_values
-- WHERE type = 'O9T'
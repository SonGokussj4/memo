USE [MemoDB]
GO
/****** Object:  StoredProcedure [dbo].[spSumMinutesByOrderName]    Script Date: 27.07.2020 17:45:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[spSumMinutesByOrderName]
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
	--ORDER BY cOrders.OrderCode
END

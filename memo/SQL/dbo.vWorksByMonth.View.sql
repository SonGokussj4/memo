GO
/****** Object:  View [dbo].[vWorksByMonth]    Script Date: 17.07.2020 17:12:21 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vWorksByMonth]
AS
SELECT     co.IDProject, co.OrderCode, ct.TypeCode, w.wYear, w.wMonth, w.WorkDone
FROM
    (
    SELECT IDOrder,
           IDType,
           Year(Datum) AS wYear,
           Month(Datum) AS wMonth,
           SUM(Minutes) AS WorkDone
    FROM tWorks
    GROUP BY IDOrder, IDType, Year(Datum), Month(Datum)
    )
    w
    INNER JOIN dbo.cOrders co ON w.IDOrder = co.IDOrder
    INNER JOIN dbo.cTypes ct ON w.IDType = ct.IDType
GO

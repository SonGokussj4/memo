/****** Object:  View [dbo].[vEmployees]    Script Date: 02.10.2020 14:43:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vEmployees]
AS
SELECT     u.ID, u.FormatedName, u2.ID AS IDDepartment, u2.FormatedName AS DepartName, g2.ParentID AS IDCompany,
                      CASE WHEN g2.ParentID = 1700 THEN 1 ELSE 0 END AS EVE, CASE WHEN g2.ParentID = 1701 THEN 1 ELSE 0 END AS EVAT
FROM         dbo.tUsers AS u INNER JOIN
                      dbo.tGroups AS g1 ON u.ID = g1.ChildID INNER JOIN
                      dbo.tUsers AS u2 ON g1.ParentID = u2.ID AND u2.intAccType = 2 INNER JOIN
                      dbo.tGroups AS g2 ON u2.ID = g2.ChildID AND g2.ParentID IN (1700, 1701)
WHERE     (u.intAccType = 1)
GO

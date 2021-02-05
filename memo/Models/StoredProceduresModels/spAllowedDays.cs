
// ALTER PROCEDURE [dbo].[pAllowedDays]
// 	  @IDUser   int
// 	, @OldDays tinyint = 6
// 	, @AllDays bit = 0

// AS

// DECLARE @StartDate smalldatetime, @EndDate smalldatetime

// SET @StartDate = DateAdd(d, -@OldDays, CONVERT(varchar, GetDate(), 112))
// SET @EndDate = DateAdd(n, 1439, CONVERT(varchar, GetDate(), 112))


// SELECT w.*, CASE WHEN Att>Assigned THEN 3 WHEN Assigned > Att THEN 2 WHEN Att>0 THEN 1 ELSE 0 END as OType
// FROM dbo.DayAttWork(@IDUser, @StartDate, @EndDate) w
// WHERE @AllDays = 1 OR Typ = 1 OR (Att + Assigned) > 0
// ORDER BY Datum Desc

using System;

namespace memo.Models
{
    public class spAllowedDays
    {
        public DateTime Datum { get; set; }
        public short Typ { get; set; }
        public int Att { get; set; }
        public int AttClean { get; set; }
        public int Assigned { get; set; }
        public int OType { get; set; }
    }
}
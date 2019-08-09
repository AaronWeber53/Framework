using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DojoManagmentSystem.Infastructure.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            // Get difference in days to the closest day of the week.
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;

            // Add the days to the given date.
            return dt.AddDays(-1 * diff).Date;
        }
    }
}
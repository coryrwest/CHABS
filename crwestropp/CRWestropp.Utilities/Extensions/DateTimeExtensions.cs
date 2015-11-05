using System;

namespace CRWestropp.Utilities.Extensions
{
	public static class DateTimeExtensions
    {
        #region Business Days Count
        /// <summary>
		/// Calculates number of business days, taking into account:
		///  - weekends (Saturdays and Sundays)
		///  - bank holidays in the middle of the week
		/// </summary>
		/// <param name="firstDay">First day in the time interval</param>
		/// <param name="lastDay">Last day in the time interval</param>
		/// <param name="bankHolidays">List of bank holidays excluding weekends</param>
		/// <returns>Number of business days during the 'span'</returns>
		public static int BusinessDaysUntil(this DateTime firstDay, DateTime lastDay, params DateTime[] bankHolidays)
		{
			// Checks to see if firstDayOfWeek and lastDayOfWeek are the same Sunday
			// and returns an appropraite value.
			int firstDayOfWeek = firstDay.DayOfWeek == DayOfWeek.Sunday
				? 7 : (int)firstDay.DayOfWeek;
			int lastDayOfWeek = lastDay.DayOfWeek == DayOfWeek.Sunday
				? 7 : (int)lastDay.DayOfWeek;
			if (firstDay > lastDay)
				throw new ArgumentException("Incorrect last day " + lastDay);

			TimeSpan span = lastDay - firstDay;
			int businessDays = span.Days + 1;
			int fullWeekCount = businessDays / 7;
			// find out if there are weekends during the time exceedng the full weeks
			if (businessDays > fullWeekCount * 7)
			{
				// we are here to find out if there is a 1-day or 2-days weekend
				// in the time interval remaining after subtracting the complete weeks
				if (lastDayOfWeek < firstDayOfWeek)
					lastDayOfWeek += 7;
				if (firstDayOfWeek <= 6)
				{
					if (lastDayOfWeek >= 7)// Both Saturday and Sunday are in the remaining time interval
						businessDays -= 2;
					else if (lastDayOfWeek >= 6)// Only Saturday is in the remaining time interval
						businessDays -= 1;
				}
				else if (firstDayOfWeek <= 7 && lastDayOfWeek >= 7)// Only Sunday is in the remaining time interval
					businessDays -= 1;
			}

			// subtract the weekends during the full weeks in the interval
			businessDays -= fullWeekCount + fullWeekCount;

			// subtract the number of bank holidays during the time interval
			foreach (DateTime bankHoliday in bankHolidays)
			{
				DateTime bh = bankHoliday.Date;
				if (firstDay <= bh && bh <= lastDay && !(bh.DayOfWeek == DayOfWeek.Sunday || bh.DayOfWeek == DayOfWeek.Saturday))
					--businessDays;
			}

			return businessDays;
        }
        #endregion

		public static DateTime FirstDay(this DateTime value) {
			return value.Date.AddDays(1 - value.Day);
		}

		public static DateTime LastDay(this DateTime value) {
			return value.FirstDay().AddMonths(1).AddDays(-1);
		}

		public static string ToPostgresString(this DateTime value) {
			return value.ToString("yyyy-MM-dd HH:mm:ss");
		}
    }
}

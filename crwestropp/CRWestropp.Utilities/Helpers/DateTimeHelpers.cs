using System;
using System.Collections.Generic;
using System.Linq;

namespace CRWestropp.Utilities.Helpers {
	public class DateTimeHelper {
		#region Get Week(end) Days
		/// <summary>
		/// Returns a list of weekdays in the specified month
		/// </summary>
		/// <param name="year"></param>
		/// <param name="month"></param>
		/// <returns>List of DateTimes</returns>
		public static List<DateTime> GetWeekdayDates(int year, int month) {
			return Enumerable.Range(1, DateTime.DaysInMonth(year, month))
							 .Select(day => new DateTime(year, month, day))
							 .Where(dt => dt.DayOfWeek != DayOfWeek.Sunday &&
										  dt.DayOfWeek != DayOfWeek.Saturday)
							 .ToList();
		}

		/// <summary>
		/// Returns a list of weekends in the specified month
		/// </summary>
		/// <param name="year"></param>
		/// <param name="month"></param>
		/// <returns>List of DateTimes</returns>
		public static List<DateTime> GetWeekendDates(int year, int month) {
			List<DateTime> dates = Enumerable.Range(1, DateTime.DaysInMonth(year, month))
							 .Select(day => new DateTime(year, month, day))
							 .Where(dt => dt.DayOfWeek == DayOfWeek.Sunday ||
										  dt.DayOfWeek == DayOfWeek.Saturday)
							 .ToList();
			return dates;
		}
		#endregion

		#region Business Days
		/// <summary>
		/// Returns all business days, taking into account:
		///  - weekends (Saturdays and Sundays)
		///  - bank holidays in the middle of the week
		/// </summary>
		/// <param name="month"></param>
		/// <param name="year"></param>
		/// <param name="bankHolidays">DateTime array of holidays to account for</param>
		/// <returns>DateTime List of business days in the month specified</returns>
		public static List<DateTime> GetBusinessDays(int month, int year, params DateTime[] bankHolidays) {
			List<DateTime> businessDays = new List<DateTime>();

			// Get all weekdays
			List<DateTime> weekdays = Enumerable.Range(1, DateTime.DaysInMonth(year, month))
				.Select(day => new DateTime(year, month, day))
				.Where(dt => dt.DayOfWeek != DayOfWeek.Sunday &&
							 dt.DayOfWeek != DayOfWeek.Saturday)
				.ToList();

			// Check for holidays
			foreach (DateTime day in weekdays) {
				foreach (DateTime holiday in bankHolidays) {
					if (day != holiday) {
						businessDays.Add(day);
					}
				}
			}

			return businessDays;
		}
		#endregion

		#region DateTime Period
		/// <summary>
		/// Returns the period of time between the supplied dates.
		/// If endDate is before startDate, a TimeSpan of 1,1,1
		/// is returned.
		/// </summary>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <returns></returns>
		public static TimeSpan DateTimePeriod(DateTime startDate, DateTime endDate) {
			TimeSpan timeSpan = new TimeSpan();
			if (endDate < startDate) {
				return new TimeSpan(1, 1, 1);
			}
			else {
				timeSpan = endDate - startDate;
			}

			return timeSpan;
		}
		#endregion
	}

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CHABS.API {
	public static class Extensions {
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

	    public static DateTime FirstDay(this DateTime dt) {
	        return new DateTime(dt.Year, dt.Month, 1);
	    }

	    public static DateTime LastDay(this DateTime dt) {
	        return dt.FirstDay().AddMonths(1).AddDays(-1);
        }

	    public static string JoinFormat<T>(this IEnumerable<T> list, string separator,
	        string formatString) {
	        formatString = string.IsNullOrWhiteSpace(formatString) ? "{0}" : formatString;
	        return string.Join(separator,
	            list.Select(item => string.Format(formatString, item)));
	    }

		public static string Truncate(this string value, int maxChars) {
			return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
		}

		public static string InsertBefore(this string str, string insert, object[] stringsToCheck) {
	        var lastIndex = 0;
            foreach (string check in stringsToCheck) {
                if (lastIndex >= 0) {
                    lastIndex = str.IndexOf(check, StringComparison.Ordinal);
                }
	        }

			// Spitting is different based on whether we find a match or not
		    var firstPart = "";
		    var lastPart = "";

		    if (lastIndex == -1) {
			    firstPart = str;
		    }
		    else {
				firstPart = str.Substring(0, lastIndex);
			    lastPart = str.Substring(lastIndex, str.Length - lastIndex);
			}

	        return firstPart + insert + lastPart;
	    }

		/// <summary>
		/// Will import all matching properties from source to dest excluding provided members.
		/// If a property in destination is not null and in source it is null, it will not be imported.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="source"></param>
		/// <param name="exclusions"></param>
		public static void Import(this object destination, object source, params string[] exclusions) {
			// If any this null throw an exception
			if (source == null || destination == null)
				throw new Exception("Source or/and Destination Objects are null");
			// Getting the Types of the objects
			Type typeDest = destination.GetType();
			Type typeSrc = source.GetType();

			// Iterate the Properties of the source instance and  
			// populate them from their desination counterparts  
			PropertyInfo[] srcProps = typeSrc.GetProperties();
			foreach (PropertyInfo srcProp in srcProps) {
				if (!srcProp.CanRead) {
					continue;
				}
				PropertyInfo targetProperty = typeDest.GetProperty(srcProp.Name);
				if (targetProperty == null) {
					continue;
				}
				if (!targetProperty.CanWrite) {
					continue;
				}
				if (targetProperty.GetSetMethod(true) != null && targetProperty.GetSetMethod(true).IsPrivate) {
					continue;
				}
				if ((targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) != 0) {
					continue;
				}
				if (!targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType)) {
					continue;
				}
				// If target property is part of exclusions, ignore
				if (exclusions.Contains(targetProperty.Name)) {
					continue;
				}
				// If source is null and destination is not, ignore
				if (srcProp.GetValue(source, null) == null && targetProperty.GetValue(destination, null) != null) {
					continue;
				}
				// Passed all tests, lets set the value
				targetProperty.SetValue(destination, srcProp.GetValue(source, null), null);
			}
		}
	}

}
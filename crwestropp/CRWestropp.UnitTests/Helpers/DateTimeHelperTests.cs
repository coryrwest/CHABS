using System;
using CRWestropp.Utilities.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace CRWestropp.UnitTests
{
    [TestClass]
    public class DateTimeHelperTests
    {
		[TestMethod]
		public void DateTimePeriodNegative() {
			TimeSpan span = DateTimeHelper.DateTimePeriod(DateTime.Now, new DateTime(2012, 1, 1));

			Assert.AreEqual(new TimeSpan(1, 1, 1), span);
		}

		[TestMethod]
		public void DateTimePeriod() {
			TimeSpan span = DateTimeHelper.DateTimePeriod(DateTime.Now, DateTime.Now.AddDays(1));

			Assert.AreEqual(new TimeSpan(24, 0, 0), span);
		}

		[TestMethod]
		public void GetWeekendDates() {
			List<DateTime> dates = DateTimeHelper.GetWeekendDates(2013, 9);

			Assert.IsTrue(dates.Contains(new DateTime(2013, 9, 14)));
		}

		[TestMethod]
		public void GetWeekdayDates() {
			List<DateTime> dates = DateTimeHelper.GetWeekdayDates(2013, 9);

			Assert.IsTrue(dates.Contains(new DateTime(2013, 9, 12)));
		}
    }
}

using System;
using OfficeOpenXml;

namespace CHABS.API.ExcelHelpers {
	public static class CellExtensions {
		public static ExcelRange NextRow(this ExcelRange cell, int multiplier = 1) {
			var newCell = cell[cell.Offset(multiplier, 0).Address];
			return newCell;
		}

		public static ExcelRange PrevRow(this ExcelRange cell, int multiplier = 1) {
			var newCell = cell[cell.Offset(multiplier * -1, 0).Address];
			return newCell;
		}

		public static ExcelRange NextColumn(this ExcelRange cell, int multiplier = 1) {
			var newCell = cell[cell.Offset(0, multiplier).Address];
			return newCell;
		}

		public static string ToFormattedDateTime(this ExcelRange cell, string format) {
			var value = Convert.ToDouble(cell.Value);
			var date = DateTime.FromOADate(value);
			return date.ToString(format);
		}
	}
}

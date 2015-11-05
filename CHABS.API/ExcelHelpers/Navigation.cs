using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRWestropp.Utilities.Extensions;
using OfficeOpenXml;

namespace CHABS.API.ExcelHelpers {
	public static class Navigation {
		/// <summary>
		/// Gets the last empty row (by default, based off of the A column).
		/// </summary>
		/// <returns></returns>
		public static ExcelRange GetLastEmptyRow(ExcelWorksheet sheet, string column = "A") {
			// Start at the top left corner
			var cell = sheet.Cells[column + "1"];

			// Get to last empty row
			while (!string.IsNullOrEmpty(cell.Text)) {
				cell = cell.NextRow();
			}
			return cell;
		}

		public static ExcelRange GetLastFullEmptyRow(ExcelWorksheet sheet, params string[] columnsToCheck) {
			ExcelRange firstCellOfEmptyRow = sheet.Cells["A1"];
			List<ExcelRange> lastRows = new List<ExcelRange>();
			foreach (string column in columnsToCheck) {
				// Start at the top left corner
				var cell = sheet.Cells[column + "1"];

				// Get to last empty row
				while (!string.IsNullOrEmpty(cell.Text)) {
					cell = cell.NextRow();
				}
				lastRows.Add(cell);
			}

			var lastEmptyCellColumn = GetRow(firstCellOfEmptyRow);
			foreach (ExcelRange row in lastRows) {
				var rowColumn = GetRow(row);
				if (rowColumn > lastEmptyCellColumn) {
					lastEmptyCellColumn = rowColumn;
				}
			}

			return sheet.Cells["A" + lastEmptyCellColumn];
		}

		public static int GetRow(ExcelRange cell) {
			return Convert.ToInt32(cell.Address.Substring(1));
		}

		public static string GetColumn(ExcelRange cell) {
			return cell.Address.Substring(0, 1);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRWestropp.Utilities.Extensions;
using OfficeOpenXml;

namespace CHABS.API.ExcelHelpers {
	public static class DataRetreival {
		/// <summary>
		/// Will find the last empty row in the column specified and 
		/// get all the data from row 1 to that column.
		/// </summary>
		/// <param name="sheet"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public static List<object> GetDataFromColumn(ExcelWorksheet sheet, string column = "A", int row = 0, int firstRowStart = 1) {
			var lastRow = row == 0 ? Navigation.GetLastEmptyRow(sheet, column) : sheet.Cells["A" + row];
			var range = sheet.Cells[string.Format("{1}{2}:{1}{0}", Navigation.GetRow(lastRow), column, firstRowStart)];

			var list = new List<object>();
			foreach (ExcelRangeBase rangeRow in range) {
				if (!string.IsNullOrEmpty(rangeRow.Text)) {
					list.Add(rangeRow.Value);
				}
			}

			return list;
		}

		public static List<List<object>> GetDataFromColumns(ExcelWorksheet sheet, params string[] columns) {
			var data = new List<List<object>>();
			foreach (string column in columns) {
				var columnData = GetDataFromColumn(sheet, column);
				data.Add(columnData);
			}
			return data;
		} 
	}
}

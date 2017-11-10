using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using CHABS.API.ExcelHelpers;

namespace CHABS.API.WorkingBudget {
	public class WorkingBudgetFileHandler {
		#region Properties

		private ExcelPackage package;
		private ExcelWorkbook workbook;

		private ExcelWorksheet monthTransationsWS;
		private ExcelWorksheet MonthTransactionsWS {
			get {
				if (monthTransationsWS == null) {
					monthTransationsWS = GetCurrentMonthTransactionsWorksheet();
				}

				return monthTransationsWS;
			}
		}
		#endregion

		public WorkingBudgetFileHandler(byte[] bytes) {
			package = new ExcelPackage();
			workbook = LoadSave.LoadWorkbookFromBytes(package, bytes);
		}

		public byte[] GetByteArray() {
			return LoadSave.SaveWorkbookToBytes(package);
		}

		public List<string> GetCurrentSheetTransIDs() {
			var lastRow = Navigation.GetLastFullEmptyRow(MonthTransactionsWS, "A", "B", "C", "D", "E", "F", "G", "H");

			var list = new List<string>();
			var data = DataRetreival.GetDataFromColumn(MonthTransactionsWS, "H", Navigation.GetRow(lastRow), 2);
			list = data.Select(d => d.ToString()).ToList();

			return list;
		}

		public string GetMostRecentDate() {
			var lastRow = Navigation.GetLastEmptyRow(MonthTransactionsWS);

			// Get the last full row
			var lastCell = MonthTransactionsWS.Cells[lastRow.Address];
			lastCell = lastCell.PrevRow();

			// Convert the date
			return lastCell.ToFormattedDateTime("yyyy-MM-dd");
		}

		public void AddDataRow(dynamic data) {
			var lastEmptyRow = Navigation.GetLastFullEmptyRow(MonthTransactionsWS, "A", "B", "C", "D", "E", "F", "G", "H");

			var cell = MonthTransactionsWS.Cells[lastEmptyRow.Address];

			// Insert blank row
			if (cell.Address != "A2") {
				MonthTransactionsWS.InsertRow(Navigation.GetRow(lastEmptyRow), 1, Navigation.GetRow(lastEmptyRow) - 1);
			}

			// Date
			cell.Value = DateTime.Parse(data.Date.ToString()).ToOADate();
			cell.Style.Font.Bold = true;
			cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
			cell.Style.Numberformat.Format = "m/d/yyyy";
			// Amount
			cell = cell.NextColumn();
			string amount = data.Amount.ToString();
			amount = Regex.Replace(amount, "[^+-.0-9]", "");
			cell.Value = Convert.ToDecimal(amount);
			cell.Style.Numberformat.Format = "_($* #,##0.00_);_($* (#,##0.00);_($* \" - \"??_);_(@_)";
			// Description
			cell = cell.NextColumn();
			cell.Value = data.Description.ToString();
			// Source
			cell = cell.NextColumn();
			cell.Value = data.Source.ToString();
			cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
			// Category
			cell = cell.NextColumn();
			if (data.Category != null) {
				cell.Value = data.Category.ToString();
			}
			// TransID
			cell = cell.NextColumn(3);
			cell.Value = data.ServiceId.ToString();
		}

		#region Private Members
		/// <summary>
		/// Will get the current month Transactions sheet. If it does not exist
		/// it will create it as well as the Summary sheet.
		/// </summary>
		/// <returns></returns>
		private ExcelWorksheet GetCurrentMonthTransactionsWorksheet() {
			// Get the worksheet for the current month
			var formattedCurrentMonth = DateTime.Now.ToString("MMMM yyyy");
			var name = string.Format("{0} Transactions", formattedCurrentMonth);
			var worksheet = workbook.Worksheets[name];
			var mostCurrentSheetMonth = DateTime.Now.AddDays(-30).ToString("MMMM yyyy");

			if (worksheet == null) {
				// If we fail then we have to add it.
				var lastMonthName = string.Format("{0} Transactions", mostCurrentSheetMonth);
				worksheet = workbook.Worksheets[lastMonthName];
				if (worksheet == null) {
					// Go back one more month
					mostCurrentSheetMonth = DateTime.Now.AddDays(-60).ToString("MMMM yyyy");
					var lastTwoMonthName = string.Format("{0} Transactions", mostCurrentSheetMonth);
					worksheet = workbook.Worksheets[lastTwoMonthName];
				}
				worksheet = workbook.Worksheets.Add(name, worksheet);
				// Move it to the front
				workbook.Worksheets.MoveToStart(name);
				// Delete data after copy
				// TODO: Does this mess up formatting?
				worksheet.DeleteRow(2, 200);

				// We also need to copy the summary sheet
				workbook.Worksheets.Add(formattedCurrentMonth, workbook.Worksheets[mostCurrentSheetMonth]);
				// Move it to the front
				workbook.Worksheets.MoveToStart(formattedCurrentMonth);
			}

			return worksheet;
		}
		#endregion
	}
}

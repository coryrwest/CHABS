using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using CHABS.API.ExcelHelpers;
using CRWestropp.Utilities.Extensions;

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
			var lastEmptyRow = Navigation.GetLastFullEmptyRow(MonthTransactionsWS);

			var cell = MonthTransactionsWS.Cells[lastEmptyRow.Address];

			// Insert blank row
			if (cell.Address != "A2") {
				MonthTransactionsWS.InsertRow(Navigation.GetRow(lastEmptyRow), 1, Navigation.GetRow(lastEmptyRow) - 1);
			}

			// Date
			cell.Value = DateTime.Parse(data.date.ToString()).ToOADate();
			cell.Style.Font.Bold = true;
			cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
			cell.Style.Numberformat.Format = "m/d/yyyy";
			// Amount
			cell = cell.NextColumn();
			var amount = data.amount.ToString();
			amount = Regex.Replace(amount, "[^+.0-9]", "");
			if (amount.IndexOf("+") != -1) {
				cell.Value = amount.ToDecimal();
			} else {
				cell.Value = amount.ToDecimal() * -1;
			}
			// Description
			cell = cell.NextColumn();
			cell.Value = data.description.ToString();
			// Source
			cell = cell.NextColumn();
			cell.Value = data.source.ToString();
			cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
			// Category
			cell = cell.NextColumn();
			if (data.category != null) {
				cell.Value = data.category.ToString();
			}
			// TransID
			cell = cell.NextColumn(3);
			cell.Value = data.transactionId.ToString();
		}

		#region Private Members
		/// <summary>
		/// Will get the current month Transactions sheet. If it does not exist
		/// it will create it as well as the Summary sheet.
		/// </summary>
		/// <returns></returns>
		private ExcelWorksheet GetCurrentMonthTransactionsWorksheet() {
			// Get the worksheet for the current month
			var name = string.Format("{0} Transactions", DateTime.Now.ToString("MMMM yyyy"));
			var worksheet = workbook.Worksheets[name];

			if (worksheet == null) {
				// If we fail then we have to add it.
				var lastMonthName = string.Format("{0} Transactions", DateTime.Now.AddDays(-30).ToString("MMMM yyyy"));
				worksheet = workbook.Worksheets.Add(name, workbook.Worksheets[lastMonthName]);
				// TODO: Delete data after copy

				// We also need to copy the summary sheet
				var summaryName = DateTime.Now.ToString("MMMM yyyy");
				var lastMonthSummaryName = DateTime.Now.AddDays(-30).ToString("MMMM yyyy");
				workbook.Worksheets.Add(summaryName, workbook.Worksheets[lastMonthSummaryName]);
			}

			return worksheet;
		}
		#endregion
	}
}

using System;
using System.Collections.Generic;
using CHABS.API.Objects;

namespace CHABS.API.WorkingBudget {
	public static class WorkingBudgetUpdater {
		public static byte[] Update(byte[] bytes, List<BankLoginAccountTransaction> transactions) {
			var fileHandler = new WorkingBudgetFileHandler(bytes);

			// Get last date and TransIDs
			var transIDs = fileHandler.GetCurrentSheetTransIDs();
			
			// If no trans ids then we are in a new month with no auto transactions (possibly only manual)
			string date;
			if (transIDs.Count == 0) {
				date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("yyyy-MM-dd");
			} else {
				date = fileHandler.GetMostRecentDate();
			}

			// Filter the transactions
			var newTransactions = new List<BankLoginAccountTransaction>();
			foreach (BankLoginAccountTransaction transaction in transactions) {
				var id = transaction.ServiceId;
				if (!transIDs.Contains(id)) {
					newTransactions.Add(transaction);
				}
			}
			// Fuzzy filter transactions
			//foreach (BankLoginAccountTransaction transaction in newTransactions) {
				
			//}

			// Add the transactions to the document
			foreach (dynamic newTransaction in newTransactions) {
				fileHandler.AddDataRow(newTransaction);
			}

			return fileHandler.GetByteArray();
		}
	}
}

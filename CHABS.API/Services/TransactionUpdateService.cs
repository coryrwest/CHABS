using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CHABS.API.Objects;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace CHABS.API.Services {
	public static class TransactionUpdateService {
		public class Options {
			public IBankDataService BankService { get; set; }
			public BankAccountService DataService { get; set; }
			public List<Guid> LoginIds { get; set; }
			public Guid UserId { get; set; }

			public Options(IBankDataService bankService, BankAccountService dataService, List<Guid> loginIds, Guid userId) {
				BankService = bankService;
				DataService = dataService;
				LoginIds = loginIds;
				UserId = userId;
			}
		}

		/// <summary>
		/// Will get transactions from the bank service, check existing,
		/// map categories, and save to the database.
		/// </summary>
		public static void DoTransactionUpdate(Options options) {
			// Loop through the login ids
			foreach (Guid loginId in options.LoginIds) {
				ProcessLogin(loginId, options);
			}
		}

		private static void ProcessLogin(Guid loginId, Options options) {
			var login = options.DataService.Logins.GetById(loginId);
			if (login.AccessToken != null) {
				var transactions = options.BankService.GetRecentTransactions(login.AccessToken).Result;
				ProcessTransactions(transactions, login, options);
			}
		}

		private static void ProcessTransactions(List<BankLoginAccountTransaction> transactions, BankLogin login, Options options) {
			foreach (BankLoginAccountTransaction transaction in transactions) {
				transaction.LoginId = login.Id;
				// Check existing
				var existing = options.DataService.Transactions.GetSingle(new { serviceid = transaction.ServiceId });
				if (existing == null) {
					// Get the source name
					var source = options.DataService.Accounts.GetSingle(new { serviceid = transaction.ServiceAccountId });
					transaction.Source = source.Name;
					// Map categories
					var category = options.DataService.Categories.FindCategoryMatch(transaction.Description);
					if (category != null) {
						transaction.Category = category.Name;
					}
					// Save if a new transaction
					options.DataService.Transactions.Upsert(transaction);
				}
			}
		}
	}
}

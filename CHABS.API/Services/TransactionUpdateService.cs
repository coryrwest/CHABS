using System;
using System.Collections.Generic;
using CHABS.API.Objects;
using CHABS.API.Services.DataServices;

namespace CHABS.API.Services {
	public static class TransactionUpdateService {
		public class ExecutionContext {
			public List<Guid> LoginIds { get; set; }
			public Guid UserId { get; set; }
			public DataService Services { get; set; }
			public IBankDataService BankService { get; set; }

			public ExecutionContext(List<Guid> loginIds, Guid userId, DataService services, IBankDataService bankService) {
				LoginIds = loginIds;
				UserId = userId;
				Services = services;
				BankService = bankService;
			}
		}

		/// <summary>
		/// Will get transactions from the bank service, check existing,
		/// map categories, and save to the database.
		/// </summary>
		public static void DoTransactionUpdate(PlaidOptions options, ExecutionContext methodOptions, DateTime start, DateTime end) {
			// Loop through the login ids
			foreach (Guid loginId in methodOptions.LoginIds) {
				ProcessLogin(options, loginId, methodOptions, start, end);
			}
		}

		private static void ProcessLogin(PlaidOptions options, Guid loginId, ExecutionContext methodOptions, DateTime start, DateTime end) {
			var login = methodOptions.Services.BankConnections.GetById(loginId);
			if (login.AccessToken != null) {
				var transactions = methodOptions.BankService.GetRecentTransactions(options, login.AccessToken, start, end);
				ProcessTransactions(transactions, login, methodOptions);
			}
		}

		private static void ProcessTransactions(List<AccountTransaction> transactions, BankConnection connection, ExecutionContext options) {
			foreach (AccountTransaction transaction in transactions) {
				transaction.LoginId = connection.Id;
				// Check existing
				var existing = options.Services.AccountTransactions.GetSingle("serviceid = @serviceid", new { serviceid = transaction.ServiceId }, true);
				if (existing == null) {
					// Get the source name
					var source = options.Services.BankAccounts.GetSingle("serviceid = @serviceid", new { serviceid = transaction.ServiceAccountId });
					transaction.Source = source.DisplayName;
					// Map categories
					var category = options.Services.Categories.FindCategoryMatch(transaction.Description);
					if (category != null) {
						transaction.Category = category.Name;
					}
					// Save if a new transaction
					try {
						options.Services.AccountTransactions.Upsert(transaction);
					}
					catch (Exception ex) {
						throw new Exception($"Error with transaction {transaction.ServiceId}, {transaction.Description}. {ex.Message}", ex);
					}
				}
			}
		}
	}
}

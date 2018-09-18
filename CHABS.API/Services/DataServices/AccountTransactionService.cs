using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CHABS.API.Objects;

namespace CHABS.API.Services.DataServices
{
	public class AccountTransactionService : BaseService<AccountTransaction> {
		public AccountTransactionService(Session session) : base(session) {
		}

		public List<AccountTransaction> GetLast50Transactions(List<Guid> loginIds) {
			var serviceIdsPart = GetFormattedServiceIds(loginIds);
			if (string.IsNullOrEmpty(serviceIdsPart)) {
				return new List<AccountTransaction>();
			}

			var query = string.Format("select *, false as IsNew from transactions where serviceaccountid in ({0}) order by date desc limit 50", serviceIdsPart);
			var transactions = RawQuery<AccountTransaction>(query, new { }).ToList();
			return transactions;
		}

		public List<AccountTransaction> GetLast30DaysTransactions(List<Guid> loginIds) {
			var serviceIdsPart = GetFormattedServiceIds(loginIds);
			if (string.IsNullOrEmpty(serviceIdsPart)) {
				return new List<AccountTransaction>();
			}

			// Get dates
			var startDate = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
			var endDate = DateTime.Now.ToString("yyyy-MM-dd");

			var query = string.Format("select *, false as IsNew from transactions where serviceaccountid in ({0}) and date between '{1}' and '{2}' order by date desc", serviceIdsPart, startDate, endDate);
			var transactions = RawQuery<AccountTransaction>(query, new { }).ToList();
			return transactions;
		}

		public List<AccountTransaction> GetThisMonthsTransactions(List<Guid> loginIds) {
			// Get dates
			var startDate = DateTime.Now.FirstDay();
			var endDate = DateTime.Now.LastDay();
			
			return GetTransactionsByDateRange(loginIds, startDate, endDate);
		}

		public List<AccountTransaction> GetTransactionsByDateRange(List<Guid> loginIds, DateTime start, DateTime end) {
			var serviceIdsPart = GetFormattedServiceIds(loginIds);
			if (string.IsNullOrEmpty(serviceIdsPart)) {
				return new List<AccountTransaction>();
			}

			// Get dates
			var startDate = start.ToString("yyyy-MM-dd");
			var endDate = end.ToString("yyyy-MM-dd");

			var query = string.Format("select *, false as IsNew from transactions where serviceaccountid in ({0}) and date between '{1}' and '{2}' order by date desc", serviceIdsPart, startDate, endDate);
			var transactions = RawQuery<AccountTransaction>(query, new { }).ToList();
			transactions.ForEach(t => t.IsNew = false);
			return transactions;
		}

		private string GetFormattedServiceIds(List<Guid> loginIds) {
			// Get the visible accounts to make sure we are not operating on unwanted transactions
			var accountService = new BankAccountService(Session);
			var accountServiceIds = new List<string>();
			foreach (Guid loginId in loginIds) {
				accountServiceIds.AddRange(accountService.GetVisibleAccountServiceIds(loginId));
			}

			// Join ids for IN
			var serviceIdsPart = accountServiceIds.JoinFormat(",", "'{0}'");
			if (string.IsNullOrEmpty(serviceIdsPart)) {
				return "";
			} else {
				return serviceIdsPart;
			}
		}
	}
}

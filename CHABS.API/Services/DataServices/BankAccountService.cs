using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using CHABS.API.Objects;
using CRWestropp.Utilities.Extensions;

namespace CHABS.API.Services {
	public class BankAccountService {
		public BaseBankLoginService Logins;
		public BaseBankLoginAccountTransactionService Transactions;
		public BaseBankLoginAccountService Accounts;
		public BaseCategoryService Categories;
		public BaseService<CategoryMatch> CategoryMatches;
		public BaseService<Budget> Budgets; 
		private static Session Session { get; set; }

		public BankAccountService(Session session) {
			Session = session;
			Logins = new BaseBankLoginService(Session);
			Transactions = new BaseBankLoginAccountTransactionService(Session);
			Accounts = new BaseBankLoginAccountService(Session);
			Categories = new BaseCategoryService(Session);
			CategoryMatches = new BaseService<CategoryMatch>(Session);
			Budgets = new BaseService<Budget>(Session);
		}
	}

	public class BaseBankLoginAccountService : BaseService<BankLoginAccount> {
		public BaseBankLoginAccountService(Session session) : base(session) {
		}

		public List<string> GetVisibleAccountServiceIds(Guid loginId) {
			var accounts = GetList(new { loginid = loginId, shown = true });
			return accounts.Select(a => a.ServiceId).ToList();
		} 
	}

	public class BaseCategoryService : BaseService<Category> {
		public BaseCategoryService(Session session) : base(session) {
		}

		public List<Category> GetAll(bool includeDeleted = false) {
			var where = string.Format("where householdid = '{0}' order by sort, deleted", Session.Household.Id);
			return GetList(where, includeDeleted);
		}

		public Category FindCategoryMatch(string transactionDescription) {
			string query = string.Format("select categoryid, match from category_matches join categories on categories.id = categoryid where categories.householdid = @householdid");
			var matches = db.Query(query, new { householdid = Session.Household.Id });
			Guid categoryId = Guid.Empty;
			foreach (dynamic match in matches) {
				if (transactionDescription.ToLower().Contains(match.match.ToLower())) {
					string id = match.categoryid.ToString();
					categoryId = id.ToGuid();
				}
			}
			return GetById(categoryId);
		}

		protected override void BeforeInsert(DataObject daObj) {
			// Check for existing objects that may match and error appropraitely.
			var category = daObj as Category;
			if (category != null) {
				var name = category.Name;
				var existingItem = GetSingle(new { name }, true);
				if (existingItem != null) {
					throw new Exception("A category by that name already exists. It was deleted. Please restore the deleted category instead of making a new one.");
				}
			}
			base.BeforeInsert(daObj);
		}
	}

	public class BaseBankLoginService : BaseService<BankLogin> {
		public BaseBankLoginService(Session session) : base(session) {
		}

		public List<Guid> GetHouseholdLoginIds(Guid householdId) {
			return GetList(new { householdId }).Select(l => l.Id).ToList();
		}

		public override void Delete(Guid id) {
			// Build query
			const string query = "delete from accounts where loginid = @loginId; delete from logins where id = @loginId;";
			db.Execute(query, new { loginId = id });
		}

		// Plaid Methods
		public void SavePublicToken(string token) {
			db.Execute("insert into public_token_temp (publictoken, userid) values (@Token, @UserId);", new { Token = token, UserId = Session.UserId });
		}

		public void DeletePublicToken(string token) {
			db.Execute("delete from public_token_temp  where publictoken = @Token and userid = @UserId;", new { Token = token, UserId = Session.UserId });
		}
	}

	public class BaseBankLoginAccountTransactionService : BaseService<BankLoginAccountTransaction> {
		public BaseBankLoginAccountTransactionService(Session session) : base(session) {
		}

		public List<BankLoginAccountTransaction> GetDateRangeTransactions(DateTime startDate, DateTime endDate, List<Guid> loginIds) {
			var loginQueryPart = loginIds.JoinFormat(",", "'{0}'");
			var query = string.Format("where loginid in ({2}) and date between '{0}' and '{1}' order by date desc",
				startDate.ToPostgresString(), endDate.ToPostgresString(), loginQueryPart);

			var transactions = GetList(query);
			return transactions;
		}

		public List<BankLoginAccountTransaction> GetMonthTransactions(List<Guid> loginIds, int month, int year = 0) {
			var rangeYear = year == 0 ? DateTime.Now.Year : year;
			var startDate = new DateTime(rangeYear, month, 1).FirstDay();
			var endDate = new DateTime(rangeYear, month, 1).LastDay();
			return GetDateRangeTransactions(startDate, endDate, loginIds);
		}

		public List<BankLoginAccountTransaction> GetLast50Transactions(List<Guid> loginIds) {
			var serviceIdsPart = GetFormattedServiceIds(loginIds);

			if (serviceIdsPart.IsNull()) {
				return new List<BankLoginAccountTransaction>();
			}

			var query = string.Format("where serviceaccountid in ({0}) order by date desc limit 50", serviceIdsPart);
			var transactions = GetList(query);
			return transactions;
		}

		public List<BankLoginAccountTransaction> GetLast30DaysTransactions(List<Guid> loginIds) {
			var serviceIdsPart = GetFormattedServiceIds(loginIds);

			// Get dates
			var startDate = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
			var endDate = DateTime.Now.ToString("yyyy-MM-dd");

			var query = string.Format("where serviceaccountid in ({0}) and date between '{1}' and '{2}' order by date desc", serviceIdsPart, startDate, endDate);
			var transactions = GetList(query);
			return transactions;
		}

		public List<BankLoginAccountTransaction> GetThisMonthsTransactions(List<Guid> loginIds) {
			var serviceIdsPart = GetFormattedServiceIds(loginIds);

			// Get dates
			var startDate = DateTime.Now.FirstDay().ToString("yyyy-MM-dd");
			var endDate = DateTime.Now.LastDay().ToString("yyyy-MM-dd");

			var query = string.Format("where serviceaccountid in ({0}) and date between '{1}' and '{2}' order by date desc", serviceIdsPart, startDate, endDate);
			var transactions = GetList(query);
			return transactions;
		}

		private string GetFormattedServiceIds(List<Guid> loginIds) {
			// Get the visible accounts to make sure we are not operating on unwanted transactions
			var accountService = new BaseBankLoginAccountService(Session);
			var accountServiceIds = new List<string>();
			foreach (Guid loginId in loginIds) {
				accountServiceIds.AddRange(accountService.GetVisibleAccountServiceIds(loginId));
			}

			// Join ids for IN
			var serviceIdsPart = accountServiceIds.JoinFormat(",", "'{0}'");
			if (serviceIdsPart.IsNull()) {
				return "";
			} else {
				return serviceIdsPart;
			}
		}
	}
}
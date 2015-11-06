using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using CHABS.API.Objects;
using CRWestropp.Utilities.Extensions;

namespace CHABS.API.Services {
	public class BankAccountService {
		public BaseBankLoginService Logins;
		public BaseBankLoginAccountTransactionService Transactions;
		public BaseService<BankLoginAccount> Accounts;
		public BaseCategoryService Categories;
		public BaseService<CategoryMatch> CategoryMatches; 
		private static Session Session { get; set; }

		public BankAccountService(Session session) {
			Session = session;
			Logins = new BaseBankLoginService(Session);
			Transactions = new BaseBankLoginAccountTransactionService(Session);
			Accounts = new BaseService<BankLoginAccount>(Session);
			Categories = new BaseCategoryService(Session);
			CategoryMatches = new BaseService<CategoryMatch>(Session);
		}
	}

	public class BaseCategoryService : BaseService<Category> {
		public BaseCategoryService(Session session) : base(session) {
		}

		public List<Category> GetAll() {
			var where = string.Format("where householdid = '{0}' order by sort", Session.Household.Id);
			return GetList(where);
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
	}

	public class BaseBankLoginService : BaseService<BankLogin> {
		public BaseBankLoginService(Session session) : base(session) {
		}

		public List<Guid> GetUserLoginIds(Guid userId) {
			return GetList(new {userid = userId}).Select(l => l.Id).ToList();
		}

		public override void Delete(Guid id) {
			// Build query
			const string query = "delete from accounts where loginid = @loginId; delete from logins where id = @loginId;";
			db.Execute(query, new { loginId = id });
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

		public List<BankLoginAccountTransaction> GetLast30DaysTransactions(List<Guid> loginIds) {
			var loginQueryPart = loginIds.JoinFormat(",", "'{0}'");
			if (loginQueryPart.IsNull()) {
				return new List<BankLoginAccountTransaction>();
			}
			var query = string.Format("where loginid in ({0}) order by date desc", loginQueryPart);
			var transactions = GetList(query);
			return transactions;
		} 
	}
}
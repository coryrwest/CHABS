using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CHABS.API.Objects;

namespace CHABS.API.Services {
	public interface IBankDataService {
		string AuthenticateBankUser(PlaidOptions options, BankDataServiceOptions serviceOptions, out List<BankAccount> BankList);
		List<AccountTransaction> GetRecentTransactions(PlaidOptions options, string token, DateTime start, DateTime end);
		List<BankAccount> GetAccounts(PlaidOptions options, Guid loginId, string token);

		/// <summary>
		/// Will exchange a public token from plaid link to an access token
		/// </summary>
		/// <param name="afterAuthData"></param>
		/// <returns></returns>
		string RunAfterAuthFunction(PlaidOptions options, string afterAuthData);


		string DeleteUser(PlaidOptions options, string token);
	}

	public class BankDataServiceOptions {
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Institution { get; set; }
		public string HookUrl { get; set; }
		public Guid LoginId { get; set; }

		public BankDataServiceOptions(string username, string password, string insitution, string hookUrl, Guid loginId) {
			UserName = username;
			Password = password;
			Institution = insitution;
			HookUrl = hookUrl;
			LoginId = loginId;
		}
	}
}

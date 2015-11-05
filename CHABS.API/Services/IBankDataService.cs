using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CHABS.API.Objects;
using CRWestropp.Utilities;

namespace CHABS.API.Services {
	public interface IBankDataService {
		Task<KeyValuePairs> GetInstitutions();
		string AuthenticateBankUser(BankDataServiceOptions options, out List<BankLoginAccount> BankList);
		Task<List<BankLoginAccountTransaction>> GetRecentTransactions(string token);
		Task<List<BankLoginAccount>> GetAccounts(Guid loginId, string token);

		/// <summary>
		/// Will exchange a public token from plaid link to an access token
		/// </summary>
		/// <param name="publicToken"></param>
		/// <returns></returns>
		string ExchangePublicToken(string publicToken);

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

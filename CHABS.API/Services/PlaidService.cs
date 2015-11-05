using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CHABS.API.Objects;
using CRWestropp.Utilities;
using CRWestropp.Utilities.Extensions;
using Newtonsoft.Json;
using RestSharp;

namespace CHABS.API.Services {
	public class PlaidService : IBankDataService {
		public RestClient Client = new RestClient(ConfigurationManager.AppSettings["PlaidURL"]);

		/// <summary>
		/// Will authenticate the user and save all banks associated with the institution
		/// </summary>
		/// <param name="username"></param>
		/// <param name="password"></param>
		/// <param name="intitution"></param>
		/// <param name="hookUrl"></param>
		/// <returns></returns>
		public string AuthenticateBankUser(BankDataServiceOptions options, out List<BankLoginAccount> bankList) {
			var request = new RestRequest("connect", Method.POST);
			var body = new {
				client_id = ConfigurationManager.AppSettings["PlaidID"],
				secret = ConfigurationManager.AppSettings["PlaidSecret"],
				username = options.UserName,
				password = options.Password,
				type = options.Institution,
				options = new {
					webhook = options.HookUrl,
					login_only = true
				}
			};
			request.AddJsonBody(body);
			request.RequestFormat = DataFormat.Json;
			var response = Client.Execute(request);

			JsonObject jresponse = JsonConvert.DeserializeObject<JsonObject>(response.Content);

			JsonObject[] banks = JsonConvert.DeserializeObject<JsonObject[]>(jresponse["accounts"].ToString());
			bankList = banks.Select(bank => {
				var meta = JsonConvert.DeserializeObject<JsonObject>(bank["meta"].ToString());
				return new BankLoginAccount() {
					LoginId = options.LoginId,
					Name = meta["name"].ToString(),
					ServiceId = bank["_id"].ToString()
				};
			}).ToList();

			if (response.StatusCode == HttpStatusCode.OK) {
				return jresponse["access_token"].ToString();
			} else {
				throw new Exception(response.StatusDescription);
			}
		}

		/// <summary>
		/// Will exchange a public token from plaid link to an access token
		/// </summary>
		/// <param name="publicToken"></param>
		/// <returns></returns>
		public string ExchangePublicToken(string publicToken) {
			var request = new RestRequest("exchange_token", Method.POST);
			var body = new {
				client_id = ConfigurationManager.AppSettings["PlaidID"],
				secret = ConfigurationManager.AppSettings["PlaidSecret"],
				public_token = publicToken
			};
			request.AddJsonBody(body);
			request.RequestFormat = DataFormat.Json;
			var response = Client.Execute(request);
			JsonObject jresponse = JsonConvert.DeserializeObject<JsonObject>(response.Content);
			if (response.StatusCode == HttpStatusCode.OK) {
				return jresponse["access_token"].ToString();
			} else {
				throw new Exception(response.StatusDescription);
			}
		}

		public Task<KeyValuePairs> GetInstitutions() {
			var func = new Func<KeyValuePairs>(() => {
				var request = new RestRequest("institutions", Method.GET);
				var response = Client.Execute(request);
				if (response.StatusCode != HttpStatusCode.OK) {
					throw new Exception(response.StatusDescription);
				}
				JsonObject[] institutions = JsonConvert.DeserializeObject<JsonObject[]>(response.Content);
				var kvp = new KeyValuePairs();
				foreach (JsonObject institution in institutions) {
					kvp.Add(institution["type"].ToString(), institution["name"].ToString());
				}
				return kvp;
			});
			return Task.Factory.StartNew(func);
		}

		public Task<List<BankLoginAccount>> GetAccounts(Guid loginId, string token) {
			var func = new Func<List<BankLoginAccount>>(() => {
				var response = RetreiveAccountsAndTransactions(token);

				var accountList = new List<BankLoginAccount>();
				JsonObject jresponse = JsonConvert.DeserializeObject<JsonObject>(response);
				var accounts = JsonConvert.DeserializeObject<JsonObject[]>(jresponse["accounts"].ToString());
				foreach (JsonObject account in accounts) {
					accountList.Add(new BankLoginAccount() {
						LoginId = loginId,
						Name = JsonConvert.DeserializeObject<JsonObject>(account["meta"].ToString())["name"].ToString(),
						ServiceId = account["_id"].ToString(),
						Shown = true
					});
				}

				return accountList;
			});
			return Task.Factory.StartNew(func);
		}

		public Task<List<BankLoginAccountTransaction>> GetRecentTransactions(string token) {
			var func = new Func<List<BankLoginAccountTransaction>>(() => {
				var response = RetreiveAccountsAndTransactions(token);
				
				var transactionList = new List<BankLoginAccountTransaction>();
				JsonObject jresponse = JsonConvert.DeserializeObject<JsonObject>(response);
				var transactions = JsonConvert.DeserializeObject<JsonObject[]>(jresponse["transactions"].ToString());
				foreach (JsonObject transaction in transactions) {
					transactionList.Add(new BankLoginAccountTransaction() {
						Date = DateTime.Parse(transaction["date"].ToString()),
						Amount = Decimal.Negate(transaction["amount"].ToDecimal()),
						Description = transaction["name"].ToString(),
						ServiceId = transaction["_id"].ToString(),
						ServiceAccountId = transaction["_account"].ToString()
					});
				}

				return transactionList;
			});
			return Task.Factory.StartNew(func);
		}

		private string RetreiveAccountsAndTransactions(string token) {
			var request = new RestRequest("connect/get", Method.POST);
			var body = new {
				client_id = ConfigurationManager.AppSettings["PlaidID"],
				secret = ConfigurationManager.AppSettings["PlaidSecret"],
				access_token = token
			};
			request.AddJsonBody(body);
			request.RequestFormat = DataFormat.Json;
			var response = Client.Execute(request);

			if (response.StatusCode != HttpStatusCode.OK) {
				throw new Exception(response.StatusDescription);
			}
			return response.Content;
		}
	}
}

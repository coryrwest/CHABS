using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CHABS.API.Objects;
using Newtonsoft.Json;

namespace CHABS.API.Services {
    public class PlaidService : IBankDataService {
        public HttpClient Client = new HttpClient();
        
        /// <summary>
        /// Will authenticate the user and save all banks associated with the institution
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="intitution"></param>
        /// <param name="hookUrl"></param>
        /// <returns></returns>
        public string AuthenticateBankUser(PlaidOptions options, BankDataServiceOptions serviceOptions, out List<BankAccount> bankList) {
            var body = new {
                client_id = options.ClientId,
                secret = options.ClientSecret,
                username = serviceOptions.UserName,
                password = serviceOptions.Password,
                type = serviceOptions.Institution,
                options = new {
                    webhook = serviceOptions.HookUrl,
                    login_only = true
                }
            };
            var response = Client.PostAsync($"{options.Url}/connect", new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")).Result;

            var jresponse = JsonConvert.DeserializeObject<Dictionary<string,object>>(response.Content.ReadAsStringAsync().Result);

            var banks = JsonConvert.DeserializeObject<Dictionary<string, object>[]>(jresponse["accounts"].ToString());
            bankList = banks.Select(bank => {
                var meta = JsonConvert.DeserializeObject<Dictionary<string, object>>(bank["meta"].ToString());
                return new BankAccount() {
                    LoginId = serviceOptions.LoginId,
                    Name = meta["name"].ToString(),
                    ServiceId = bank["_id"].ToString()
                };
            }).ToList();

            if (response.StatusCode == HttpStatusCode.OK) {
                return jresponse["access_token"].ToString();
            } else {
                throw new Exception(response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Will exchange a public token from plaid link to an access token
        /// </summary>
        /// <param name="afterAuthData"></param>
        /// <returns></returns>
        public string RunAfterAuthFunction(PlaidOptions options, string afterAuthData) {
            var body = new {
                client_id = options.ClientId,
                secret = options.ClientSecret,
                public_token = afterAuthData
            };
            var response = Client.PostAsync($"{options.Url}/item/public_token/exchange", new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")).Result;
            var jresponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content.ReadAsStringAsync().Result);
            if (response.StatusCode == HttpStatusCode.OK) {
                return jresponse["access_token"].ToString();
            } else {
                throw new Exception(response.ReasonPhrase, new Exception(jresponse.ToString()));
            }
        }

        public string GetPublicToken(PlaidOptions options, string accessToken) {
            var body = new {
                client_id = options.ClientId,
                secret = options.ClientSecret,
                access_token = accessToken
            };
            var response = Client.PostAsync($"{options.Url}/item/public_token/create", new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")).Result;
            var jresponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content.ReadAsStringAsync().Result);
            if (response.StatusCode == HttpStatusCode.OK) {
                return jresponse["public_token"].ToString();
            } else {
                throw new Exception(response.ReasonPhrase, new Exception(jresponse.ToString()));
            }
        }

        public List<BankAccount> GetAccounts(PlaidOptions options, Guid loginId, string token) {
            var body = new {
                client_id = options.ClientId,
                secret = options.ClientSecret,
                access_token = token
            };
            var response = Client.PostAsync($"{options.Url}/accounts/get", new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")).Result;
            var jresponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content.ReadAsStringAsync().Result);
            if (response.StatusCode != HttpStatusCode.OK) {
                throw new Exception(response.ReasonPhrase, new Exception(jresponse.ToString()));
            }

            var accountList = new List<BankAccount>();
            var accounts = JsonConvert.DeserializeObject<Dictionary<string, object>[]>(jresponse["accounts"].ToString());
            foreach (Dictionary<string, object> account in accounts) {
                accountList.Add(new BankAccount() {
                    LoginId = loginId,
                    Name = account["name"].ToString(),
                    ServiceId = account["account_id"].ToString(),
                    Shown = true
                });
            }

            return accountList;
        }

        public List<AccountTransaction> GetRecentTransactions(PlaidOptions options, string token, DateTime start, DateTime end) {
            var response = RetreiveAccountsAndTransactions(options, token, start, end);

            var transactionList = new List<AccountTransaction>();
            Dictionary<string, object> jresponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
            var transactions = JsonConvert.DeserializeObject<Dictionary<string, object>[]>(jresponse["transactions"].ToString());
            foreach (Dictionary<string, object> transaction in transactions) {
                transactionList.Add(new AccountTransaction() {
                    Date = DateTime.Parse(transaction["date"].ToString()),
                    Amount = Decimal.Negate(Convert.ToDecimal(transaction["amount"])),
                    Description = transaction["name"].ToString(),
                    ServiceId = transaction["transaction_id"].ToString(),
                    ServiceAccountId = transaction["account_id"].ToString()
                });
            }

            return transactionList;
        }

        private string RetreiveAccountsAndTransactions(PlaidOptions options, string token, DateTime start, DateTime end) {
            var body = new {
                client_id = options.ClientId,
                secret = options.ClientSecret,
                access_token = token,
                start_date = start.ToString("yyyy-MM-dd"),
                end_date = end.ToString("yyyy-MM-dd"),
                options = new {
                    count = 250
                }
            };
            var response = Client.PostAsync($"{options.Url}/transactions/get", new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")).Result;

            var content = response.Content.ReadAsStringAsync().Result;
            if (response.StatusCode != HttpStatusCode.OK) {
                throw new BankServiceException(content);
            }
            return content;
        }

        public string DeleteUser(PlaidOptions options, string token) {
            var body = new {
                client_id = options.ClientId,
                secret = options.ClientSecret,
                access_token = token
            };
            var response = Client.SendAsync(
                new HttpRequestMessage(HttpMethod.Delete,
                    $"{options.Url}/connect/get") {
                    Content = new StringContent(JsonConvert.SerializeObject(body))
                }).Result;

            var content = response.Content.ReadAsStringAsync().Result;
            if (response.StatusCode != HttpStatusCode.OK) {
                throw new BankServiceException(content);
            }
            return content;
        }
    }
}

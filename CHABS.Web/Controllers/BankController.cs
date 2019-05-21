using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CHABS.API;
using CHABS.API.Objects;
using CHABS.API.Services;
using CHABS.API.Services.DataServices;
using CHABS.Models;
using CHABS.Web;
using CHABS.Web.Models;
using CHABS.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CHABS.Controllers {
	public class BankController : BaseController {
		private readonly DataService Services;
		private readonly IBankDataService BankService;

	    private readonly UserManager<ApplicationUser> _userManager;
	    private readonly SignInManager<ApplicationUser> _signInManager;
	    private readonly ILogger _logger;
	    private readonly ConnectionStrings _connStrings;
	    private readonly PlaidOptions _plaidOptions;

        public BankController(
	        UserManager<ApplicationUser> userManager,
	        SignInManager<ApplicationUser> signInManager,
	        IOptions<ConnectionStrings> connStrings,
	        IOptions<PlaidOptions> plaidOptions,
            ILogger<BankController> logger)
	    {
	        _userManager = userManager;
	        _signInManager = signInManager;
	        _logger = logger;
	        _connStrings = connStrings.Value;
	        _plaidOptions = plaidOptions.Value;
            
            AppSession.ConnectionString = _connStrings.DefaultConnection;

		    Services = new DataService(AppSession);
			BankService = new PlaidService();
        }

        #region Logins
        public ActionResult Logins() {
			var model = new BankLoginViewModel();
			model.CurrentLogins =
				Services.BankConnections.GetAllForHousehold();

            foreach (var login in model.CurrentLogins) {
                login.PublicToken = (BankService as PlaidService)?.GetPublicToken(_plaidOptions, login.AccessToken);
            }

            model.PlaidEnv = _plaidOptions.Env;
			return View(model);
		}

		[HttpPost]
		public ActionResult Logins(IFormCollection form) {
			var publicToken = form["public_token"];
			
			// Save the token in case of failure
			Services.BankConnections.SavePublicToken(publicToken);

			// Exchange for accessToken
			var accessToken = BankService.RunAfterAuthFunction(_plaidOptions, publicToken);

			// Save login
			var login = new BankConnection() {
				Institution = "plaid_link",
				Name = form["name"],
				HouseholdId = GetHouseholdIdForCurrentUser(),
				AccessToken = accessToken
			};
			Services.BankConnections.Upsert(login);

			// Delete the token after a successful translation
			Services.BankConnections.DeletePublicToken(publicToken);

			// Get accounts
			var accounts = BankService.GetAccounts(_plaidOptions, login.Id, accessToken);
			// Save accounts
			foreach (BankAccount bank in accounts) {
				Services.BankAccounts.Upsert(bank);
			}

			return RedirectToAction("Logins");
	    }

	    [HttpPost]
	    public ActionResult UpdateLogin(IFormCollection form) {
	        var publicToken = form["public_token"];
	        var loginId = form["login_id"];

            // Save the token in case of failure
            Services.BankConnections.SavePublicToken(publicToken);

	        // Exchange for accessToken
	        var accessToken = BankService.RunAfterAuthFunction(_plaidOptions, publicToken);

            // Save login
	        var logins = Services.BankConnections.GetAllForHousehold();
	        var login = logins.First(l => l.Id == loginId);
	        login.AccessToken = accessToken;
	        Services.BankConnections.Upsert(login);

	        // Delete the token after a successful translation
	        Services.BankConnections.DeletePublicToken(publicToken);

	        // Get accounts
	        var accounts = BankService.GetAccounts(_plaidOptions, login.Id, accessToken);
	        // Save accounts
	        foreach (BankAccount bank in accounts) {
	            Services.BankAccounts.Upsert(bank);
	        }

	        return RedirectToAction("Logins");
		}

		public ActionResult Refresh(Guid loginId) {
			var login = Services.BankConnections.GetById(loginId);

			var accounts = BankService.GetAccounts(_plaidOptions, loginId, login.AccessToken);

			foreach (var bankAccount in accounts) {
				var account = Services.BankAccounts.GetSingle("serviceid = @serviceid", new {serviceid = bankAccount.ServiceId});
				if (account != null) {
					bankAccount.Import(account);
				}
				Services.BankAccounts.Upsert(bankAccount);
			}

			return RedirectToAction("Logins");
		}

		public ActionResult DeleteLogin(Guid Id) {
			var login = Services.BankConnections.GetById(Id);
			Services.BankConnections.DeleteObject(login);
			// Delete the BankDataService user
			var response = BankService.DeleteUser(_plaidOptions, login.AccessToken);

			return RedirectToAction("Logins");
		}
		#endregion

		#region LoginAccounts
		public ActionResult AccountList(Guid loginId) {
			var accounts = Services.BankAccounts.GetListByLogin(loginId).ToList();
			var login = Services.BankConnections.GetById(loginId);
			var model = new AccountListViewModel();
			model.Accounts = accounts;
			model.LoginName = login.Name;
			return View(model);
		}

		public ActionResult ToggleAccount(Guid accountId, Guid loginId) {
			// Toggle the account
			var account = Services.BankAccounts.GetById(accountId);
			account.Shown = !account.Shown;
			Services.BankAccounts.Upsert(account);

			return RedirectToAction("AccountList", new {loginId});
		}
	    [HttpPost]
	    [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBankLoginAccount(BankAccount account) {
	        if (ModelState.IsValid) {
	            account.IsNew = false;
		        var existing = Services.BankAccounts.GetById(account.Id);
		        existing.Import(account, "LoginId");
				Services.BankAccounts.Upsert(existing);
                return Json(new { success = true, message = "Account saved." });
	        }

	        return Json(new { success = false, message = "There was an error saving." });
        }
        #endregion
		
		public ActionResult TransactionList(int monthsDifference = 0) {
			var logins = Services.BankConnections.GetListForHousehold(GetHouseholdIdForCurrentUser());

			var startDate = DateTime.Now.AddMonths(monthsDifference).FirstDay();
			var endDate = DateTime.Now.AddMonths(monthsDifference).LastDay();

			var model = new TransactionsViewModel(AppSession, Services.AccountTransactions.GetTransactionsByDateRange(logins, startDate, endDate));

			model.MonthsDifference = monthsDifference;
			model.StartDate = startDate;
			model.EndDate = endDate;

			model.RangeString = $"Transactions from {model.StartDate:d} to {model.EndDate:d}";

			return View(model);
		}
		
		// Update
		public ActionResult Update() {
			return View();
		}

		[HttpPost]
		public ActionResult FetchTransactions(int monthsDifference = 0) {
			var success = false;
			var message = "";
			try {
				var startDate = DateTime.Now.AddMonths(monthsDifference).FirstDay();
				var endDate = DateTime.Now.AddMonths(monthsDifference).LastDay();

				var loginIds = Services.BankConnections.GetListForHousehold(GetHouseholdIdForCurrentUser());
				var options = new TransactionUpdateService.ExecutionContext(loginIds, GetCurrentUserGuid(), Services, BankService);
				TransactionUpdateService.DoTransactionUpdate(_plaidOptions, options, startDate, endDate);
				success = true;
				message = "Transaction Update was successful";
			} catch (Exception ex) {
				message = ex.Message;
			}

			return PartialView("CallbackStatus", new CallbackStatusViewModel() {
				Success = success,
				Message = message
			});
		}
		// ----------
		
		public ActionResult MatchTransactions(int monthsDifference = 0) {
			var success = false;
			var message = "";
			try {
				var logins = Services.BankConnections.GetListForHousehold(GetHouseholdIdForCurrentUser());
				var startDate = DateTime.Now.AddMonths(monthsDifference).FirstDay();
				var endDate = DateTime.Now.AddMonths(monthsDifference).LastDay();
				var transactions = Services.AccountTransactions.GetTransactionsByDateRange(logins, startDate, endDate);

				foreach (var transaction in transactions) {
					// Get the source name
					var source = Services.BankAccounts.GetSingle("serviceid = @serviceid", new { serviceid = transaction.ServiceAccountId });
					transaction.Source = source.DisplayName;
					// Map categories
					var category = Services.Categories.FindCategoryMatch(transaction.Description);
					if (category != null) {
						transaction.Category = category.Name;
					}
					// Save if a new transaction
					Services.AccountTransactions.Upsert(transaction);
				}

				success = true;
				message = "Transaction Update was successful";
			} catch (Exception ex) {
				message = ex.Message;
			}

			return PartialView("CallbackStatus", new CallbackStatusViewModel() {
				Success = success,
				Message = message
			});
		}

		public ActionResult UploadAmazonOrders() {
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UploadAmazonOrders(UploadAmazonTransactionsViewModel model) {
			if (ModelState.IsValid) {
				// Upload the image to blob
				if (model.Orders != null) {
					var stream = new MemoryStream();
					await model.Orders.CopyToAsync(stream);
					stream.Position = 0;
					var reader = new StreamReader(stream);

					var items = AmazonOrderService.ParseOrderCsv(reader);
					var orders = AmazonOrderService.SaveAmazonOrderList(AppSession, items);
					AmazonOrderService.AttachAmazonOrders(AppSession, orders);
				}
			}

			return View();
		}


		private string BuildHookUrl() {
			return string.Format("{0}://{1}{2}Bank/BankServiceHook", Request.Scheme, Request.PathBase, Url.Content("~"));
		}
	}
}
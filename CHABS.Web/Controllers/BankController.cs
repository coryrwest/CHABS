using System;
using System.Collections.Generic;
using System.Linq;
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
	            Services.BankAccounts.Upsert(account);
                return Json(new { success = true, message = "Account saved." });
	        }

	        return Json(new { success = false, message = "There was an error saving." });
        }
        #endregion
		
		public ActionResult TransactionList() {
			var logins = Services.BankConnections.GetListForHousehold(GetHouseholdIdForCurrentUser());

			var model = new TransactionsViewModel();
			model.RangeString = "Transactions this month";
			model.Transations = Services.AccountTransactions.GetThisMonthsTransactions(logins);
			return View(model);
		}
		
		// Update
		public ActionResult Update() {
			return View();
		}

		[HttpPost]
		public ActionResult UpdateTransactions() {
			var success = false;
			var message = "";
			try {
				var loginIds = Services.BankConnections.GetListForHousehold(GetHouseholdIdForCurrentUser());
				var options = new TransactionUpdateService.ExecutionContext(loginIds, GetCurrentUserGuid(), Services, BankService);
				TransactionUpdateService.DoTransactionUpdate(_plaidOptions, options, DateTime.Now.FirstDay(), DateTime.Now.LastDay());
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

		private string BuildHookUrl() {
			return string.Format("{0}://{1}{2}Bank/BankServiceHook", Request.Scheme, Request.PathBase, Url.Content("~"));
		}
	}
}
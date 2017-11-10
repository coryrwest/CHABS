using System;
using System.Collections.Generic;
using System.Linq;
using CHABS.API;
using CHABS.API.Objects;
using CHABS.API.Services;
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
		private readonly BankAccountService Service;
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

            Service = new BankAccountService(AppSession);
	        BankService = new PlaidService();
        }

        #region Logins
        public ActionResult Logins() {
			var model = new BankLoginViewModel();
			model.CurrentLogins =
				Service.Logins.GetAllForHousehold();
            model.PlaidEnv = _plaidOptions.Env;
			return View(model);
		}

		[HttpPost]
		public ActionResult Logins(IFormCollection form) {
			var publicToken = form["public_token"];
			
			// Save the token in case of failure
			Service.Logins.SavePublicToken(publicToken);

			// Exchange for accessToken
			var accessToken = BankService.RunAfterAuthFunction(_plaidOptions, publicToken);

			// Save login
			var login = new BankLogin() {
				Institution = "plaid_link",
				Name = form["name"],
				HouseholdId = GetHouseholdIdForCurrentUser(),
				AccessToken = accessToken
			};
			Service.Logins.Upsert(login);

			// Delete the token after a successful translation
			Service.Logins.DeletePublicToken(publicToken);

			// Get accounts
			var accounts = BankService.GetAccounts(_plaidOptions, login.Id, accessToken);
			// Save accounts
			foreach (BankLoginAccount bank in accounts) {
				Service.Accounts.Upsert(bank);
			}

			return RedirectToAction("Logins");
		}
        
		public ActionResult DeleteLogin(Guid loginId) {
			var login = Service.Logins.GetById(loginId);
			Service.Logins.DeleteObject(login);
			// Delete the BankDataService user
			var response = BankService.DeleteUser(_plaidOptions, login.AccessToken);

			var current = Service.Logins.GetAllForHousehold();

			return PartialView("LoginListPartial", new LoginListViewModel(current));
		}
		#endregion

		#region LoginAccounts
		public ActionResult AccountList(Guid loginId) {
			var accounts = Service.Accounts.GetList(new { loginId }).OrderBy(a => a.Name).ToList();
			var login = Service.Logins.GetById(loginId);
			var model = new AccountListViewModel();
			model.Accounts = accounts;
			model.LoginName = login.Name;
			return View(model);
		}

		public ActionResult ToggleAccount(Guid accountId, Guid loginId) {
			// Toggle the account
			var account = Service.Accounts.GetById(accountId);
			account.Shown = !account.Shown;
			Service.Accounts.Upsert(account);

			return RedirectToAction("AccountList", new {loginId});
		}
		#endregion

		[HttpPost]
		public void BankServiceHook(string loginId) {
			//TransactionUpdate(loginId.ToGuid());
		}

		public ActionResult TransactionList() {
			var logins = Service.Logins.GetHouseholdLoginIds(GetHouseholdIdForCurrentUser());

			var model = new TransactionsViewModel();
			model.RangeString = "Transactions this month";
			model.Transations = Service.Transactions.GetThisMonthsTransactions(logins);
			return View(model);
		}

		#region Categories
		public ActionResult Categories() {
			var model = new CategoriesViewModel();
			model.CurrentCategories =
				Service.Categories.GetAll(true).ToList();

			return View(model);
		}

		[HttpPost]
		public ActionResult Categories(CategoriesViewModel model) {
			Service.Categories.Upsert(new Category() {
				Name = model.Name,
				HouseholdId = GetHouseholdIdForCurrentUser(),
				Excluded = model.Excluded
			});

			model.CurrentCategories =
				Service.Categories.GetAll(true);
			return PartialView("CategoryListPartial", new CategoriesListViewModel(model.CurrentCategories));
		}

		public ActionResult EditCategory(Guid id) {
			var category = Service.Categories.GetById(id);
			return View(category);
		}

		[HttpPost]
		public ActionResult EditCategory(Category category) {
			category.IsNew = false;
			Service.Categories.Upsert(category);
			return RedirectToAction("Categories");
		}

		public ActionResult ToggleCategory(Guid id) {
			// Toggle the account
			var category = Service.Categories.GetById(id);
			category.Excluded = !category.Excluded;
			Service.Categories.Upsert(category);

			return RedirectToAction("Categories");
		}

		public ActionResult DeleteCategory(Guid id) {
			Service.Categories.Delete(id);
			return RedirectToAction("Categories");
		}

		public ActionResult RestoreCategory(Guid id) {
			Service.Categories.Restore(id);
			return RedirectToAction("Categories");
		}

		public ActionResult CategoryMatches(Guid id) {
			var model = new CategoryMatchesViewModel();
			model.CurrentCategoryMatches =
				Service.CategoryMatches.GetList(new { categoryid = id });
			model.CategoryName = Service.Categories.GetById(id).Name;
			model.CategoryId = id;

			return View(model);
		}

		[HttpPost]
		public ActionResult CategoryMatches(CategoryMatchesViewModel model) {
			Service.CategoryMatches.Upsert(new CategoryMatch() {
				Match = model.Match,
				CategoryId = model.CategoryId
			});

			model.CurrentCategoryMatches =
				Service.CategoryMatches.GetList(new { categoryid = model.CategoryId });
			return PartialView("CategoryMatchListPartial", new CategoryMatchesListViewModel(model.CurrentCategoryMatches));
		}

		public ActionResult DeleteCategoryMatch(Guid id, Guid categoryId) {
			Service.CategoryMatches.Delete(id);
			return RedirectToAction("CategoryMatches", new { id = categoryId });
		}
		#endregion

		// Update
		public ActionResult Update() {
			return View();
		}

		[HttpPost]
		public ActionResult UpdateTransactions() {
			var success = false;
			var message = "";
			try {
				var loginIds = Service.Logins.GetHouseholdLoginIds(GetHouseholdIdForCurrentUser());
				var options = new TransactionUpdateService.Options(BankService, Service, loginIds, GetCurrentUserGuid());
				TransactionUpdateService.DoTransactionUpdate(_plaidOptions, options);
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
﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlTypes;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Antlr.Runtime.Misc;
using CHABS.API.Objects;
using CHABS.API.Services;
using CHABS.Models;
using CRWestropp.Utilities.Extensions;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security.Twitter.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CHABS.Controllers {
	public class BankController : BaseController {
		private readonly BankAccountService Service;
		private readonly UserRoleService UserService;
		private readonly IBankDataService BankService;

		public BankController() {
			Service = new BankAccountService(AppSession);
			BankService = new PlaidService();
			UserService = new UserRoleService(AppSession);
		}

		#region Logins
		public ActionResult Logins() {
			var institutions = BankService.GetInstitutions().Result;

			var model = new BankLoginViewModel();
			model.CurrentLogins =
				Service.Logins.GetList(new { HouseholdId = GetHouseholdIdForCurrentUser() });
			model.InstitutionList = new SelectList(institutions.Pairs, "Key", "Value");

			return View(model);
		}

		[HttpPost]
		public ActionResult Logins(FormCollection form) {
			//Service.Logins.Upsert(new BankLogin() {
			//	Institution = model.Institution,
			//	Name = model.Name,
			//	UserId = GetCurrentUserGuid()
			//});

			//model.CurrentLogins =
			//	Service.Logins.GetList(new { UserId = GetCurrentUserGuid() });
			//return PartialView("LoginListPartial", new LoginListViewModel(model.CurrentLogins));

			// Exchange for accessToken
			var publicToken = form["public_token"];
			var accessToken = BankService.ExchangePublicToken(publicToken);

			// Save login
			var login = new BankLogin() {
				Institution = "plaid_link",
				Name = form["Name"],
				HouseholdId = GetHouseholdIdForCurrentUser(),
				AccessToken = accessToken
			};
			Service.Logins.Upsert(login);

			// Get accounts
			var accounts = BankService.GetAccounts(login.Id, accessToken).Result;
			// Save accounts
			foreach (BankLoginAccount bank in accounts) {
				Service.Accounts.Upsert(bank);
			}

			return PartialView("LoginListPartial", new LoginListViewModel(new List<BankLogin>()));
		}

		public ActionResult ConnectLogin(Guid loginId) {
			var login = Service.Logins.GetById(loginId);
			var model = new ConnectLoginViewModel();
			model.Login = login;
			return View(model);
		}

		[HttpPost]
		public ActionResult ConnectLogin(ConnectLoginViewModel model) {
			model.Login = Service.Logins.GetById(model.LoginId);
			var hookUrl = string.Format("{0}?loginId={1}", BuildHookUrl(), model.Login.Id);
			var options = new BankDataServiceOptions(model.Username, model.Password, model.Login.Institution, hookUrl,
				model.LoginId);
			var bankList = new List<BankLoginAccount>();
			var token = BankService.AuthenticateBankUser(options, out bankList);
			model.Login.AccessToken = token;
			Service.Logins.Upsert(model.Login);

			// Save banks
			foreach (BankLoginAccount bank in bankList) {
				Service.Accounts.Upsert(bank);
			}

			return RedirectToAction("AccountList", new {model.LoginId});
		}

		public ActionResult DeleteLogin(Guid loginId) {
			Service.Logins.Delete(loginId);

			var current = Service.Logins.GetList(new { HouseholdId = GetHouseholdIdForCurrentUser() });

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

		public ActionResult Transactions() {
			var logins = Service.Logins.GetList(new { householdid = GetHouseholdIdForCurrentUser() }).Select(l => l.Id).ToList();

			var model = new TransactionsViewModel();
			model.RangeString = "Last 30 days";
			model.Transations = Service.Transactions.GetLast30DaysTransactions(logins);
			return View(model);
		}

		#region Categories
		public ActionResult Categories() {
			var model = new CategoriesViewModel();
			model.CurrentCategories =
				Service.Categories.GetAll().OrderBy(c => c.Sort).ToList();

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
				Service.Categories.GetList(new { HouseholdId = GetHouseholdIdForCurrentUser() });
			return PartialView("CategoryListPartial", new CategoriesListViewModel(model.CurrentCategories));
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
				var loginIds = Service.Logins.GetUserLoginIds(GetCurrentUserGuid());
				var options = new TransactionUpdateService.Options(BankService, Service, loginIds, GetCurrentUserGuid());
				TransactionUpdateService.DoTransactionUpdate(options);
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
			return string.Format("{0}://{1}{2}Bank/BankServiceHook", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
		}
	}
}
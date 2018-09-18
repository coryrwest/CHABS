using System;
using CHABS.API;
using CHABS.API.Objects;
using CHABS.API.Services;
using CHABS.API.Services.DataServices;
using CHABS.Models;
using CHABS.Web;
using CHABS.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CHABS.Controllers {
	public class BudgetsController : BaseController {
		private readonly DataService Services;

		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly ILogger _logger;
		private readonly ConnectionStrings _connStrings;

		public BudgetsController(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			IOptions<ConnectionStrings> connStrings,
			ILogger<BankController> logger) {
			_userManager = userManager;
			_signInManager = signInManager;
			_logger = logger;
			_connStrings = connStrings.Value;

			AppSession.ConnectionString = _connStrings.DefaultConnection;

			Services = new DataService(AppSession);
		}

		public ActionResult Index() {
			var budgets = Services.Budgets.GetAllForHousehold(true);
			var categories = Services.Categories.GetAllForHousehold(true);
			var model = new BudgetViewModel();
			model.CurrentBudgets = budgets;
			model.Categories = new SelectList(categories, "Id", "Name");
			return View(model);
		}

		[HttpPost]
		public ActionResult Index(BudgetViewModel model) {
			var budget = new Budget();
			budget.Name = model.Name;
			budget.Amount = model.Amount;
			budget.HouseholdId = GetHouseholdIdForCurrentUser();
			Services.Budgets.Upsert(budget);
			// Save the map
			var map = new BudgetCategoryMap();
			map.BudgetId = budget.Id;
			map.CategoryId = model.CategoryId;
			Services.BudgetCategoryMaps.Upsert(map);

			model.CurrentBudgets = Services.Budgets.GetAllForHousehold(true);
			var categories = Services.Categories.GetAllForHousehold(true);
			model.Categories = new SelectList(categories, "Name", "Name");
			return PartialView("BudgetListPartial", new BudgetListViewModel(model.CurrentBudgets));
		}

		public ActionResult EditBudget(Guid id) {
			var budget = Services.Budgets.GetById(id);
			return View(budget);
		}

		[HttpPost]
		public ActionResult EditBudget(Budget budget) {
			budget.IsNew = false;
			Services.Budgets.Upsert(budget);
			return RedirectToAction("Index");
		}

		public ActionResult DeleteBudget(Guid id) {
			var budget = Services.Budgets.GetById(id);
			Services.Budgets.DeleteObject(budget);
			return RedirectToAction("Index");
		}

		public ActionResult RestoreBudget(Guid id) {
			Services.Budgets.Restore(id);
			return RedirectToAction("Index");
		}

		#region Budget Categories
		public ActionResult Categories(Guid id) {
			var categories = Services.Categories.GetAllForBudget(id);
			var allCategories = Services.Categories.GetAllForHousehold();
			var model = new BudgetCategoryViewModel();
			model.CurrentBudgetCategorys = categories;
			model.BudgetId = id;
			model.Categories = new SelectList(allCategories, "Id", "Name");
			return View(model);
		}

		[HttpPost]
		public ActionResult Categories(BudgetCategoryViewModel model) {
			var map = new BudgetCategoryMap();
			map.BudgetId = model.BudgetId;
			map.CategoryId = model.CategoryId;
			Services.BudgetCategoryMaps.Upsert(map);

			var categories = Services.Categories.GetAllForBudget(model.BudgetId);
			return PartialView("BudgetCategoryListPartial", new BudgetCategoryListViewModel(categories));
		}

		public ActionResult RemoveCategory(Guid id, Guid budgetId) {
			Services.BudgetCategoryMaps.DeleteMap(id, budgetId);
			return RedirectToAction("Index");
		}
		#endregion
	}
}
using System;
using System.Web.Mvc;
using CHABS.API.Objects;
using CHABS.API.Services;
using CHABS.Models;
using CRWestropp.Utilities.Extensions;

namespace CHABS.Controllers {
	public class BudgetsController : BaseController {
		private readonly BankAccountService Service;

		public BudgetsController() {
			Service = new BankAccountService(AppSession);
		}

		public ActionResult Index() {
			var budgets = Service.Budgets.GetAllForHousehold(true);
			var categories = Service.Categories.GetAllForHousehold(true);
			var model = new BudgetViewModel();
			model.CurrentBudgets = budgets;
			model.Categories = new SelectList(categories, "Id", "Name");
			return View(model);
		}

		[HttpPost]
		public ActionResult Index(BudgetViewModel model) {
			var budget = new Budget();
			model.CopyProperties(budget);
			budget.HouseholdId = GetHouseholdIdForCurrentUser();
			Service.Budgets.Upsert(budget);
			// Save the map
			var map = new BudgetCategoryMap();
			map.BudgetId = budget.Id;
			map.CategoryId = model.CategoryId;
			Service.BudgetCategoryMaps.Upsert(map);

			model.CurrentBudgets = Service.Budgets.GetAllForHousehold(true);
			var categories = Service.Categories.GetAllForHousehold(true);
			model.Categories = new SelectList(categories, "Name", "Name");
			return PartialView("BudgetListPartial", new BudgetListViewModel(model.CurrentBudgets));
		}

		public ActionResult EditBudget(Guid id) {
			var budget = Service.Budgets.GetById(id);
			return View(budget);
		}

		[HttpPost]
		public ActionResult EditBudget(Budget budget) {
			budget.IsNew = false;
			Service.Budgets.Upsert(budget);
			return RedirectToAction("Index");
		}

		public ActionResult DeleteBudget(Guid id) {
			var budget = Service.Budgets.GetById(id);
			Service.Budgets.DeleteObject(budget);
			return RedirectToAction("Index");
		}

		public ActionResult RestoreBudget(Guid id) {
			Service.Budgets.Restore(id);
			return RedirectToAction("Index");
		}
	}
}
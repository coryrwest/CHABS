using System;
using CHABS.API.Objects;
using CHABS.API.Services;
using CHABS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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

		//[HttpPost]
		//public ActionResult Index(BudgetViewModel model) {
		//	var budget = new Budget();
		//	model.CopyProperties(budget);
		//	budget.HouseholdId = GetHouseholdIdForCurrentUser();
		//	Service.Budgets.Upsert(budget);
		//	// Save the map
		//	var map = new BudgetCategoryMap();
		//	map.BudgetId = budget.Id;
		//	map.CategoryId = model.CategoryId;
		//	Service.BudgetCategoryMaps.Upsert(map);

		//	model.CurrentBudgets = Service.Budgets.GetAllForHousehold(true);
		//	var categories = Service.Categories.GetAllForHousehold(true);
		//	model.Categories = new SelectList(categories, "Name", "Name");
		//	return PartialView("BudgetListPartial", new BudgetListViewModel(model.CurrentBudgets));
		//}

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

		#region Budget Categories
		public ActionResult Categories(Guid id) {
			var categories = Service.Categories.GetAllForBudget(id);
			var allCategories = Service.Categories.GetAllForHousehold();
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
			Service.BudgetCategoryMaps.Upsert(map);

			var categories = Service.Categories.GetAllForBudget(model.BudgetId);
			return PartialView("BudgetCategoryListPartial", new BudgetCategoryListViewModel(categories));
		}

		public ActionResult RemoveCategory(Guid id, Guid budgetId) {
			var budgetCategory = Service.BudgetCategoryMaps.GetSingle(new {categoryid = id, budgetid = budgetId});
			Service.BudgetCategoryMaps.DeleteObject(budgetCategory);
			return RedirectToAction("Index");
		}
		#endregion
	}
}
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
	public class CategoriesController : BaseController {
		private readonly DataService Services;

		private readonly UserManager<ApplicationUser> _userManager;
	    private readonly SignInManager<ApplicationUser> _signInManager;
	    private readonly ILogger _logger;
	    private readonly ConnectionStrings _connStrings;
	    private readonly PlaidOptions _plaidOptions;

        public CategoriesController(
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
		}

		public ActionResult Index() {
			var model = new CategoriesViewModel();
			model.CurrentCategories =
				Services.Categories.GetAll(true).ToList();

			return View(model);
		}

		[HttpPost]
		public ActionResult Index(CategoriesViewModel model) {
			Services.Categories.Upsert(new Category() {
				Name = model.Name,
				HouseholdId = GetHouseholdIdForCurrentUser(),
				Excluded = model.Excluded
			});

			model.CurrentCategories =
				Services.Categories.GetAll(true).ToList();
			return PartialView("CategoryListPartial", new CategoriesListViewModel(model.CurrentCategories));
		}

		public ActionResult EditCategory(Guid id) {
			var category = Services.Categories.GetById(id);
			return View(category);
		}

		[HttpPost]
		public ActionResult EditCategory(Category category) {
			category.IsNew = false;
			Services.Categories.Upsert(category);
			return RedirectToAction(nameof(Index));
		}

		public ActionResult ToggleCategory(Guid id) {
			// Toggle the account
			var category = Services.Categories.GetById(id);
			category.Excluded = !category.Excluded;
			Services.Categories.Upsert(category);

			return RedirectToAction(nameof(Index));
		}

		public ActionResult DeleteCategory(Guid id) {
			Services.Categories.Delete(id);
			return RedirectToAction(nameof(Index));
		}

		public ActionResult RestoreCategory(Guid id) {
			Services.Categories.Restore(id);
			return RedirectToAction(nameof(Index));
		}

		public ActionResult Matches(Guid id) {
			var model = new CategoryMatchesViewModel();
			model.CurrentCategoryMatches =
				Services.CategoryMatches.GetListForCategory(id).ToList();
			model.CategoryName = Services.Categories.GetById(id).Name;
			model.CategoryId = id;

			return View(model);
		}

		[HttpPost]
		public ActionResult Matches(CategoryMatchesViewModel model) {
			Services.CategoryMatches.Upsert(new CategoryMatch() {
				Match = model.Match,
				CategoryId = model.CategoryId
			});

			model.CurrentCategoryMatches =
				Services.CategoryMatches.GetListForCategory(model.CategoryId).ToList();
			return PartialView("CategoryMatchListPartial", new CategoryMatchesListViewModel(model.CurrentCategoryMatches));
		}

		public ActionResult DeleteCategoryMatch(Guid id, Guid categoryId) {
			Services.CategoryMatches.Delete(id);
			return RedirectToAction(nameof(Matches), new { id = categoryId });
		}
	}
}
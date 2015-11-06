using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CHABS.API.Services;
using CHABS.Models;

namespace CHABS.Controllers {
	public class HouseholdController : BaseController {
		private readonly HouseholdService Service;
		private readonly UserRoleService UserService;

		public HouseholdController() {
			Service = new HouseholdService(AppSession);
			UserService = new UserRoleService(AppSession);
		}

		// GET: Household
		public ActionResult Index() {
			var model = new HouseholdViewModel();
			var household = GetHouseholdForCurrentUser();
			model.CurrentHouseholdName = household.Name;
			model.HouseholdUsers = Service.HouseholdMaps.GetUsernamesForHousehold(household.Id);
			return View(model);
		}

		[HttpPost]
		public ActionResult Index(HouseholdViewModel model) {
			// Get the user by email
			if (model.UserEmail != null) {
				var user = UserService.Users.GetByEmail(model.UserEmail);
				if (user == null) ModelState.AddModelError("", "There is no user by that email address. Confirm that the email address you entered is correct.");
				
				if (ModelState.IsValid) {
					Service.HouseholdMaps.AddUserToHousehold(user.Id, GetHouseholdIdForCurrentUser());
				}
			}

			if (ModelState.IsValid) {
				// Save the new household name if necessary
				var household = GetHouseholdForCurrentUser();
				if (model.CurrentHouseholdName != household.Name) {
					household.Name = model.CurrentHouseholdName;
					Service.Households.Upsert(household);
				}
			}

			return View(model);
		}
	}
}
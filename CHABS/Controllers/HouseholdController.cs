using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CHABS.API.Objects;
using CHABS.API.Services;
using CHABS.Models;
using CRWestropp.Utilities;

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
			var household = GetHouseholdForCurrentUser();

			// Get the user by email
			if (model.UserEmail != null) {
				var user = UserService.Users.GetByEmail(model.UserEmail);
				if (user == null) {
					// Send an invite to be added to the household.
					var options = new Emailer.EmailOptions() {
						SmtpServer = ConfigurationManager.AppSettings["SmtpServer"],
						Port = 587,
						Username = ConfigurationManager.AppSettings["SmtpUser"],
						Password = ConfigurationManager.AppSettings["SmtpPassword"]
					};
					// Build new invited user and save before email
					var iuser = new InvitedUser() {
						Email = model.UserEmail,
						HouseholdId = household.Id,
						Token = Guid.NewGuid().ToString()
					};
					UserService.InvitedUsers.Upsert(iuser);
					var baseUrl = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
					var link = string.Format("{0}Account/Register/{1}", baseUrl, iuser.Token);
					try {
						Emailer.SendEmail(options, iuser.Email, "postmark@crwest.com",
						"You have been invited to join a household on CHABS.",
						"Click the link below to register in order to access the household. " + link);
					} catch (Exception ex) {
						// If we fail to send the email, delete the iuser
						UserService.InvitedUsers.DeleteObject(iuser);
						ModelState.AddModelError("", "Failed to send invite email. Please contact support. " + ex.Message);
					} 
				} else {
					if (ModelState.IsValid) {
						Service.HouseholdMaps.AddUserToHousehold(user.Id, GetHouseholdIdForCurrentUser());
					}
				}
			}

			if (ModelState.IsValid) {
				// Save the new household name if necessary
				if (model.CurrentHouseholdName != household.Name) {
					household.Name = model.CurrentHouseholdName;
					Service.Households.Upsert(household);
				}
			}

			model.CurrentHouseholdName = household.Name;
			model.HouseholdUsers = Service.HouseholdMaps.GetUsernamesForHousehold(household.Id);
			return View(model);
		}
	}
}
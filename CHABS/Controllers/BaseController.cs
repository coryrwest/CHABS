using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CHABS.API.Objects;
using CHABS.API.Services;
using CRWestropp.Utilities.Extensions;
using Microsoft.AspNet.Identity;

namespace CHABS.Controllers {
	public class BaseController : Controller {
		public Guid GetCurrentUserGuid() {
			if (User != null) {
				return User.Identity.GetUserId().ToGuid();
			} else {
				return Guid.Empty;
			}
		}

		public Guid GetHouseholdIdForCurrentUser() {
			return GetHouseholdForCurrentUser().Id;
		}

		public Household GetHouseholdForCurrentUser(Guid userId = default(Guid)) {
			if (AppSession.Household == null) {
				if (userId == Guid.Empty) {
					userId = GetCurrentUserGuid();
				}
				var householdService = new HouseholdService(AppSession);
				var household = householdService.Households.GetHouseholdForUser(userId);
				return household;
			} else {
				return AppSession.Household;
			}
		}

		protected Session AppSession { get; private set; }

		public BaseController() {
			if (AppSession == null) {
				// Get from session first
				if (System.Web.HttpContext.Current.Session["Session"] != null) {
					AppSession = (Session)System.Web.HttpContext.Current.Session["Session"];
				} else {
					AppSession = new Session();
					var UserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
					if (!UserId.IsNull()) {
						var household = GetHouseholdForCurrentUser(UserId.ToGuid());
						AppSession.BuildSession(UserId.ToGuid(), household);
						// Set the session
						System.Web.HttpContext.Current.Session.Add("Session", AppSession);
					}
				}
			}
		}

		protected void ClearSession() {
			AppSession = null;
			System.Web.HttpContext.Current.Session.Add("Session", null);
		}

		protected override void OnActionExecuting(ActionExecutingContext filterContext) {
			base.OnActionExecuting(filterContext);
		}
	}
}
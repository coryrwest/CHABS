using System;
using CHABS.API.Objects;
using CHABS.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CHABS.Controllers {
	public class BaseController : Controller {
		public Guid GetCurrentUserGuid() {
		    return Guid.Parse("69FAD275-0942-41B8-89BB-BDE0D8C540B1");
		    //if (User != null) {
		    //	return User.Identity.GetUserId().ToGuid();
		    //} else {
		    //	return Guid.Empty;
		    //}
		}

		public Guid GetHouseholdIdForCurrentUser() {
			//return GetHouseholdForCurrentUser().Id;
		    return Guid.Parse("65c4f14b-1abd-435b-afe4-5f8e76881e9c");
		}

		//public Household GetHouseholdForCurrentUser(Guid userId = default(Guid)) {
		//	if (AppSession.Household == null) {
		//		if (userId == Guid.Empty) {
		//			userId = GetCurrentUserGuid();
		//		}
		//		var householdService = new HouseholdService(AppSession);
		//		var household = householdService.Households.GetHouseholdForUser(userId);
		//		return household;
		//	} else {
		//		return AppSession.Household;
		//	}
		//}

		protected Session AppSession { get; private set; }

		public BaseController() {
            if (AppSession == null) {
                AppSession = new Session();
                AppSession.BuildSession(GetCurrentUserGuid(), GetHouseholdIdForCurrentUser());
                //// Get from session first
                //if (System.Web.HttpContext.Current.Session["Session"] != null &&
                //    (System.Web.HttpContext.Current.Session["Session"] as Session).UserId != Guid.Empty) {
                //    AppSession = (Session)System.Web.HttpContext.Current.Session["Session"];
                //} else {
                //    AppSession = new Session();
                //    var UserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                //    if (!UserId.IsNull()) {
                //        var household = GetHouseholdForCurrentUser(UserId.ToGuid());
                //        AppSession.BuildSession(UserId.ToGuid(), household);
                //        // Set the session
                //        System.Web.HttpContext.Current.Session.Add("Session", AppSession);
                //    }
                //}
            }
        }

		protected void ClearSession() {
			//AppSession = null;
			//System.Web.HttpContext.Current.Session.Add("Session", null);
		}

	    public override void OnActionExecuting(ActionExecutingContext filterContext) {
			base.OnActionExecuting(filterContext);
		}
	}
}
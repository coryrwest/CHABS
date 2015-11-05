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
			return User.Identity.GetUserId().ToGuid();
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
						AppSession.BuildSession(UserId.ToGuid());
					}
				}
			}
		}

		protected void ClearSession() {
			AppSession = null;
		}

		protected override void OnActionExecuting(ActionExecutingContext filterContext) {
			base.OnActionExecuting(filterContext);
		}
	}
}
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.IO;
using System.Web.Mvc;
using CHABS.API.WorkingBudget;
using DropboxRestAPI;
using Newtonsoft.Json;

namespace CHABS.Controllers {
	public class HomeController : Controller {
		public ActionResult Index() {
			if (User.Identity.IsAuthenticated) {
				return RedirectToAction("Transactions", "Bank");
			} else {
				return RedirectToAction("Login", "Account");
			}
		}
	}
}

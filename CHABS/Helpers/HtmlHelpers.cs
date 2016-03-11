using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CHABS.Helpers {
	public static class HtmlHelpers {
		public static string IsSelected(this HtmlHelper html, string controllers = "", string actions = "", string cssClass = "selected") {
			ViewContext viewContext = html.ViewContext;
			bool isChildAction = viewContext.Controller.ControllerContext.IsChildAction;

			if (isChildAction)
				viewContext = html.ViewContext.ParentActionViewContext;

			RouteValueDictionary routeValues = viewContext.RouteData.Values;
			string currentAction = routeValues["action"].ToString();
			string currentController = routeValues["controller"].ToString();

			if (String.IsNullOrEmpty(actions))
				actions = currentAction;

			if (String.IsNullOrEmpty(controllers))
				controllers = currentController;

			string[] acceptedActions = actions.Trim().Split(',').Distinct().ToArray();
			string[] acceptedControllers = controllers.Trim().Split(',').Distinct().ToArray();

			// Negatives
			string[] negativeActions = acceptedActions.Where(a => a.StartsWith("!")).Select(a => a.Substring(1)).ToArray();

			// Current action is in the list and controller matches
			var actionMatched = acceptedActions.Contains(currentAction) && acceptedControllers.Contains(currentController);
			// Action list is empty and controller matches
			var controllerMatchedAllActions = !acceptedActions.Any(a => !a.StartsWith("!")) && acceptedControllers.Contains(currentController);
			// Current action is in the negative list and controller matches
			var negativeTrigger = negativeActions.Length > 0 && !negativeActions.Contains(currentAction);

			if (actionMatched || controllerMatchedAllActions) {
				return cssClass;
			} else {
				return string.Empty;
			}
		}
	}
}
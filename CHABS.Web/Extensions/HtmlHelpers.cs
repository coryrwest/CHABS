using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace CHABS.Helpers {
	public static class HtmlHelpers {
		public static string IsSelected(this IHtmlHelper html, string controllers = "", string actions = "", string cssClass = "selected") {
			var context = html.ViewContext;
			var routeValues = context.RouteData;
			string currentAction = routeValues.Values["action"].ToString();
			string currentController = routeValues.Values["controller"].ToString();

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
			var controllerMatchedAllActions = acceptedActions.All(a => a.StartsWith("!")) && acceptedControllers.Contains(currentController);
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
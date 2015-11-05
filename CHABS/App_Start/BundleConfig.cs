using System.Web.Optimization;

namespace CHABS {
	public class BundleConfig {
		// For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
		public static void RegisterBundles(BundleCollection bundles) {
			bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
						"~/Scripts/jquery-{version}.js"));

			// Use the development version of Modernizr to develop with and learn from. Then, when you're
			// ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
			bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
						"~/Scripts/modernizr-*"));

			bundles.Add(new ScriptBundle("~/bundles/ajax").Include(
						"~/Scripts/jquery.unobtrusive-ajax.js",
						"~/Scripts/jquery.validate.unobtrusive.js"));

			bundles.Add(new ScriptBundle("~/bundles/form").Include(
						"~/Scripts/jquery.validate.js"));

			bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
						"~/Scripts/bootstrap.js",
						"~/Scripts/respond.js"));

			bundles.Add(new StyleBundle("~/Content/css").Include(
						"~/Content/bootstrap.css",
						"~/Content/site.css",
						"~/Content/datatables.css"));

			bundles.Add(new ScriptBundle("~/bundles/table").Include(
						"~/Scripts/datatables.js"));

			//bundles.Add(new ScriptBundle("~/bundles/react-container").Include(
			//			"~/Scripts/JSXTransformer-{version}.js",
			//			"~/Scripts/react-with-addons.js",
			//			"~/Scripts/sugar.js"));

			//bundles.Add(new JsxBundle("~/bundles/react-app").Include(
			//			"~/Scripts/app/chabs.jsx",
			//			"~/Scripts/app/transactionList.jsx",
			//			"~/Scripts/app/app.jsx"));

			// Set EnableOptimizations to false for debugging. For more information,
			// visit http://go.microsoft.com/fwlink/?LinkId=301862
			BundleTable.EnableOptimizations = false;
		}
	}
}

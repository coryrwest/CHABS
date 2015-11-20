using System;
using System.Configuration;
using System.Data.SqlTypes;
using System.IO;
using System.Web.Mvc;
using CHABS.API.Objects;
using CHABS.API.Services;
using CHABS.API.WorkingBudget;
using CHABS.Models;
using CRWestropp.Utilities.Extensions;
using DropboxRestAPI;
using Microsoft.AspNet.Identity;

namespace CHABS.Controllers {
	public class DropboxController : BaseController {
		private Options options = new Options {
			ClientId = ConfigurationManager.AppSettings["DropboxClientId"],
			ClientSecret = ConfigurationManager.AppSettings["DropboxClientSecret"]
		};
		private readonly UserRoleService Service;
		private readonly BankAccountService BankDataService;

		public DropboxController() {
			Service = new UserRoleService(AppSession);
			BankDataService = new BankAccountService(AppSession);
		}

		public ActionResult Connect() {
			var setting = Service.UserSettings.GetByUserKey(BaseUserSettingService.SettingKeys.DropboxToken, GetCurrentUserGuid());
			if (setting == null) {
				options.RedirectUri = Request.Url.AbsoluteUri.Replace("Connect", "Authorize");
				return ConnectDropBox();
			} else {
				return View("Authorize", new DropboxViewModel() {
					AlreadyConnected = true
				});
			}
		}

		public ActionResult Authorize() {
			options.RedirectUri = Request.Url.AbsoluteUri.Split('?')[0];
			var model = AuthorizeDropBox();

			return View(model);
		}

		public ActionResult Settings() {
			var model = new DropboxSettingsViewModel();
			var path = Service.UserSettings.GetByUserKey("DropboxPath", GetCurrentUserGuid());
			if (path != null) {
				model.FilePath = path.SettingValue;
			}
			var name = Service.UserSettings.GetByUserKey("DropboxFile", GetCurrentUserGuid());
			if (name != null) {
				model.FileName = name.SettingValue;
			}
			return View(model);
		}

		[HttpPost]
		public ActionResult Settings(DropboxSettingsViewModel model) {
			if (!model.FilePath.IsNull()) {
				if (!model.FilePath.StartsWith("/")) {
					model.FilePath = "/" + model.FilePath;
				}
				Service.UserSettings.SaveSetting("DropboxPath", model.FilePath, GetCurrentUserGuid());
			}
			if (!model.FileName.IsNull()) {
				if (!model.FileName.EndsWith(".xlsm")) {
					model.FileName = model.FileName = ".xlsm";
				}
				Service.UserSettings.SaveSetting("DropboxFile", model.FileName, GetCurrentUserGuid());
			}

			return PartialView("SettingsPartial", model);
		}

		/// <summary>
		/// Will cause an immediate redirect
		/// </summary>
		private RedirectResult ConnectDropBox() {
			// Initialize a new Client (without an AccessToken)
			var client = new Client(options);

			// Get the OAuth Request Url
			var authRequestUrl = client.Core.OAuth2.AuthorizeAsync("code").Result;

			return Redirect(authRequestUrl.ToString());
		}

		private DropboxViewModel AuthorizeDropBox() {
			var authCode = Request.QueryString["code"];

			var client = new Client(options);

			var model = new DropboxViewModel();
			if (authCode != null) {
				try {
					// Exchange the Authorization Code with Access/Refresh tokens
					var token = client.Core.OAuth2.TokenAsync(authCode).Result;

					// Get account info
					var accountInfo = client.Core.Accounts.AccountInfoAsync().Result;
					model.UID = accountInfo.uid.ToString();
					model.DisplayName = accountInfo.display_name;
					model.Email = accountInfo.email;

					// Save the token to user settings
					Service.UserSettings.SaveSetting(BaseUserSettingService.SettingKeys.DropboxToken, token.access_token,
						GetCurrentUserGuid());
				} catch (AggregateException ex) {
					model.DisplayName = ex.InnerException.Message;
				} catch (Exception ex) {
					model.DisplayName = ex.Message;
				}
			} else {
				model.DisplayName = "Dropbox was not connected.";
			}

			return model;
		}

		[HttpPost]
		public ActionResult UpdateSpreadsheet() {
			var success = false;
			var message = "";
			try {
				// Get necessary settings
				var dropboxToken = Service.UserSettings.GetByUserKey(BaseUserSettingService.SettingKeys.DropboxToken, GetCurrentUserGuid());
				if (dropboxToken == null) {
					throw new Exception("Dropbox is not connected. Cannot update spreadsheet. Check your dropbox settings anad make sure you are connected.");
				}
				var dbPath = Service.UserSettings.GetByUserKey(BaseUserSettingService.SettingKeys.DropboxPath, GetCurrentUserGuid());
				var dbFile = Service.UserSettings.GetByUserKey(BaseUserSettingService.SettingKeys.DropboxFile, GetCurrentUserGuid());
				if (dbPath == null || dbFile == null) {
					throw new Exception("Dropbox is not setup. Cannot update spreadsheet. Check your dropbox settings anad make sure you are connected.");
				}

				// Get the transactions
				var logins = BankDataService.Logins.GetHouseholdLoginIds(GetHouseholdIdForCurrentUser());
				var transactions = BankDataService.Transactions.GetThisMonthsTransactions(logins);

				// Get the xls file
				var options = new Options();
				options.AccessToken = dropboxToken.SettingValue;
				var client = new Client(options);

				byte[] budgetFile;
				var results = client.Core.Metadata.SearchAsync(dbPath.SettingValue, dbFile.SettingValue).Result;
				foreach (var searchResult in results) {
					budgetFile = new byte[searchResult.bytes];
					using (var stream = new MemoryStream()) {
						client.Core.Metadata.FilesAsync(searchResult.path, stream).Wait();
						budgetFile = stream.ToArray();
					}

					budgetFile = WorkingBudgetUpdater.Update(budgetFile, transactions);

					// Replace file after update
					using (var stream = new MemoryStream(budgetFile)) {
						client.Core.Metadata.FilesPutAsync(stream, Path.Combine(dbPath.SettingValue, dbFile.SettingValue)).Wait();
					}
					break;
				}

				success = true;
				message = "Spreadsheet Update was successful";
			} catch (Exception ex) {
				message = ex.Message;
			}

			return PartialView("CallbackStatus", new CallbackStatusViewModel() {
				Success = success,
				Message = message
			});
		}

		//private void RunNormalUpdate(ref dynamic data) {
		//	var options = new Options();
		//	options.AccessToken = TokenSaver.GetToken("dropboxtoken");
		//	var client = new Client(options);

		//	byte[] budgetFile;
		//	var results = client.Core.Metadata.SearchAsync("/Documents/Money", "Working Budget.xlsm").Result;
		//	foreach (var searchResult in results) {
		//		budgetFile = new byte[searchResult.bytes];
		//		using (var stream = new MemoryStream()) {
		//			client.Core.Metadata.FilesAsync(searchResult.path, stream).Wait();
		//			budgetFile = stream.ToArray();
		//			// Save file as backup
		//			//BudgetBackup.BackupBudgetFile(budgetFile);
		//		}

		//		var updater = new WorkingBudgetUpdater();
		//		budgetFile = updater.Update(budgetFile);

		//		// Replace file after update
		//		using (var stream = new MemoryStream(budgetFile)) {
		//			client.Core.Metadata.FilesPutAsync(stream, "/Documents/Money/Working Budget.xlsm").Wait();
		//		}
		//		data.Message = "Successful Update";
		//	}
		//}
	}
}
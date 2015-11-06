using System;
using System.Linq;
using CHABS.API.Objects;

namespace CHABS.API.Services {
	public class UserRoleService {
		public BaseUserService Users;
		public BaseService<Role> Roles;
		public BaseService<UserLogin> UserLogins;
		public BaseUserSettingService UserSettings;
		public BaseService<UserClaim> UserClaims;
		private Session Session { get; set; }

		public UserRoleService(Session session) {
			Session = session;
			Users = new BaseUserService(Session);
			Roles = new BaseService<Role>(Session);
			UserLogins = new BaseService<UserLogin>(Session);
			UserSettings = new BaseUserSettingService(Session);
			UserClaims = new BaseService<UserClaim>(Session);
		}
	}

	public class BaseUserService : BaseService<User> {
		public BaseUserService(Session session)
			: base(session) {
		}

		/// <summary>
		/// Get a user by email or username. Will return null if there is no user.
		/// </summary>
		/// <param name="email"></param>
		/// <returns></returns>
		public User GetByEmail(string email) {
			var user = db.GetList<User>(new { Email = email }).FirstOrDefault();
			return user;
		}

		public User GetById(Guid id) {
			return base.GetById(id);
		}

		protected override void AfterSave(bool isInserting, DataObject daObj) {
			// Create a new household for this user and map it
			if (isInserting) {
				var service = new HouseholdService(Session);
				var household = new Household() {
					Name = (daObj as User).Email
				};
				service.Households.Upsert(household);
				service.HouseholdMaps.AddUserToHousehold(daObj.Id, household.Id);
			}

			base.AfterSave(isInserting, daObj);
		}
	}

	public class BaseUserSettingService : BaseService<UserSetting> {
		public enum SettingKeys {
			DropboxToken,
			DropboxPath,
			DropboxFile
		}

		public BaseUserSettingService(Session session)
			: base(session) {
		}
		
		/// <summary>
		/// Get a user by email or username.
		/// </summary>
		/// <param name="email"></param>
		/// <returns></returns>
		public UserSetting SaveSetting(SettingKeys key, string value, Guid userId) {
			return SaveSetting(key, value, userId);
		}

		public UserSetting SaveSetting(string key, string value, Guid userId) {
			var setting = new UserSetting() {
				UserId = userId,
				SettingKey = key,
				SettingValue = value
			};
			var dbSetting = GetByUserKey(setting);
			if (dbSetting != null) {
				setting.IsNew = false;
			}
			Upsert(setting);
			return setting;
		}

		public UserSetting GetByUserKey(UserSetting setting) {
			var user = db.GetList<UserSetting>(new { setting.UserId, setting.SettingKey }).FirstOrDefault();
			return user;
		}

		public UserSetting GetByUserKey(SettingKeys key, Guid userId) {
			return GetByUserKey(key.ToString(), userId);
		}

		public UserSetting GetByUserKey(string key, Guid userId) {
			var user = db.GetList<UserSetting>(new { UserId = userId, SettingKey = key }).FirstOrDefault();
			return user;
		}
	}

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CHABS.API.Objects;
using CHABS.API.Services;
using CRWestropp.Utilities.Extensions;
using Microsoft.AspNet.Identity;

namespace CHABS.Models {
	public class AppUserStore : IUserStore<AppUser>, IUserPasswordStore<AppUser>, IUserEmailStore<AppUser, string>,
		IUserLoginStore<AppUser>, IUserClaimStore<AppUser>, IUserRoleStore<AppUser>, IUserSecurityStampStore<AppUser>, 
		IUserLockoutStore<AppUser, string>, IUserTwoFactorStore<AppUser, string> {
		private UserRoleService Service;
		public AppUserStore(UserRoleService service) {
			Service = service;
		}

		#region UserStore
		public Task CreateAsync(AppUser user) {
			user.IsNew = true;
			Service.Users.Upsert(user.MapToUser());
			return Task.FromResult(0);
		}

		public Task UpdateAsync(AppUser user) {
			user.IsNew = false;
			Service.Users.Upsert(user.MapToUser());
			return Task.FromResult(0);
		}

		public Task DeleteAsync(AppUser user) {
			Service.Users.Delete(user.MapToUser());
			return Task.FromResult(0);
		}

		public Task<AppUser> FindByIdAsync(string userId) {
			var dbUser = Service.Users.GetById(userId.ToGuid());
			return Task.FromResult(AppUser.MapFromUser(dbUser));
		}

		public Task<AppUser> FindByIdAsync(Guid userId) {
			var dbUser = Service.Users.GetById(userId);
			return Task.FromResult(AppUser.MapFromUser(dbUser));
		}

		public Task<AppUser> FindByNameAsync(string userName) {
			var dbUser = Service.Users.GetByEmail(userName);
			return Task.FromResult(dbUser == null ? null : AppUser.MapFromUser(dbUser));
		}
		#endregion

		#region PasswordStore
		public Task SetPasswordHashAsync(AppUser user, string passwordHash) {
			user.PasswordHash = passwordHash;
			return Task.FromResult(0);
		}

		public Task<string> GetPasswordHashAsync(AppUser user) {
			if (user.PasswordHash.IsNull() && !user.Id.IsNull()) {
				var retreivedUser = Service.Users.GetById(user.Id.ToGuid());
				return Task.FromResult(retreivedUser.PasswordHash);
			} else {
				return Task.FromResult(user.PasswordHash);
			}
		}

		public Task<bool> HasPasswordAsync(AppUser user) {
			throw new NotImplementedException();
		}
		#endregion

		#region EmailStore
		public Task SetEmailAsync(AppUser user, string email) {
			throw new NotImplementedException();
		}

		public Task<string> GetEmailAsync(AppUser user) {
			if (user.Email.IsNull() && !user.Id.IsNull()) {
				var retreivedUser = Service.Users.GetById(user.Id.ToGuid());
				return Task.FromResult(retreivedUser.Email);
			}
			return Task.FromResult(user.Email.IsNull() ? "" : user.Email);
		}

		public Task<bool> GetEmailConfirmedAsync(AppUser user) {
			return Task.FromResult(user.EmailConfirmed);
		}

		public Task SetEmailConfirmedAsync(AppUser user, bool confirmed) {
			user.EmailConfirmed = confirmed;
			return Task.FromResult(0);
		}

		public Task<AppUser> FindByEmailAsync(string email) {
			var dbUser = Service.Users.GetByEmail(email);
			var appUser = dbUser == null ? null : AppUser.MapFromUser(dbUser);
			return Task.FromResult(appUser);
		}
		#endregion

		public void Dispose() {
			
		}

		#region Logins
		public Task AddLoginAsync(AppUser user, UserLoginInfo login) {
			if (!user.Logins.Any(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey)) {
				user.Logins.Add(login);
				Service.UserLogins.Upsert(new UserLogin() {
					LoginProvider = login.LoginProvider,
					ProviderKey = login.ProviderKey,
					UserId = user.Id.ToGuid()
				});
			}

			return Task.FromResult(true);
		}

		public Task RemoveLoginAsync(AppUser user, UserLoginInfo login) {
			// TODO: implement
			user.Logins.RemoveAll(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey);

			return Task.FromResult(0);
		}

		public Task<IList<UserLoginInfo>> GetLoginsAsync(AppUser user) {
			var logins = Service.UserLogins.GetList(new {UserId = user.Id});
			var loginInfos = logins.Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey)).ToList();
			user.SetLogins(loginInfos);
			return Task.FromResult((IList<UserLoginInfo>)user.Logins);
		}

		public Task<AppUser> FindAsync(UserLoginInfo login) {
			var loginKeys = Service.UserLogins.GetSingle(new {login.LoginProvider, login.ProviderKey});
			User user = null;
			if (loginKeys != null) {
				user = Service.Users.GetById(loginKeys.UserId);
			}

			return Task.FromResult(AppUser.MapFromUser(user));
		}
		#endregion

		#region Claims
		public Task<IList<Claim>> GetClaimsAsync(AppUser user) {
			// TODO: Implement
			IList<Claim> result = user.Claims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList();
			return Task.FromResult(result);
		}

		public Task AddClaimAsync(AppUser user, Claim claim) {
			if (!user.Claims.Any(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value)) {
				var identityUserClaim = new IdentityUserClaim {
					ClaimType = claim.Type,
					ClaimValue = claim.Value,
					UserId = user.Id
				};
				user.Claims.Add(identityUserClaim);
				Service.UserClaims.Upsert(identityUserClaim.MapToUserClaim());
			}

			return Task.FromResult(0);
		}

		public Task RemoveClaimAsync(AppUser user, Claim claim) {
			// TODO: Implement
			user.Claims.RemoveAll(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);
			return Task.FromResult(0);
		}
		#endregion

		#region Roles
		public Task AddToRoleAsync(AppUser user, string roleName) {
			// TODO: Implement
			if (!user.Roles.Contains(roleName, StringComparer.InvariantCultureIgnoreCase))
				user.Roles.Add(roleName);

			return Task.FromResult(true);
		}

		public Task RemoveFromRoleAsync(AppUser user, string roleName) {
			// TODO: Implement
			user.Roles.RemoveAll(r => String.Equals(r, roleName, StringComparison.InvariantCultureIgnoreCase));

			return Task.FromResult(0);
		}

		public Task<IList<string>> GetRolesAsync(AppUser user) {
			var func = new Func<IList<string>>(() => {
				if (user.Roles.Count == 0 && !user.Id.IsNull()) {
					return new List<string>();
				} else {
					return user.Roles;
				}
			});
			return Task.Factory.StartNew(func);
		}

		public Task<bool> IsInRoleAsync(AppUser user, string roleName) {
			// TODO: Implement
			return Task.FromResult(user.Roles.Contains(roleName, StringComparer.InvariantCultureIgnoreCase));
		}
		#endregion

		#region Security Stamp
		public Task SetSecurityStampAsync(AppUser user, string stamp) {
			user.SecurityStamp = stamp;

			return Task.FromResult(0);
		}

		public Task<string> GetSecurityStampAsync(AppUser user) {
			string stamp;
			if (user.SecurityStamp.IsNull() && !user.Id.IsNull()) {
				var retreivedUser = Service.Users.GetById(user.Id.ToGuid());
				stamp = retreivedUser.SecurityStamp;
			} else {
				stamp = user.SecurityStamp;
			}
			return Task.FromResult(stamp);
		}
		#endregion

		#region Lockout
		public Task<DateTimeOffset> GetLockoutEndDateAsync(AppUser user) {
			// TODO: Implement
			return Task.FromResult(new DateTimeOffset());
		}

		public Task SetLockoutEndDateAsync(AppUser user, DateTimeOffset lockoutEnd) {
			// TODO: Implement
			return Task.FromResult(0);
		}

		public Task<int> IncrementAccessFailedCountAsync(AppUser user) {
			// TODO: Implement
			return Task.FromResult(0);
		}

		public Task ResetAccessFailedCountAsync(AppUser user) {
			// TODO: Implement
			return Task.FromResult(0);
		}

		public Task<int> GetAccessFailedCountAsync(AppUser user) {
			// TODO: Implement
			return Task.FromResult(0);
		}

		public Task<bool> GetLockoutEnabledAsync(AppUser user) {
			// TODO: Implement
			return Task.FromResult(false);
		}

		public Task SetLockoutEnabledAsync(AppUser user, bool enabled) {
			// TODO: Implement
			return Task.FromResult(0);
		}
		#endregion

		#region TwoFactor
		public Task SetTwoFactorEnabledAsync(AppUser user, bool enabled) {
			// TODO: Implement
			return Task.FromResult(0);
		}

		public Task<bool> GetTwoFactorEnabledAsync(AppUser user) {
			// TODO: Implement
			return Task.FromResult(false);
		}
		#endregion
	}
}
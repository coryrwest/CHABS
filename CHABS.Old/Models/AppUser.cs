using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using CHABS.API.Objects;
using CRWestropp.Utilities.Extensions;
using Microsoft.AspNet.Identity;

namespace CHABS.Models {
	/// <summary>
	/// This is an ASP Identity specific middleware to handle differences
	/// between how ASP Identity handles objects and how the DAL handles them.
	/// </summary>
	public class AppUser: IUser {
		public string Id { get; set; }
		public string UserName { get; set; }
		public string Email { get { return UserName; } set { UserName = value; } }
		public string PasswordHash { get; set; }
		public string SecurityStamp { get; set; }
		public bool EmailConfirmed { get; set; }
		public bool IsNew { get; set; }
		public List<string> Roles { get; private set; }
		public List<IdentityUserClaim> Claims { get; private set; }
		public List<UserLoginInfo> Logins { get; private set; }

		public AppUser() {
			IsNew = true;
			Id = Guid.NewGuid().ToString();
			this.Claims = new List<IdentityUserClaim>();
			this.Roles = new List<string>();
			this.Logins = new List<UserLoginInfo>();
		}

		public AppUser(string userName)
			: this() {
			this.UserName = userName;
		}

		public Task<ClaimsIdentity> GenerateUserIdentityAsync(ApplicationUserManager userManager) {
			return userManager.ClaimsIdentityFactory.CreateAsync(userManager, this, DefaultAuthenticationTypes.ApplicationCookie);
		}

		/// <summary>
		/// Map an AppUser to the DataObject User for saving.
		/// </summary>
		/// <returns></returns>
		public User MapToUser() {
			var user = new User();
			if (!Id.IsNull()) {
				user.Id = Id.ToGuid();
			}
			user.Email = UserName;
			user.PasswordHash = PasswordHash;
			user.SecurityStamp = SecurityStamp;
			user.IsNew = IsNew;
			user.EmailConfirmed = EmailConfirmed;
			return user;
		}

		/// <summary>
		/// Map an AppUser from the DataObject User.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public static AppUser MapFromUser(User user) {
			if (user != null) {
				var appUser = new AppUser();
				appUser.Id = user.Id.ToString();
				appUser.UserName = user.Email;
				appUser.Email = user.Email;
				appUser.PasswordHash = user.PasswordHash;
				appUser.SecurityStamp = user.SecurityStamp;
				appUser.IsNew = user.IsNew;
				appUser.EmailConfirmed = user.EmailConfirmed;
				return appUser;
			} else {
				return null;
			}
		}

		public void SetLogins(List<UserLoginInfo> logins) {
			Logins = logins;
		}
	}

	public class IdentityUserClaim {
		public virtual string Id { get; set; }
		public virtual string UserId { get; set; }
		public virtual string ClaimType { get; set; }
		public virtual string ClaimValue { get; set; }

		/// <summary>
		/// Map an IdentityUserClaim to the DataObject UserClaim for saving.
		/// </summary>
		/// <returns></returns>
		public UserClaim MapToUserClaim() {
			var userClaim = new UserClaim();
			userClaim.Id = Id.IsNull() ? Guid.NewGuid() : Id.ToGuid();
			userClaim.UserId = UserId.ToGuid();
			userClaim.ClaimType = ClaimType;
			userClaim.ClaimValue = ClaimValue;
			return userClaim;
		}

		/// <summary>
		/// Map an IdentityUserClaim from the DataObject UserClaim.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public static IdentityUserClaim MapFromUserClaim(UserClaim userClaim) {
			if (userClaim != null) {
				var claim = new IdentityUserClaim();
				claim.Id = userClaim.Id.ToString();
				claim.UserId = userClaim.UserId.ToString();
				claim.ClaimType = userClaim.ClaimType;
				claim.ClaimValue = userClaim.ClaimValue;
				return claim;
			} else {
				return null;
			}
		}
	}
}
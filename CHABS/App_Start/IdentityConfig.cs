using System;
using System.Security.Claims;
using System.Security.Policy;
using System.Threading.Tasks;
using CHABS.API.Objects;
using CHABS.API.Services;
using CHABS.Models;
using CRWestropp.Utilities.Extensions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace CHABS {
	public class EmailService : IIdentityMessageService {
		public Task SendAsync(IdentityMessage message) {
			// Plug in your email service here to send an email.
			return Task.FromResult(0);
		}
	}

	// Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
	public class ApplicationUserManager : UserManager<AppUser> {
		public ApplicationUserManager(IUserStore<AppUser> store)
			: base(store) {
		}

		public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) {
			var manager = new ApplicationUserManager(new AppUserStore(new UserRoleService(new Session())));
			// Configure validation logic for usernames
			manager.UserValidator = new UserValidator<AppUser>(manager) {
				AllowOnlyAlphanumericUserNames = false,
				RequireUniqueEmail = true
			};
			// Configure validation logic for passwords
			manager.PasswordValidator = new PasswordValidator {
				RequiredLength = 6,
				RequireNonLetterOrDigit = false,
				RequireDigit = true,
				RequireLowercase = true,
				RequireUppercase = false
			};

			// Configure user lockout defaults
			manager.UserLockoutEnabledByDefault = true;
			manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
			manager.MaxFailedAccessAttemptsBeforeLockout = 5;

			// Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
			// You can write your own provider and plug it in here.
			//manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<AppUser> {
			//	Subject = "Security Code",
			//	BodyFormat = "Your security code is {0}"
			//});
			manager.EmailService = new EmailService();
			var dataProtectionProvider = options.DataProtectionProvider;
			if (dataProtectionProvider != null) {
				manager.UserTokenProvider =
					new DataProtectorTokenProvider<AppUser>(dataProtectionProvider.Create("ASP.NET Identity"));
			}
			return manager;
		}
	}

	// Configure the application sign-in manager which is used in this application.
	public class ApplicationSignInManager : SignInManager<AppUser, string> {
		public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
			: base(userManager, authenticationManager) {
		}

		public override Task<ClaimsIdentity> CreateUserIdentityAsync(AppUser user) {
			return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
		}

		public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context) {
			return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
		}

		public override Task SignInAsync(AppUser user, bool isPersistent, bool rememberBrowser) {
			// Set the session
			var AppSession = new Session();
			AppSession.BuildSession(user.Id.ToGuid());
			System.Web.HttpContext.Current.Session["Session"] = AppSession;

			return base.SignInAsync(user, isPersistent, rememberBrowser);
		}
	}
}

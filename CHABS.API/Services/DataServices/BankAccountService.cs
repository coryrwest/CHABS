using System;
using System.Collections.Generic;
using System.Linq;
using CHABS.API.Objects;
using CHABS.API.Services.DataServices;

namespace CHABS.API.Services {
	public class BankAccountService : BaseService<BankAccount> {
		public BankAccountService(Session session) : base(session) {
		}

		public List<string> GetVisibleAccountServiceIds(Guid loginId) {
			var accounts = GetList("loginid = @loginid and shown = true", new { loginid = loginId });
			return accounts.Select(a => a.ServiceId).ToList();
		}

		public List<BankAccount> GetListByLogin(Guid loginId) {
			var accounts = GetList("loginid = @loginid", new { loginid = loginId });
			return accounts.OrderBy(a => a.Name).ToList();
		}
	}
}
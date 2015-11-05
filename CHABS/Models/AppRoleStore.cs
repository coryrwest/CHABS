using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using CHABS.API.Objects;
using Microsoft.AspNet.Identity;

namespace CHABS.Models {
	public class AppRoleStore : IRoleStore<Role, Guid> {
		public void Dispose() {
			throw new NotImplementedException();
		}

		public Task CreateAsync(Role role) {
			throw new NotImplementedException();
		}

		public Task UpdateAsync(Role role) {
			throw new NotImplementedException();
		}

		public Task DeleteAsync(Role role) {
			throw new NotImplementedException();
		}

		public Task<Role> FindByIdAsync(Guid roleId) {
			throw new NotImplementedException();
		}

		public Task<Role> FindByNameAsync(string roleName) {
			throw new NotImplementedException();
		}
	}
}
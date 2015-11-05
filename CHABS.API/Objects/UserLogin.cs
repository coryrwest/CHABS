using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNet.Identity;

namespace CHABS.API.Objects {
	[Table("user_logins")]
	public class UserLogin : DataObject {
		public Guid UserId { get; set; }
		public string LoginProvider { get; set; }
		public string ProviderKey { get; set; }
	}
}

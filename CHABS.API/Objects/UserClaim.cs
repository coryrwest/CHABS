using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNet.Identity;

namespace CHABS.API.Objects {
	[Table("user_claims")]
	public class UserClaim : DataObject {
		public Guid UserId { get; set; }
		public string ClaimType { get; set; }
		public string ClaimValue { get; set; }
	}
}

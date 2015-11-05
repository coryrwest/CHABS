using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace CHABS.API.Objects {
	[Table("users")]
	public class User : DataObject {
		public string PasswordHash { get; set; }
		public string SecurityStamp { get; set; }
		public string Email { get; set; }
		public bool EmailConfirmed { get; set; }
		public int AccessFailedCount { get; set; }
		public bool LockoutEnabled { get; set; }
		public DateTime LockoutEndDateUtc { get; set; }
	}
}

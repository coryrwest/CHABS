using System;
using Dapper;

namespace CHABS.API.Objects {
	[Table("invited_users")]
	public class InvitedUser : DataObject {
		public Guid HouseholdId { get; set; }
		public string Email { get; set; }
		public string Token { get; set; }
	}
}

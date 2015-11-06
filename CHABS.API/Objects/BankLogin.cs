using System;
using Dapper;

namespace CHABS.API.Objects {
	[Table("logins")]
	public class BankLogin : DataObject {
		public Guid HouseholdId { get; set; }
		public string Name { get; set; }
		public string Institution { get; set; }
		public string AccessToken { get; set; }
	}
}
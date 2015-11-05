using System;
using Dapper;

namespace CHABS.API.Objects {
	[Table("accounts")]
	public class BankLoginAccount : DataObject {
		public Guid LoginId { get; set; }
		public string Name { get; set; }
		public string ServiceId { get; set; }
		public bool Shown { get; set; }
	}
}
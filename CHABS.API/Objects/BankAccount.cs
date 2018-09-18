using System;
using Dapper;

namespace CHABS.API.Objects {
	[Table("accounts")]
	public class BankAccount : DataObject {
		public Guid LoginId { get; set; }
		public string Name { get; set; }
	    public string DisplayName { get; set; }
		public string ServiceId { get; set; }
		public bool Shown { get; set; }
		public string Mask { get; set; }
		public string Type { get; set; }
		public string SubType { get; set; }
	}
}
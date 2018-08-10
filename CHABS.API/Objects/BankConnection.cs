using System;
using Dapper;

namespace CHABS.API.Objects {
	[Table("logins")]
	public class BankConnection : DataObject {
		public Guid HouseholdId { get; set; }
		public string Name { get; set; }
		public string Institution { get; set; }
		public string AccessToken { get; set; }

        [Editable(false)]
        public string PublicToken { get; set; }


	    public override bool Deleted { get; set; }
	    [Editable(false)]
	    public override bool Perpetual => true;
	}
}
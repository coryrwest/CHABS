using System;
using Dapper;

namespace CHABS.API.Objects {
	[Table("budgets")]
	public class Budget : DataObject {
		public string Name { get; set; }
		public decimal Amount { get; set; }
		public DateTime Created { get; set; }
		public DateTime Disabled { get; set; }
		public Guid HouseholdId { get; set; }

		public Budget() {
			Created = DateTime.Now;
		}

		#region Make Perpetual
		[Editable(false)]
		public override bool Perpetual {
			get { return true; }
		}
		public override bool Deleted { get; set; }
		#endregion
	}
}

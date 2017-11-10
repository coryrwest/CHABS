using System;
using Dapper;

namespace CHABS.API.Objects {
	[Table("categories")]
	public class Category : DataObject {
		public string Name { get; set; }
		public Guid HouseholdId { get; set; }
		public bool Excluded { get; set; }
		public int Sort { get; set; }

		#region Make Perpetual
		[Editable(false)]
		public override bool Perpetual {
			get { return true; }
		}
		public override bool Deleted { get; set; }
		#endregion
	}
}

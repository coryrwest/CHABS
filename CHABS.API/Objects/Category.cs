using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNet.Identity;

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

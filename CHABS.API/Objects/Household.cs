using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace CHABS.API.Objects {
	[Table("households")]
	public class Household : DataObject {
		public string Name { get; set; }
	}


	[Table("householdusermap")]
	public class HouseholdUserMap : DataObject {
		public Guid HouseholdId { get; set; }
		public Guid UserId { get; set; }
	}
}
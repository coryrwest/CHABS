using System;
using Dapper;

namespace CHABS.API.Objects {
	[Table("category_matches")]
	public class CategoryMatch : DataObject {
		public Guid CategoryId { get; set; }
		public string Match { get; set; }
	}
}

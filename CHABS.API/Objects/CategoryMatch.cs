using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNet.Identity;

namespace CHABS.API.Objects {
	[Table("category_matches")]
	public class CategoryMatch : DataObject {
		public Guid CategoryId { get; set; }
		public string Match { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHABS.API.Objects {
	public class BudgetCategoryMap : DataObject {
		public Guid BudgetId { get; set; }
		public Guid CategoryId { get; set; }
	}
}

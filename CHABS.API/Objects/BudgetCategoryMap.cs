using System;

namespace CHABS.API.Objects {
	public class BudgetCategoryMap : DataObject {
		public Guid BudgetId { get; set; }
		public Guid CategoryId { get; set; }
	}
}

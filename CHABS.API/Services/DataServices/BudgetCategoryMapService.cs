using System;
using System.Collections.Generic;
using System.Text;
using CHABS.API.Objects;

namespace CHABS.API.Services.DataServices {

	public class BudgetCategoryMapService : BaseService<BudgetCategoryMap> {
		public BudgetCategoryMapService(Session session) : base(session) {
		}

		public void DeleteMap(Guid categoryId, Guid budgetId) {
			RawExecute("delete from budgetcategorymap where categoryId = @categoryId and budgetId = @budgetId", new { categoryId, budgetId });
		}
	}
}

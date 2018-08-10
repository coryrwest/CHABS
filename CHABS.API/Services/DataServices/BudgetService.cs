using System;
using System.Collections.Generic;
using System.Text;
using CHABS.API.Objects;

namespace CHABS.API.Services.DataServices {

	public class BudgetService : BaseService<Budget> {
		public BudgetService(Session session) : base(session) {
		}
		
		public override void DeleteObject(Budget budget) {
			budget.Disabled = DateTime.Now;
			base.DeleteObject(budget);
		}
	}
}

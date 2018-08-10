using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CHABS.API.Objects;

namespace CHABS.API.Services.DataServices
{
	public class CategoryMatchesService : BaseService<CategoryMatch> {
		public CategoryMatchesService(Session session) : base(session) {
		}

		public IEnumerable<CategoryMatch> GetListForCategory(Guid categoryId) {
			return GetList("categoryId = @categoryId", new { categoryId });
		}
		
	}
}

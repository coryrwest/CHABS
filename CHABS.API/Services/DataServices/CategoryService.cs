using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CHABS.API.Objects;

namespace CHABS.API.Services.DataServices
{
	public class CategoryService : BaseService<Category> {
		public CategoryService(Session session) : base(session) {
		}

		public IEnumerable<Category> GetAll(bool includeDeleted = false) {
			return GetList("householdid = @householdid order by sort, deleted", new { householdid = Session.HouseholdId }, includeDeleted);
		}

		public List<Category> GetAllForBudget(Guid budgetId, bool includeDeleted = false) {
			var sql = "select * from categories join budgetcategorymap map on map.categoryid = categories.id where map.budgetid = @budgetid";
			return db.Query<Category>(sql, new { budgetid = budgetId }).ToList();
		}

		public Category FindCategoryMatch(string transactionDescription) {
			string query = string.Format("select categoryid, match from category_matches join categories on categories.id = categoryid where categories.householdid = @householdid");
			var matches = db.Query(query, new { householdid = Session.HouseholdId });
			Guid categoryId = Guid.Empty;
			foreach (dynamic match in matches) {
				if (transactionDescription.ToLower().Contains(match.match.ToLower())) {
					string id = match.categoryid.ToString();
					categoryId = Guid.Parse(id);
				}
			}
			return GetById(categoryId);
		}

		protected override void BeforeInsert(DataObject daObj) {
			// Check for existing objects that may match and error appropraitely.
			var category = daObj as Category;
			if (category != null) {
				var name = category.Name;
				var existingItem = GetSingle("name = @name", new { name }, true);
				if (existingItem != null) {
					throw new Exception("A category by that name already exists. It was deleted. Please restore the deleted category instead of making a new one.");
				}
			}
			base.BeforeInsert(daObj);
		}
	}
}

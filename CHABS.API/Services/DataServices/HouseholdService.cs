using System;
using System.Collections.Generic;
using System.Linq;
using CHABS.API.Objects;
using CRWestropp.Utilities.Extensions;

namespace CHABS.API.Services {
	public class HouseholdService {
		public BaseHouseholdMapService HouseholdMaps;
		public BaseHouseholdService Households;
		private Session Session { get; set; }

		public HouseholdService(Session session) {
			Session = session;
			HouseholdMaps = new BaseHouseholdMapService(Session);
			Households = new BaseHouseholdService(Session);
		}
	}

	public class BaseHouseholdService : BaseService<Household> {
		public BaseHouseholdService(Session session)
			: base(session) {
		}

		public Household GetHouseholdForUser(Guid userId) {
			var household =
				db.Query<Household>("select households.* from households " +
									"inner join householdusermap on householdusermap.householdid = households.id " +
									"where householdusermap.userid = @UserId " +
									"limit 1;",
					new {UserId = userId});
			return household;
		}
	}

	public class BaseHouseholdMapService : BaseService<HouseholdUserMap> {
		public BaseHouseholdMapService(Session session)
			: base(session) {
		}

		/// <summary>
		/// Get all the users for a household
		/// </summary>
		/// <returns></returns>
		public List<Guid> GetUserIdsForHousehold(Guid householdId) {
			var households = db.GetList<HouseholdUserMap>(new { HouseholdId = householdId }).ToList();
			var users = households.Select(h => h.UserId).ToList();
			return users;
		}

		public List<string> GetUsernamesForHousehold(Guid householdId) {
			var userNames =
				db.Query(
					"select users.email " +
					"from users " +
					"inner join householdusermap on householdusermap.userid = users.id " +
					"where householdusermap.householdid = @HouseholdId",
					new { HouseholdId = householdId });
			var names = userNames.Select(u => u.email).Cast<string>().ToList();
			return names;
		}

		public bool IsUserInHousehold(Guid userId, Guid householdId) {
			var household =
				db.Query("select count(*) from householdusermap where userid = @UserId and householdid = @HouseholdId",
					new { UserId = userId, HouseholdId = householdId }).ToInt(0);
			return household != 0;
		}

		/// <summary>
		/// Will delete the users current household associations
		/// </summary>
		/// <param name="userid"></param>
		/// <param name="householdId"></param>
		public void AddUserToHousehold(Guid userid, Guid householdId) {
			// Delete from the map
			db.Execute(
				@"	DELETE FROM householdusermap
					where userid = @UserId",
				new { UserId = userid });
			// Add them to the new household
			db.Execute(
				@"	INSERT INTO householdusermap (userid, householdid) 
					SELECT @UserId as userid, @HouseholdId as householdid 
					from householdusermap 
					where not (userid in (select userid from householdusermap where userid = @UserId))",
				new { UserId = userid, HouseholdId = householdId });
		}

		public void RemoveUserFromHousehold(Guid userid, Guid householdId) {
			db.Execute(
				@"	delete from householdusermap
					where userid = @UserId and
					householdid = @HouseholdId",
				new { UserId = userid, HouseholdId = householdId });
		}
	}
}

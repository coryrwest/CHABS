using System;

namespace CHABS.API.Objects {
	public class Session {
		public Guid UserId { get; set; }
		public Guid HouseholdId { get; set; }
	    public String ConnectionString { get; set; }

		public void BuildSession(Guid userId, Guid householdId) {
			UserId = userId;
		    HouseholdId = householdId;
		}
	}
}

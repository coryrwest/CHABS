using System;

namespace CHABS.API.Objects {
	public class Session {
		public Guid UserId { get; set; }
		public Household Household { get; set; }

		public void BuildSession(Guid userId, Household household) {
			UserId = userId;
			Household = household;
		}
	}
}

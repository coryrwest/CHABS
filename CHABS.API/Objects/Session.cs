using System;

namespace CHABS.API.Objects {
	public class Session {
		public Guid UserId { get; set; }

		public void BuildSession(Guid userId) {
			UserId = userId;
		}
	}
}

using System;
using CHABS.API.Objects;
using CRWestropp.Utilities.Extensions;

namespace CHABS.API.Services {
	public static class PermissionsService {
		/// <summary>
		/// Check the permissions on an object against the Session. Will return false
		/// if Session UserId is empty or the user does not have permissions
		/// </summary>
		/// <param name="poco"></param>
		/// <param name="session"></param>
		/// <returns></returns>
		public static bool CheckObjectPermissions(DataObject poco, Session session) {
			if (poco.HasProperty("UserId") && poco.GetType().Name != "UserLogin") {
				// Its user specific
				if (session == null) {
					return false;
				}
				if (session.UserId == Guid.Empty) {
					return false;
				}
				if (poco.GetPropValue("UserId").ToGuid() == session.UserId) {
					return true;
				}
				return false;
			}
			// If we are not user specific then we dont care about permissions
			return true;
		}
	}
}

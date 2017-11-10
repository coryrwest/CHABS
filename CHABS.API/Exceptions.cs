using System;

namespace CHABS.API {
	public class NoResultsFoundException : Exception {
		public NoResultsFoundException(Type type, string field, string value)
			: base(string.Format("No result found for {0} with {1} of {2}.", type.Name, field, value)) {
		}

		public NoResultsFoundException(Type type, string whereClause)
			: base(string.Format("No result found for {0} with where clause: {1}.", type.Name, whereClause)) {
		}
	}

	public class PermissionsException : Exception {
		public PermissionsException()
			: base("You do not have permission to perform that action.") {
		}
	}
}

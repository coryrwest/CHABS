using System;

namespace CRWestropp.Utilities.Extensions {
	public static class GuidExtensions {
        /// <summary>
        /// Checks if a Guid is null or Guid.Empty
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
		public static bool IsNull(this Guid guid) {
			if (guid == null) {
				return true;
			}
			else if (guid == Guid.Empty) {
				return true;
			}
			else {
				return false;
			}
		}
	}
}

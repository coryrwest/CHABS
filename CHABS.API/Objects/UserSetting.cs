using System;
using Dapper;

namespace CHABS.API.Objects {
	[Table("user_settings")]
	public class UserSetting : DataObject {
		public Guid UserId { get; set; }
		public string SettingKey { get; set; }
		public string SettingValue { get; set; }
	}
}

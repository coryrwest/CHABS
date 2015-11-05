using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNet.Identity;

namespace CHABS.API.Objects {
	[Table("user_settings")]
	public class UserSetting : DataObject {
		public Guid UserId { get; set; }
		public string SettingKey { get; set; }
		public string SettingValue { get; set; }
	}
}

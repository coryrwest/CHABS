using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using CHABS.API.Objects;

namespace CHABS.Models {
	public class HouseholdViewModel {
		[Display(Name="Change this to rename your household")]
		public string CurrentHouseholdName { get; set; }

		[Display(Name="Email of user to invite to your household")]
		public string UserEmail { get; set; }

		public List<string> HouseholdUsers { get; set; }
	}
}
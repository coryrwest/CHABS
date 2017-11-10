using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CHABS.API.Objects;
using Microsoft.AspNet.Identity;

namespace CHABS.Models {
	public class AppRole : Role, IRole<Guid> {
	}
}
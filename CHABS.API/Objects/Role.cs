using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace CHABS.API.Objects {
	public class Role : DataObject, IRole<Guid> {
		public string Name { get; set; }
	}
}

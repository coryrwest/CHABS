using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHABS.API.Objects {
	public class Budget : DataObject {
		public string Name { get; set; }
		public decimal Amount { get; set; }
	}
}

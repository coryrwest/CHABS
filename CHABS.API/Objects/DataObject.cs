using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using Dapper;

namespace CHABS.API.Objects {
	public abstract class DataObject {
		[Key]
		public Guid Id { get; set; }
		[Editable(false)]
		public bool IsNew { get; set; }
		[Editable(false)]
		public virtual bool Deleted { get; set; }
		[Editable(false)]
		public virtual bool Perpetual { get { return false; } }

		protected DataObject() {
			IsNew = true;
			Id = Guid.NewGuid();
		}

		/// <summary>
		/// This will be called immedaitely after the save returns
		/// </summary>
		public virtual void AfterSave() { }

		/// <summary>
		/// This will be called immedaitely before the save happens
		/// </summary>
		public virtual void BeforeSave() { }
	}
}

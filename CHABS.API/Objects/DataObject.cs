using System;
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
		public virtual bool Perpetual => false;

		protected DataObject() {
			IsNew = true;
			Id = Guid.NewGuid();
		}
	}
}

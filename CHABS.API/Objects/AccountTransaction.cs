using System;
using Dapper;

namespace CHABS.API.Objects {
	[Table("transactions")]
	public class AccountTransaction : DataObject {
		public string Description { get; set; }
		public decimal Amount { get; set; }
		public DateTime Date { get; set; }
		public string Category { get; set; }
        /// <summary>
        /// Name of the Account that this transaction came from.
        /// </summary>
        public string Source { get; set; }

		public Guid LoginId { get; set; }
		public string ServiceId { get; set; }
		/// <summary>
		/// Corresponds to a BankLoginAccount ServiceId
		/// </summary>
		public string ServiceAccountId { get; set; }

		public bool Custom { get; set; }
		public CustomTransactionType CustomType { get; set; }

		public override bool Deleted { get; set; }
		[Editable(false)]
		public override bool Perpetual {
			get { return true; }
		}
	}

	public enum CustomTransactionType {
		Cash,
		Placeholder
	}
}
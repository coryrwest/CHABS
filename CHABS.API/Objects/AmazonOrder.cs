using System;
using System.Collections.Generic;
using System.Text;
using Dapper;

namespace CHABS.API.Objects
{
	[Table("amazonorders")]
	public class AmazonOrder : DataObject {
		public string OrderId { get; set; }
		public decimal Total { get; set; }
		public DateTime ShipmentDate { get; set; }

		public string Items { get; set; }
	}

	public class AmazonOrderItem {
		public string OrderID { get; set; }
		public string Title { get; set; }
		public string ShipmentDate { get; set; }
		public string ItemTotal { get; set; }
	}
}

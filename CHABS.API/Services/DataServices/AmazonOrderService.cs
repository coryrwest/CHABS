using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CHABS.API.Objects;

namespace CHABS.API.Services.DataServices {
	public class AmazonOrderService : BaseService<AmazonOrder> {
		public AmazonOrderService(Session session) : base(session) {
		}

		public static List<AmazonOrder> SaveAmazonOrderList(Session se, List<AmazonOrderItem> orders) {
			var grouped = orders.GroupBy(i => i.OrderID);

			var service = new AmazonOrderService(se);

			var orderList = new List<AmazonOrder>();
			foreach (var g in grouped) {
				var its = g.Select(i => i.Title);
				var totalString = g.Select(i => i.ItemTotal.Replace("$", ""));
				try {
					var total = totalString.Sum(t => Convert.ToDecimal(t));
					var order = new AmazonOrder() {
						OrderId = g.Key,
						Items = its.ToList().JoinFormat(",", ""),
						ShipmentDate = DateTime.Parse(g.First().ShipmentDate),
						Total = total
					};

					orderList.Add(order);
					var existingOrder = service.GetSingle("orderid = @orderid", new { orderid = order.OrderId }, true);
					if (existingOrder == null) {
						service.Upsert(order);
					}
				}
				catch (Exception e) {
					// Skip this transaction, swallow error I guess.
				}
			}

			return orderList;
		}

		public static List<AmazonOrderItem> ParseOrderCsv(StreamReader orderCsv) {
			var CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
			var items = new List<AmazonOrderItem>();
			using (orderCsv) {
				// Skip header
				orderCsv.ReadLine();
				while (!orderCsv.EndOfStream) {
					var line = orderCsv.ReadLine();
					var values = CSVParser.Split(line);
					items.Add(new AmazonOrderItem() {
						OrderID = values[1],
						Title = values[2],
						ShipmentDate = values[18],
						ItemTotal = values[29]
					});
				}
			}

			return items;
		}

		public static void AttachAmazonOrders(Session session, List<AmazonOrder> orders) {
			// Match Transactions
			var loginIds = new BankConnectionService(session).GetListForHousehold(session.HouseholdId);
			var transactionService = new AccountTransactionService(session);
			var transactions = transactionService.GetTransactionsByDateRange(loginIds, DateTime.Now.AddMonths(-3).FirstDay(), DateTime.Now.LastDay());

			foreach (var trans in transactions) {
				if (trans.Description.ToLower().Contains("amazon") || trans.Source.ToLower().Contains("amazon") || trans.Description.ToLower().Contains("amzn")) {
					var order = orders.FirstOrDefault(o => o.Total == Math.Abs(trans.Amount) && Math.Abs((o.ShipmentDate - trans.Date).TotalDays) < 15);
					if (order != null) {
						trans.RelatedID = order.Id;
						trans.Description = $"{trans.Description} - {order.Items.Truncate(100)}";
						transactionService.Upsert(trans);
					}
				}
			}
		}
	}
}

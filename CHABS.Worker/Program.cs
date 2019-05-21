using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CHABS.API;
using CHABS.API.Objects;
using CHABS.API.Services;
using CHABS.API.Services.DataServices;

namespace CHABS.Worker
{
    class Program
    {
        static void Main(string[] args)
        {
			var session = new Session() {
			    ConnectionString = "Host=10.10.0.125;Port=5432;Database=chabs;Username=chabs;Password=IhQlbK9Q;Pooling=true;",
			    HouseholdId = Guid.Parse("65c4f14b-1abd-435b-afe4-5f8e76881e9c"),
			    UserId = Guid.Parse("69FAD275-0942-41B8-89BB-BDE0D8C540B1")
		    };

	        var services = new DataService(session);
			var bankService = new PlaidService();

			   DoUpdate(services, bankService, new PlaidOptions() {
			    ClientSecret = "be06908ed5e2b09607cd9a193b5777",
			    ClientId = "55849d453b5cadf40371c371",
			    Env = "Development",
			    Url = "https://development.plaid.com"
			}, session);

	        //var items = AmazonOrderService.ParseOrderCsv(new StreamReader(@"orders.csv"));
	        //var orders = AmazonOrderService.SaveAmazonOrderList(session, items);
	        //AmazonOrderService.AttachAmazonOrders(session, orders);

        }

	    public static void DoUpdate(DataService services, PlaidService BankService, PlaidOptions plaidOptions, Session session) {
		    var loginIds = services.BankConnections.GetListForHousehold(session.HouseholdId);
		    var options = new TransactionUpdateService.ExecutionContext(loginIds, session.UserId, services, BankService);

		    Console.WriteLine("Starting Update");

		    TransactionUpdateService.DoTransactionUpdate(plaidOptions, options, new DateTime(2016, 6, 1), new DateTime(2016, 12, 29));
		    //for (int i = -4; i <= 0; i++) {
			   // Console.WriteLine("Update " + i);
			   // TransactionUpdateService.DoTransactionUpdate(plaidOptions, options, DateTime.Now.AddMonths(i).FirstDay(), DateTime.Now.AddMonths(i).LastDay());
		    //}
	    }
	}

}

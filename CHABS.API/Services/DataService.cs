using System;
using System.Collections.Generic;
using System.Text;
using CHABS.API.Objects;
using CHABS.API.Services.DataServices;

namespace CHABS.API.Services
{
    public class DataService {
		public BudgetService Budgets;
	    public CategoryService Categories;
	    public CategoryMatchesService CategoryMatches;
		public BankConnectionService BankConnections;
	    public BankAccountService BankAccounts;
	    public BudgetCategoryMapService BudgetCategoryMaps;
		public AccountTransactionService AccountTransactions;

	    public DataService(Session session) {
		    Budgets = new BudgetService(session);
		    Categories = new CategoryService(session);
		    CategoryMatches = new CategoryMatchesService(session);
			BankConnections = new BankConnectionService(session);
		    BankAccounts = new BankAccountService(session);
		    BudgetCategoryMaps = new BudgetCategoryMapService(session);
			AccountTransactions = new AccountTransactionService(session);
		}
	}
}

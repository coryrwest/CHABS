﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CHABS.API.Objects;
using CHABS.API.Services.DataServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CHABS.Models {
	public class BankLoginViewModel {
		public string Name { get; set; }
		public string Institution { get; set; }
	    public SelectList InstitutionList { get; set; }
	    public string PlaidEnv { get; set; }

        public List<BankConnection> CurrentLogins { get; set; }
	}

	public class LoginListViewModel {
		public LoginListViewModel(List<BankConnection> currentLogins) {
			CurrentLogins = currentLogins;
		}

		public List<BankConnection> CurrentLogins { get; set; }
	}

	public class ConnectLoginViewModel {
		public BankConnection Connection { get; set; }
		public Guid LoginId { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
	}

	public class AccountListViewModel {
		public List<BankAccount> Accounts { get; set; }
		public string LoginName { get; set; }
	}

	public class TransactionsViewModel {
		public TransactionsViewModel(Session se, List<AccountTransaction> transactions) {
			Transations = new List<TransactionWithRelated>();

			foreach (var accountTransaction in transactions) {
				if (accountTransaction.RelatedID != Guid.Empty) {
					// Find the related item and get the id
					var service = new AmazonOrderService(se);
					var related = service.GetById(accountTransaction.RelatedID);
					Transations.Add(new TransactionWithRelated() {
						Transaction = accountTransaction,
						Related = related?.OrderId
					});
				}
				else {
					Transations.Add(new TransactionWithRelated() {
						Transaction = accountTransaction,
						Related = ""
					});
				}
			}
		}

		[DataType(DataType.Date)]
		public DateTime StartDate { get; set; }
		[DataType(DataType.Date)]
		public DateTime EndDate { get; set; }

		public int MonthsDifference { get; set; }

		/// <summary>
		/// String for range of transactions
		/// </summary>
		public string RangeString { get; set; }

		public List<Guid> Logins { get; set; }
		public List<TransactionWithRelated> Transations { get; set;  }

		public class TransactionWithRelated {
			public AccountTransaction Transaction { get; set; }
			public string Related { get; set; }
		}
	}

	public class CategoriesViewModel {
		public string Name { get; set; }
		public bool Excluded { get; set; }

		public List<Category> CurrentCategories { get; set; }
	}

	public class CategoriesListViewModel {
		public CategoriesListViewModel(List<Category> currentCategories) {
			CurrentCategories = currentCategories;
		}

		public List<Category> CurrentCategories { get; set; }
	}

	public class CategoryMatchesViewModel {
		public string Match { get; set; }
		public string CategoryName { get; set; }
		public Guid CategoryId { get; set; }

		public List<CategoryMatch> CurrentCategoryMatches { get; set; }
	}

	public class CategoryMatchesListViewModel {
		public CategoryMatchesListViewModel(List<CategoryMatch> currentCategoryMatches) {
			CurrentCategoryMatches = currentCategoryMatches.OrderBy(c => c.Match).ToList();
		}

		public List<CategoryMatch> CurrentCategoryMatches { get; set; }
	}

	public class BudgetViewModel {
		public string Name { get; set; }
		[DataType(DataType.Currency)]
		public decimal Amount { get; set; }
		public Guid CategoryId { get; set; }

		public List<Budget> CurrentBudgets { get; set; }
		public SelectList Categories { get; set; }
	}

	public class BudgetListViewModel {
		public BudgetListViewModel(List<Budget> currentBudgets) {
			CurrentBudgets = currentBudgets;
		}

		public List<Budget> CurrentBudgets { get; set; }
	}

	public class BudgetCategoryViewModel {
		public Guid BudgetId { get; set; }
		public Guid CategoryId { get; set; }
		public List<Category> CurrentBudgetCategorys { get; set; }
		public SelectList Categories { get; set; }
	}

	public class BudgetCategoryListViewModel {
		public BudgetCategoryListViewModel(List<Category> currentBudgetCategorys) {
			CurrentBudgetCategorys = currentBudgetCategorys;
		}

		public List<Category> CurrentBudgetCategorys { get; set; }
	}

	public class UploadAmazonTransactionsViewModel {
		[DataType(DataType.Upload)]
		public IFormFile Orders { get; set; }
	}
}
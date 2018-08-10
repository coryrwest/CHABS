using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CHABS.Controllers;
using Microsoft.AspNetCore.Mvc;
using CHABS.Web.Models;

namespace CHABS.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() {
	        return RedirectToAction(nameof(BankController.TransactionList), "Bank");
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

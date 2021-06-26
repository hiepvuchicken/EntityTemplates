using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Entity;
using WebTest.Reports;

namespace WebTest.Controllers
{
    public class ReportController : Controller
    {
        // GET: Report
        public ActionResult reportView(string id)
        {
            Response.Redirect("~/Reports/ReportViewer.aspx");
            return View();
        }

        

        
    }
}
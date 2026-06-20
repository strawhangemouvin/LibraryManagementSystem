using LibraryManagementSystem.Services.Impl;
using LibraryManagementSystem.Services.Interface;
using System.Web.Mvc;

namespace LibraryManagementSystem.Controllers.MVC
{
    public class ReportsMvcController : Controller
    {
        private readonly IReportService reportService;

        public ReportsMvcController()
        {
            reportService = new ReportService();
        }

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Librarian")
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            var summary = reportService.GetSummaryReport();
            return View(summary);
        }

        public ActionResult Books()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Librarian")
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            var books = reportService.GetBookReport();
            return View(books);
        }

        public ActionResult Borrowings()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Librarian")
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            var borrowings = reportService.GetBorrowingReport();
            return View(borrowings);
        }
    }
}
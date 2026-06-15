using LibraryManagementSystem.Helpers;
using LibraryManagementSystem.Services.Impl;
using LibraryManagementSystem.Services.Interface;
using System.Web.Http;

namespace LibraryManagementSystem.Controllers
{
    [TokenAuth]
    public class ReportsController : ApiController
    {
        private readonly IReportService reportService;

        public ReportsController()
        {
            reportService = new ReportService();
        }

        [HttpGet]
        [Route("api/reports/Summary")]
        public IHttpActionResult Summary()
        {
            var report = reportService.GetSummaryReport();
            return Ok(report);
        }

        [HttpGet]
        [Route("api/reports/Books")]
        public IHttpActionResult BookReport()
        {
            var books = reportService.GetBookReport();
            return Ok(books);
        }

        [HttpGet]
        [Route("api/reports/Borrowings")]
        public IHttpActionResult BorrowingReport()
        {
            var borrowings = reportService.GetBorrowingReport();
            return Ok(borrowings);
        }
    }
}
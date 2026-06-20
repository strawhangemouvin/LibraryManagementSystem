using LibraryManagementSystem.Services.Impl;
using LibraryManagementSystem.Services.Interface;
using System.Web.Mvc;

namespace LibraryManagementSystem.Controllers.MVC
{
    public class ActivityLogsMvcController : Controller
    {
        private readonly IActivityLogService activityLogService;

        public ActivityLogsMvcController()
        {
            activityLogService = new ActivityLogService();
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

            var logs = activityLogService.GetAllLogs();
            return View(logs);
        }
    }
}
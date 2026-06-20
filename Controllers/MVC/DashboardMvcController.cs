using LibraryManagementSystem.Models.ViewModel;
using LibraryManagementSystem.Services.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace LibraryManagementSystem.Controllers.MVC
{
    public class DashboardMvcController : Controller
    {
        private readonly LibraryDbContext db = new LibraryDbContext();

        public ActionResult Index()
        {
            if (Session["Role"] == null)
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            if (Session["Role"].ToString() == "Librarian")
            {
                return RedirectToAction("Librarian");
            }

            if (Session["Role"].ToString() == "Member")
            {
                return RedirectToAction("Member");
            }

            return RedirectToAction("Login", "AuthMvc");
        }

        public ActionResult Librarian()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Librarian")
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            return View();
        }

        public ActionResult Member()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Member")
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            var member = db.Members.FirstOrDefault(x => x.UserId == userId);

            var reminders = new List<BorrowingReminderViewModel>();

            if (member != null)
            {
                DateTime today = DateTime.Now.Date;
                DateTime reminderLimit = today.AddDays(3);

                var reminderData = db.Borrowings
                    .Where(x =>
                        x.MemberId == member.Id &&
                        x.Status == "Borrowed" &&
                        x.DueDate != null &&
                        x.DueDate <= reminderLimit
                    )
                    .Join(
                        db.Books,
                        borrowing => borrowing.BookId,
                        book => book.Id,
                        (borrowing, book) => new
                        {
                            borrowing,
                            book
                        }
                    )
                    .ToList();

                reminders = reminderData.Select(x =>
                {
                    int daysRemaining = (x.borrowing.DueDate.Value.Date - today).Days;

                    string reminderStatus;

                    if (daysRemaining < 0)
                    {
                        reminderStatus = "Terlambat";
                    }
                    else if (daysRemaining == 0)
                    {
                        reminderStatus = "Hari ini jatuh tempo";
                    }
                    else
                    {
                        reminderStatus = "Mendekati jatuh tempo";
                    }

                    return new BorrowingReminderViewModel
                    {
                        BorrowingId = x.borrowing.Id,
                        BookTitle = x.book.Title,
                        DueDate = x.borrowing.DueDate,
                        DaysRemaining = daysRemaining,
                        ReminderStatus = reminderStatus
                    };
                }).ToList();
            }

            ViewBag.Reminders = reminders;

            return View();
        }
    }
}

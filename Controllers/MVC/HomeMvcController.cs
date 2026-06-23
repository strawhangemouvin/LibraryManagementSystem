using LibraryManagementSystem.Models.Entity;
using LibraryManagementSystem.Services.Context;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace LibraryManagementSystem.Controllers.MVC
{
    public class HomeMvcController : Controller
    {
        private readonly LibraryDbContext db = new LibraryDbContext();

        public ActionResult Index()
        {
            List<Book> featuredBooks = new List<Book>();
            int totalBooks = 0;
            int activeMembers = 0;
            int totalBorrowings = 0;

            try
            {
                // Fetch first 3 books with their Category info
                featuredBooks = db.Books.Include(b => b.Category).Take(3).ToList();
                
                // Sum the stock of all books
                totalBooks = db.Books.Sum(b => (int?)b.Stock) ?? 0;
                
                // Count approved members
                activeMembers = db.Users.Count(u => u.Role == "Member" && u.Status == "Active");
                
                // Count total borrowings
                totalBorrowings = db.Borrowings.Count();
            }
            catch (Exception)
            {
                // Handle database initialization errors gracefully
            }

            ViewBag.TotalBooks = totalBooks;
            ViewBag.ActiveMembers = activeMembers;
            ViewBag.TotalBorrowings = totalBorrowings;

            return View(featuredBooks);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SubmitMessage(string fullName, string email, string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
                {
                    TempData["Error"] = "All fields are required.";
                    return RedirectToAction("Index");
                }

                var cleanEmail = email.Trim().ToLower();
                if (!System.Text.RegularExpressions.Regex.IsMatch(cleanEmail, @"^[^@\s]+@(gmail\.com|yahoo\.com|email\.com)$"))
                {
                    TempData["Error"] = "Email can only use @gmail.com, @yahoo.com, or @email.com domains.";
                    return RedirectToAction("Index");
                }

                string subject = "Moon Books - New Contact Message from " + fullName;
                string body = $@"
                    <h3>New Contact Message Received</h3>
                    <p><b>From:</b> {fullName} ({email})</p>
                    <p><b>Platform Identity:</b> moonbooksarea</p>
                    <p><b>Message Content:</b></p>
                    <p style='padding: 10px; background: #f9f9f9; border-left: 4px solid #C97A7E;'>{message}</p>
                ";

                LibraryManagementSystem.Helpers.EmailHelper.SendEmail("nabilamonica07@gmail.com", subject, body);

                TempData["Success"] = "Your message has been sent successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to send message: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
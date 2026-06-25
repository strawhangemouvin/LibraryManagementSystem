using LibraryManagementSystem.Models.Entity;
using LibraryManagementSystem.Services.Context;
using System;
using System.Linq;
using System.Web.Mvc;

namespace LibraryManagementSystem.Controllers.MVC
{
    public class MoonPayController : Controller
    {
        private readonly LibraryDbContext db = new LibraryDbContext();

        // GET: MoonPay/Checkout/PAY-MB-100005
        // Menampilkan halaman simulasi checkout pembayaran denda di tab baru
        [HttpGet]
        public ActionResult Checkout(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return HttpNotFound("Payment code or ID is required.");
            }

            int borrowingId;
            // Mendekode kode bayar terenkripsi (misal: PAY-MB-100005) menjadi ID peminjaman asli
            if (id.StartsWith("PAY-MB-", StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(id.Substring(7), out int parsedNum))
                {
                    return HttpNotFound("Invalid payment code format.");
                }
                borrowingId = parsedNum - 100000;
            }
            else
            {
                if (!int.TryParse(id, out borrowingId))
                {
                    return HttpNotFound("Invalid ID format.");
                }
            }
            
            var borrowing = db.Borrowings.Find(borrowingId);
            if (borrowing == null)
            {
                return HttpNotFound("Borrowing record not found.");
            }

            if (borrowing.FineAmount == null || borrowing.FineAmount <= 0)
            {
                TempData["Error"] = "This record does not have any fine.";
                return RedirectToAction("Index", "MVCBorrowings");
            }

            if (borrowing.FineStatus == "Paid")
            {
                TempData["Error"] = "Fine has already been paid.";
                return RedirectToAction("Index", "MVCBorrowings");
            }

            // Menyimpan kode bayar ke ViewBag untuk ditampilkan di halaman view
            ViewBag.PaymentCode = id.StartsWith("PAY-MB-", StringComparison.OrdinalIgnoreCase) ? id : $"PAY-MB-{100000 + borrowing.Id}";

            return View(borrowing);
        }

        // POST: MoonPay/ConfirmPayment/PAY-MB-100005
        // Memproses konfirmasi pembayaran denda secara aman dan memperbarui status di database
        [HttpPost]
        public ActionResult ConfirmPayment(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    return Json(new { success = false, message = "Payment code or ID is required." });
                }

                int borrowingId;
                // Mendekode kode bayar terenkripsi menjadi ID peminjaman
                if (id.StartsWith("PAY-MB-", StringComparison.OrdinalIgnoreCase))
                {
                    if (!int.TryParse(id.Substring(7), out int parsedNum))
                    {
                        return Json(new { success = false, message = "Invalid payment code format." });
                    }
                    borrowingId = parsedNum - 100000;
                }
                else
                {
                    if (!int.TryParse(id, out borrowingId))
                    {
                        return Json(new { success = false, message = "Invalid ID format." });
                    }
                }

                var borrowing = db.Borrowings.Find(borrowingId);
                if (borrowing == null)
                {
                    return Json(new { success = false, message = "Borrowing record not found." });
                }

                if (borrowing.FineAmount == null || borrowing.FineAmount <= 0)
                {
                    return Json(new { success = false, message = "This record does not have any fine." });
                }

                if (borrowing.FineStatus == "Paid")
                {
                    return Json(new { success = true, message = "Fine has already been paid.", alreadyPaid = true });
                }

                // Memperbarui status pembayaran denda menjadi lunas (Paid) di database
                borrowing.FineStatus = "Paid";

                // Menentukan ID pengguna yang masuk untuk pencatatan log aktivitas
                int logUserId = 1; // System fallback
                if (Session["UserId"] != null)
                {
                    logUserId = Convert.ToInt32(Session["UserId"]);
                }
                else if (borrowing.Member != null)
                {
                    logUserId = borrowing.Member.UserId;
                }

                // Create activity log
                var log = new ActivityLog
                {
                    UserId = logUserId,
                    Action = "Pay Fine",
                    Description = "Fine for borrowing ID " + borrowing.Id + " paid via Moon Pay (QRIS Simulator - Code: " + id + ")",
                    CreatedAt = DateTime.Now
                };

                db.ActivityLogs.Add(log);
                db.SaveChanges();

                return Json(new { success = true, message = "Payment successfully processed." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}

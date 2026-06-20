using LibraryManagementSystem.Models.Entity;
using LibraryManagementSystem.Services.Context;
using LibraryManagementSystem.Services.Impl;
using LibraryManagementSystem.Services.Interface;
using System;
using System.Linq;
using System.Web.Mvc;

namespace LibraryManagementSystem.Controllers.MVC
{
    public class MVCBorrowingsController : Controller
    {
        private readonly LibraryDbContext db = new LibraryDbContext();
        private readonly IBorrowingService borrowingService;

        public MVCBorrowingsController()
        {
            borrowingService = new BorrowingService();
        }

        // GET: MVCBorrowings
        // Halaman pustakawan untuk melihat semua data peminjaman
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

            var borrowings = borrowingService.GetAllBorrowings();
            return View(borrowings);
        }

        // GET: MVCBorrowings/MyHistory
        // Halaman member untuk melihat riwayat peminjamannya sendiri
        public ActionResult MyHistory()
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

            if (member == null)
            {
                TempData["Error"] = "Data member tidak ditemukan.";
                return RedirectToAction("Member", "DashboardMvc");
            }

            var history = borrowingService.GetBorrowingsByMember(member.Id);
            return View(history);
        }

        // GET: MVCBorrowings/Request?bookId=1
        // Link lama tetap aman, diarahkan ke halaman konfirmasi
        public ActionResult Request(int bookId)
        {
            return RedirectToAction("RequestConfirm", new { bookId = bookId });
        }

        // GET: MVCBorrowings/RequestConfirm?bookId=1
        // Member melihat detail buku sebelum mengajukan peminjaman
        public ActionResult RequestConfirm(int bookId)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Member")
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            var book = db.Books.Find(bookId);

            if (book == null)
            {
                TempData["Error"] = "Buku tidak ditemukan.";
                return RedirectToAction("Index", "MVCBooks");
            }

            if (book.AvailableStock <= 0)
            {
                TempData["Error"] = "Stok buku tidak tersedia.";
                return RedirectToAction("Index", "MVCBooks");
            }

            int userId = Convert.ToInt32(Session["UserId"]);

            var member = db.Members.FirstOrDefault(x => x.UserId == userId);

            if (member == null)
            {
                TempData["Error"] = "Data member tidak ditemukan.";
                return RedirectToAction("Member", "DashboardMvc");
            }

            ViewBag.Member = member;
            ViewBag.UserFullName = Session["FullName"];

            return View(book);
        }

        // POST: MVCBorrowings/RequestConfirmed
        // Setelah member klik Ajukan Peminjaman
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RequestConfirmed(int bookId)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Member")
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            try
            {
                int userId = Convert.ToInt32(Session["UserId"]);

                var member = db.Members.FirstOrDefault(x => x.UserId == userId);

                if (member == null)
                {
                    TempData["Error"] = "Data member tidak ditemukan.";
                    return RedirectToAction("Member", "DashboardMvc");
                }

                var borrowing = new Borrowing
                {
                    BookId = bookId,
                    MemberId = member.Id,
                    Notes = "Pengajuan peminjaman dari member"
                };

                borrowingService.RequestBorrowing(borrowing);

                TempData["Success"] = "Pengajuan peminjaman berhasil dikirim. Silakan menunggu persetujuan pustakawan.";
                return RedirectToAction("MyHistory");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "MVCBooks");
            }
        }

        // GET: MVCBorrowings/Approve/5
        // Pustakawan approve peminjaman
        public ActionResult Approve(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Librarian")
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            try
            {
                int approvedBy = Convert.ToInt32(Session["UserId"]);
                borrowingService.ApproveBorrowing(id, approvedBy);

                TempData["Success"] = "Peminjaman berhasil disetujui.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        // GET: MVCBorrowings/Reject/5
        // Pustakawan reject peminjaman
        public ActionResult Reject(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Librarian")
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            try
            {
                int rejectedBy = Convert.ToInt32(Session["UserId"]);
                borrowingService.RejectBorrowing(id, rejectedBy);

                TempData["Success"] = "Peminjaman berhasil ditolak.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        // GET: MVCBorrowings/Return/5
        // Link lama tetap aman, diarahkan ke halaman konfirmasi pengembalian
        public ActionResult Return(int id)
        {
            return RedirectToAction("ReturnConfirm", new { id = id });
        }

        // GET: MVCBorrowings/ReturnConfirm/5
        // Pustakawan melihat detail sebelum memproses pengembalian
        public ActionResult ReturnConfirm(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Librarian")
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            var borrowing = db.Borrowings.Find(id);

            if (borrowing == null)
            {
                TempData["Error"] = "Data peminjaman tidak ditemukan.";
                return RedirectToAction("Index");
            }

            if (borrowing.Status != "Borrowed")
            {
                TempData["Error"] = "Hanya peminjaman dengan status Borrowed yang dapat dikembalikan.";
                return RedirectToAction("Index");
            }

            int lateDays = 0;

            if (borrowing.DueDate != null && DateTime.Now.Date > borrowing.DueDate.Value.Date)
            {
                lateDays = (DateTime.Now.Date - borrowing.DueDate.Value.Date).Days;
            }

            ViewBag.LateDays = lateDays;

            return View(borrowing);
        }

        // POST: MVCBorrowings/ReturnConfirmed
        // Setelah pustakawan klik Proses Pengembalian
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReturnConfirmed(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Librarian")
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            try
            {
                int returnedBy = Convert.ToInt32(Session["UserId"]);

                borrowingService.ReturnBook(id, returnedBy);

                TempData["Success"] = "Buku berhasil dikembalikan.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }
        // GET: MVCBorrowings/PayFineConfirm/5
        // Pustakawan melihat QRIS simulasi untuk pembayaran denda
        public ActionResult PayFineConfirm(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Librarian")
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            var borrowing = db.Borrowings.Find(id);

            if (borrowing == null)
            {
                TempData["Error"] = "Data peminjaman tidak ditemukan.";
                return RedirectToAction("Index");
            }

            if (borrowing.FineAmount == null || borrowing.FineAmount <= 0)
            {
                TempData["Error"] = "Peminjaman ini tidak memiliki denda.";
                return RedirectToAction("Index");
            }

            if (borrowing.FineStatus == "Paid")
            {
                TempData["Error"] = "Denda sudah dibayar.";
                return RedirectToAction("Index");
            }

            return View(borrowing);
        }

        // POST: MVCBorrowings/PayFineConfirmed
        // Pustakawan menandai denda sudah dibayar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PayFineConfirmed(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            if (Session["Role"] == null || Session["Role"].ToString() != "Librarian")
            {
                return RedirectToAction("Login", "AuthMvc");
            }

            try
            {
                var borrowing = db.Borrowings.Find(id);

                if (borrowing == null)
                {
                    TempData["Error"] = "Data peminjaman tidak ditemukan.";
                    return RedirectToAction("Index");
                }

                if (borrowing.FineAmount == null || borrowing.FineAmount <= 0)
                {
                    TempData["Error"] = "Peminjaman ini tidak memiliki denda.";
                    return RedirectToAction("Index");
                }

                borrowing.FineStatus = "Paid";

                var log = new ActivityLog
                {
                    UserId = Convert.ToInt32(Session["UserId"]),
                    Action = "Pay Fine",
                    Description = "Pustakawan menandai denda peminjaman ID " + borrowing.Id + " sebagai Paid",
                    CreatedAt = DateTime.Now
                };

                db.ActivityLogs.Add(log);
                db.SaveChanges();

                TempData["Success"] = "Denda berhasil ditandai sudah dibayar.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

    }
}

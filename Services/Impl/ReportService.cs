using LibraryManagementSystem.Models.ViewModel;
using LibraryManagementSystem.Services.Context;
using LibraryManagementSystem.Services.Interface;
using System.Collections.Generic;
using System.Linq;

namespace LibraryManagementSystem.Services.Impl
{
    public class ReportService : IReportService
    {
        private LibraryDbContext db = new LibraryDbContext();

        public SummaryReportViewModel GetSummaryReport()
        {
            var report = new SummaryReportViewModel
            {
                TotalBooks = db.Books.Count(),
                TotalCategories = db.Categories.Count(),
                TotalMembers = db.Members.Count(),

                ActiveMembers = db.Users.Count(x => x.Role == "Member" && x.Status == "Active"),
                PendingMembers = db.Users.Count(x => x.Role == "Member" && x.Status == "Pending"),
                RejectedMembers = db.Users.Count(x => x.Role == "Member" && x.Status == "Rejected"),

                TotalBorrowings = db.Borrowings.Count(),
                RequestedBorrowings = db.Borrowings.Count(x => x.Status == "Requested"),
                BorrowedBooks = db.Borrowings.Count(x => x.Status == "Borrowed"),
                ReturnedBooks = db.Borrowings.Count(x => x.Status == "Returned"),
                RejectedBorrowings = db.Borrowings.Count(x => x.Status == "Rejected"),

                TotalStock = db.Books.Sum(x => (int?)x.Stock) ?? 0,
                AvailableStock = db.Books.Sum(x => (int?)x.AvailableStock) ?? 0,

                TotalFines = db.Borrowings.Where(x => x.FineAmount != null).Sum(x => x.FineAmount) ?? 0,
                PaidFines = db.Borrowings.Where(x => x.FineAmount != null && x.FineStatus == "Paid").Sum(x => x.FineAmount) ?? 0,
                UnpaidFines = db.Borrowings.Where(x => x.FineAmount != null && x.FineStatus == "Unpaid").Sum(x => x.FineAmount) ?? 0
            };

            return report;
        }

        public List<BookReportViewModel> GetBookReport()
        {
            var books = db.Books
                .Join(
                    db.Categories,
                    book => book.CategoryId,
                    category => category.Id,
                    (book, category) => new BookReportViewModel
                    {
                        Id = book.Id,
                        Title = book.Title,
                        Author = book.Author,
                        CategoryName = category.CategoryName,
                        Publisher = book.Publisher,
                        PublishYear = book.PublishYear,
                        Stock = book.Stock,
                        AvailableStock = book.AvailableStock
                    }
                )
                .OrderBy(x => x.Title)
                .ToList();

            return books;
        }

        public List<BorrowingReportViewModel> GetBorrowingReport()
        {
            var borrowings = db.Borrowings
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
                .Join(
                    db.Members,
                    data => data.borrowing.MemberId,
                    member => member.Id,
                    (data, member) => new
                    {
                        data.borrowing,
                        data.book,
                        member
                    }
                )
                .Join(
                    db.Users,
                    data => data.member.UserId,
                    user => user.Id,
                    (data, user) => new BorrowingReportViewModel
                    {
                        BorrowingId = data.borrowing.Id,
                        MemberName = user.FullName,
                        BookTitle = data.book.Title,
                        RequestDate = data.borrowing.RequestDate,
                        BorrowDate = data.borrowing.BorrowDate,
                        DueDate = data.borrowing.DueDate,
                        ReturnDate = data.borrowing.ReturnDate,
                        Status = data.borrowing.Status,
                        Notes = data.borrowing.Notes
                    }
                )
                .OrderByDescending(x => x.RequestDate)
                .ToList();

            return borrowings;
        }

        public List<FineReportViewModel> GetFineReport()
        {
            var fines = db.Borrowings
                .Where(x => x.FineAmount != null && x.FineAmount > 0)
                .Join(
                    db.Books,
                    borrowing => borrowing.BookId,
                    book => book.Id,
                    (borrowing, book) => new { borrowing, book }
                )
                .Join(
                    db.Members,
                    data => data.borrowing.MemberId,
                    member => member.Id,
                    (data, member) => new { data.borrowing, data.book, member }
                )
                .Join(
                    db.Users,
                    data => data.member.UserId,
                    user => user.Id,
                    (data, user) => new FineReportViewModel
                    {
                        BorrowingId = data.borrowing.Id,
                        MemberName = user.FullName,
                        Username = user.Username,
                        BookTitle = data.book.Title,
                        BorrowDate = data.borrowing.BorrowDate,
                        DueDate = data.borrowing.DueDate,
                        ReturnDate = data.borrowing.ReturnDate,
                        LateDays = data.borrowing.LateDays,
                        FineAmount = data.borrowing.FineAmount,
                        FineStatus = data.borrowing.FineStatus
                    }
                )
                .OrderByDescending(x => x.ReturnDate)
                .ToList();

            return fines;
        }
    }
}
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

        public List<BookReportViewModel> GetBookReport(System.DateTime? date = null)
        {
            var query = db.Books
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
                        AvailableStock = book.AvailableStock,
                        CreatedAt = book.CreatedAt
                    }
                );

            if (date.HasValue)
            {
                var targetDate = date.Value.Date;
                query = query.Where(x => System.Data.Entity.DbFunctions.TruncateTime(x.CreatedAt) == System.Data.Entity.DbFunctions.TruncateTime(targetDate));
            }

            return query.OrderBy(x => x.Title).ToList();
        }

        public List<BorrowingReportViewModel> GetBorrowingReport(System.DateTime? date = null)
        {
            var query = db.Borrowings
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
                );

            if (date.HasValue)
            {
                var targetDate = date.Value.Date;
                query = query.Where(x => 
                    System.Data.Entity.DbFunctions.TruncateTime(x.RequestDate) == System.Data.Entity.DbFunctions.TruncateTime(targetDate) ||
                    (x.BorrowDate != null && System.Data.Entity.DbFunctions.TruncateTime(x.BorrowDate) == System.Data.Entity.DbFunctions.TruncateTime(targetDate)) ||
                    (x.ReturnDate != null && System.Data.Entity.DbFunctions.TruncateTime(x.ReturnDate) == System.Data.Entity.DbFunctions.TruncateTime(targetDate))
                );
            }

            return query.OrderByDescending(x => x.RequestDate).ToList();
        }

        public List<FineReportViewModel> GetFineReport(System.DateTime? date = null)
        {
            var query = db.Borrowings
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
                );

            if (date.HasValue)
            {
                var targetDate = date.Value.Date;
                query = query.Where(x => x.ReturnDate != null && System.Data.Entity.DbFunctions.TruncateTime(x.ReturnDate) == System.Data.Entity.DbFunctions.TruncateTime(targetDate));
            }

            return query.OrderByDescending(x => x.ReturnDate).ToList();
        }
    }
}
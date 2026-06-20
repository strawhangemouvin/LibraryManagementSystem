using LibraryManagementSystem.Helpers;
using LibraryManagementSystem.Models.Entity;
using LibraryManagementSystem.Models.ViewModel;
using LibraryManagementSystem.Services.Context;
using LibraryManagementSystem.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibraryManagementSystem.Services.Impl
{
    public class BorrowingService : IBorrowingService
    {
        private LibraryDbContext db = new LibraryDbContext();

        public List<BorrowingViewModel> GetAllBorrowings()
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
                    (data, user) => new BorrowingViewModel
                    {
                        Id = data.borrowing.Id,
                        BookId = data.borrowing.BookId,
                        BookTitle = data.book.Title,
                        MemberId = data.borrowing.MemberId,
                        MemberName = user.FullName,
                        Username = user.Username,
                        RequestDate = data.borrowing.RequestDate,
                        BorrowDate = data.borrowing.BorrowDate,
                        DueDate = data.borrowing.DueDate,
                        ReturnDate = data.borrowing.ReturnDate,
                        Status = data.borrowing.Status,
                        Notes = data.borrowing.Notes,
                        ApprovedBy = data.borrowing.ApprovedBy,
                        ApprovedAt = data.borrowing.ApprovedAt,
                        ReturnedBy = data.borrowing.ReturnedBy,
                        ReturnedAt = data.borrowing.ReturnedAt,
                        LateDays = data.borrowing.LateDays,
                        FineAmount = data.borrowing.FineAmount,
                        FineStatus = data.borrowing.FineStatus
                    }
                )
                .OrderByDescending(x => x.RequestDate)
                .ToList();

            return borrowings;
        }

        public List<BorrowingViewModel> GetBorrowingsByMember(int memberId)
        {
            var borrowings = db.Borrowings
                .Where(x => x.MemberId == memberId)
                .Join(
                    db.Books,
                    borrowing => borrowing.BookId,
                    book => book.Id,
                    (borrowing, book) => new BorrowingViewModel
                    {
                        Id = borrowing.Id,
                        BookId = borrowing.BookId,
                        BookTitle = book.Title,
                        MemberId = borrowing.MemberId,
                        MemberName = null,
                        Username = null,
                        RequestDate = borrowing.RequestDate,
                        BorrowDate = borrowing.BorrowDate,
                        DueDate = borrowing.DueDate,
                        ReturnDate = borrowing.ReturnDate,
                        Status = borrowing.Status,
                        Notes = borrowing.Notes,
                        ApprovedBy = borrowing.ApprovedBy,
                        ApprovedAt = borrowing.ApprovedAt,
                        ReturnedBy = borrowing.ReturnedBy,
                        ReturnedAt = borrowing.ReturnedAt,
                        LateDays = borrowing.LateDays,
                        FineAmount = borrowing.FineAmount,
                        FineStatus = borrowing.FineStatus
                    }
                )
                .OrderByDescending(x => x.RequestDate)
                .ToList();

            return borrowings;
        }

        public object RequestBorrowing(Borrowing borrowing)
        {
            if (borrowing == null)
            {
                throw new Exception("Data peminjaman tidak boleh kosong");
            }

            if (borrowing.MemberId <= 0)
            {
                throw new Exception("MemberId wajib diisi");
            }

            if (borrowing.BookId <= 0)
            {
                throw new Exception("BookId wajib diisi");
            }

            var member = db.Members.Find(borrowing.MemberId);

            if (member == null)
            {
                throw new Exception("Member tidak ditemukan");
            }

            var user = db.Users.Find(member.UserId);

            if (user == null)
            {
                throw new Exception("User member tidak ditemukan");
            }

            if (user.Status != "Active")
            {
                throw new Exception("Member belum aktif, tidak bisa mengajukan peminjaman");
            }

            var book = db.Books.Find(borrowing.BookId);

            if (book == null)
            {
                throw new Exception("Buku tidak ditemukan");
            }

            if (book.AvailableStock <= 0)
            {
                throw new Exception("Stok buku tidak tersedia");
            }

            var alreadyBorrowingSameBook = db.Borrowings.Any(x =>
                x.MemberId == borrowing.MemberId &&
                x.BookId == borrowing.BookId &&
                (x.Status == "Requested" || x.Status == "Borrowed")
            );

            if (alreadyBorrowingSameBook)
            {
                throw new Exception("Member masih memiliki pengajuan atau peminjaman aktif untuk buku ini");
            }

            var activeBorrowingCount = db.Borrowings.Count(x =>
                x.MemberId == borrowing.MemberId &&
                (x.Status == "Requested" || x.Status == "Borrowed")
            );

            if (activeBorrowingCount >= 3)
            {
                throw new Exception("Member maksimal hanya boleh memiliki 3 peminjaman aktif");
            }

            var notes = borrowing.Notes;

            if (!string.IsNullOrWhiteSpace(notes))
            {
                notes = notes.Trim();
            }
            else
            {
                notes = null;
            }

            var newBorrowing = new Borrowing
            {
                BookId = borrowing.BookId,
                MemberId = borrowing.MemberId,
                RequestDate = DateTime.Now,
                BorrowDate = null,
                DueDate = null,
                ReturnDate = null,
                Status = "Requested",
                Notes = notes,
                ApprovedBy = null,
                ApprovedAt = null,
                ReturnedBy = null,
                ReturnedAt = null,
                LateDays = null,
                FineAmount = null,
                FineStatus = null
            };

            db.Borrowings.Add(newBorrowing);
            db.SaveChanges();

            SendBorrowingRequestEmail(newBorrowing.Id);

            return new
            {
                message = "Pengajuan peminjaman berhasil dibuat",
                borrowingId = newBorrowing.Id,
                status = newBorrowing.Status
            };
        }

        public object ApproveBorrowing(int id, int approvedBy)
        {
            if (approvedBy <= 0)
            {
                throw new Exception("ApprovedBy wajib diisi");
            }

            var librarian = db.Users.Find(approvedBy);

            if (librarian == null)
            {
                throw new Exception("User pustakawan tidak ditemukan");
            }

            if (librarian.Role != "Librarian" || librarian.Status != "Active")
            {
                throw new Exception("Hanya pustakawan aktif yang bisa menyetujui peminjaman");
            }

            var borrowing = db.Borrowings.Find(id);

            if (borrowing == null)
            {
                return null;
            }

            if (borrowing.Status != "Requested")
            {
                throw new Exception("Peminjaman ini sudah diproses");
            }

            var book = db.Books.Find(borrowing.BookId);

            if (book == null)
            {
                throw new Exception("Buku tidak ditemukan");
            }

            if (book.AvailableStock <= 0)
            {
                throw new Exception("Stok buku tidak tersedia");
            }

            borrowing.Status = "Borrowed";
            borrowing.BorrowDate = DateTime.Now.Date;
            borrowing.DueDate = DateTime.Now.Date.AddDays(7);
            borrowing.ApprovedBy = approvedBy;
            borrowing.ApprovedAt = DateTime.Now;

            book.AvailableStock = book.AvailableStock - 1;
            book.UpdatedAt = DateTime.Now;

            var log = new ActivityLog
            {
                UserId = approvedBy,
                Action = "Approve Borrowing",
                Description = "Pustakawan menyetujui peminjaman buku dengan ID " + borrowing.BookId,
                CreatedAt = DateTime.Now
            };

            db.ActivityLogs.Add(log);
            db.SaveChanges();

            SendBorrowingApprovedEmail(borrowing.Id);

            return new
            {
                message = "Peminjaman berhasil disetujui",
                borrowingId = borrowing.Id,
                status = borrowing.Status,
                borrowDate = borrowing.BorrowDate,
                dueDate = borrowing.DueDate,
                availableStock = book.AvailableStock
            };
        }

        public object RejectBorrowing(int id, int rejectedBy)
        {
            if (rejectedBy <= 0)
            {
                throw new Exception("RejectedBy wajib diisi");
            }

            var librarian = db.Users.Find(rejectedBy);

            if (librarian == null)
            {
                throw new Exception("User pustakawan tidak ditemukan");
            }

            if (librarian.Role != "Librarian" || librarian.Status != "Active")
            {
                throw new Exception("Hanya pustakawan aktif yang bisa menolak peminjaman");
            }

            var borrowing = db.Borrowings.Find(id);

            if (borrowing == null)
            {
                return null;
            }

            if (borrowing.Status != "Requested")
            {
                throw new Exception("Peminjaman ini sudah diproses");
            }

            borrowing.Status = "Rejected";
            borrowing.ApprovedBy = rejectedBy;
            borrowing.ApprovedAt = DateTime.Now;

            var log = new ActivityLog
            {
                UserId = rejectedBy,
                Action = "Reject Borrowing",
                Description = "Pustakawan menolak pengajuan peminjaman buku dengan ID " + borrowing.BookId,
                CreatedAt = DateTime.Now
            };

            db.ActivityLogs.Add(log);
            db.SaveChanges();

            SendBorrowingRejectedEmail(borrowing.Id);

            return new
            {
                message = "Pengajuan peminjaman berhasil ditolak",
                borrowingId = borrowing.Id,
                status = borrowing.Status
            };
        }

        public object ReturnBook(int id, int returnedBy)
        {
            if (returnedBy <= 0)
            {
                throw new Exception("ReturnedBy wajib diisi");
            }

            var librarian = db.Users.Find(returnedBy);

            if (librarian == null)
            {
                throw new Exception("User pustakawan tidak ditemukan");
            }

            if (librarian.Role != "Librarian" || librarian.Status != "Active")
            {
                throw new Exception("Hanya pustakawan aktif yang bisa memproses pengembalian buku");
            }

            var borrowing = db.Borrowings.Find(id);

            if (borrowing == null)
            {
                return null;
            }

            if (borrowing.Status != "Borrowed")
            {
                throw new Exception("Buku ini belum dalam status dipinjam atau sudah dikembalikan");
            }

            var book = db.Books.Find(borrowing.BookId);

            if (book == null)
            {
                throw new Exception("Buku tidak ditemukan");
            }

            DateTime returnDate = DateTime.Now;

            int lateDays = 0;
            decimal fineAmount = 0;

            if (borrowing.DueDate != null && returnDate.Date > borrowing.DueDate.Value.Date)
            {
                lateDays = (returnDate.Date - borrowing.DueDate.Value.Date).Days;
                fineAmount = lateDays * 2000;
            }

            borrowing.ReturnDate = returnDate;
            borrowing.Status = "Returned";
            borrowing.ReturnedBy = returnedBy;
            borrowing.ReturnedAt = DateTime.Now;
            borrowing.LateDays = lateDays;
            borrowing.FineAmount = fineAmount;

            if (fineAmount > 0)
            {
                borrowing.FineStatus = "Unpaid";
            }
            else
            {
                borrowing.FineStatus = "None";
            }

            book.AvailableStock = book.AvailableStock + 1;

            if (book.AvailableStock > book.Stock)
            {
                book.AvailableStock = book.Stock;
            }

            book.UpdatedAt = DateTime.Now;

            var log = new ActivityLog
            {
                UserId = returnedBy,
                Action = "Return Book",
                Description = "Pustakawan memproses pengembalian buku dengan ID " + borrowing.BookId,
                CreatedAt = DateTime.Now
            };

            db.ActivityLogs.Add(log);
            db.SaveChanges();

            return new
            {
                message = "Buku berhasil dikembalikan",
                borrowingId = borrowing.Id,
                status = borrowing.Status,
                returnDate = borrowing.ReturnDate,
                lateDays = borrowing.LateDays,
                fineAmount = borrowing.FineAmount,
                fineStatus = borrowing.FineStatus,
                availableStock = book.AvailableStock
            };
        }

        private void SendBorrowingRequestEmail(int borrowingId)
        {
            try
            {
                var data = db.Borrowings
                    .Join(db.Books,
                        borrowing => borrowing.BookId,
                        book => book.Id,
                        (borrowing, book) => new { borrowing, book })
                    .Join(db.Members,
                        x => x.borrowing.MemberId,
                        member => member.Id,
                        (x, member) => new { x.borrowing, x.book, member })
                    .Join(db.Users,
                        x => x.member.UserId,
                        user => user.Id,
                        (x, user) => new
                        {
                            BorrowingId = x.borrowing.Id,
                            BookTitle = x.book.Title,
                            RequestDate = x.borrowing.RequestDate,
                            FullName = user.FullName,
                            Email = user.Email
                        })
                    .FirstOrDefault(x => x.BorrowingId == borrowingId);

                if (data == null)
                {
                    return;
                }

                EmailHelper.SendEmail(
                    data.Email,
                    "Pengajuan Peminjaman Buku - Moon Books",
                    $@"
                    <h3>Pengajuan Peminjaman Berhasil</h3>
                    <p>Halo <b>{data.FullName}</b>,</p>

                    <p>Pengajuan peminjaman buku Anda telah berhasil dikirim.</p>

                    <p>
                        <b>Judul Buku:</b> {data.BookTitle}<br/>
                        <b>Tanggal Pengajuan:</b> {data.RequestDate.ToString("dd-MM-yyyy HH:mm")}<br/>
                        <b>Status:</b> Menunggu persetujuan pustakawan
                    </p>

                    <p>Silakan menunggu konfirmasi dari pustakawan Moon Books.</p>

                    <br/>
                    <p>Salam hangat,</p>
                    <p><b>Moon Books</b></p>
                    "
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Email pengajuan peminjaman gagal dikirim: " + ex.Message);
            }
        }

        private void SendBorrowingApprovedEmail(int borrowingId)
        {
            try
            {
                var data = db.Borrowings
                    .Join(db.Books,
                        borrowing => borrowing.BookId,
                        book => book.Id,
                        (borrowing, book) => new { borrowing, book })
                    .Join(db.Members,
                        x => x.borrowing.MemberId,
                        member => member.Id,
                        (x, member) => new { x.borrowing, x.book, member })
                    .Join(db.Users,
                        x => x.member.UserId,
                        user => user.Id,
                        (x, user) => new
                        {
                            BorrowingId = x.borrowing.Id,
                            BookTitle = x.book.Title,
                            BorrowDate = x.borrowing.BorrowDate,
                            DueDate = x.borrowing.DueDate,
                            FullName = user.FullName,
                            Email = user.Email
                        })
                    .FirstOrDefault(x => x.BorrowingId == borrowingId);

                if (data == null)
                {
                    return;
                }

                EmailHelper.SendEmail(
                    data.Email,
                    "Peminjaman Buku Disetujui - Moon Books",
                    $@"
                    <h3>Peminjaman Buku Disetujui</h3>
                    <p>Halo <b>{data.FullName}</b>,</p>

                    <p>Pengajuan peminjaman buku Anda telah disetujui oleh pustakawan.</p>

                    <p>
                        <b>Judul Buku:</b> {data.BookTitle}<br/>
                        <b>Tanggal Pinjam:</b> {(data.BorrowDate == null ? "-" : data.BorrowDate.Value.ToString("dd-MM-yyyy"))}<br/>
                        <b>Jatuh Tempo:</b> {(data.DueDate == null ? "-" : data.DueDate.Value.ToString("dd-MM-yyyy"))}
                    </p>

                    <p>Harap mengembalikan buku sebelum tanggal jatuh tempo agar tidak terkena denda keterlambatan.</p>

                    <br/>
                    <p>Salam hangat,</p>
                    <p><b>Moon Books</b></p>
                    "
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Email persetujuan peminjaman gagal dikirim: " + ex.Message);
            }
        }

        private void SendBorrowingRejectedEmail(int borrowingId)
        {
            try
            {
                var data = db.Borrowings
                    .Join(db.Books,
                        borrowing => borrowing.BookId,
                        book => book.Id,
                        (borrowing, book) => new { borrowing, book })
                    .Join(db.Members,
                        x => x.borrowing.MemberId,
                        member => member.Id,
                        (x, member) => new { x.borrowing, x.book, member })
                    .Join(db.Users,
                        x => x.member.UserId,
                        user => user.Id,
                        (x, user) => new
                        {
                            BorrowingId = x.borrowing.Id,
                            BookTitle = x.book.Title,
                            FullName = user.FullName,
                            Email = user.Email
                        })
                    .FirstOrDefault(x => x.BorrowingId == borrowingId);

                if (data == null)
                {
                    return;
                }

                EmailHelper.SendEmail(
                    data.Email,
                    "Pengajuan Peminjaman Ditolak - Moon Books",
                    $@"
                    <h3>Pengajuan Peminjaman Ditolak</h3>
                    <p>Halo <b>{data.FullName}</b>,</p>

                    <p>Mohon maaf, pengajuan peminjaman buku Anda belum dapat disetujui.</p>

                    <p>
                        <b>Judul Buku:</b> {data.BookTitle}<br/>
                        <b>Status:</b> Ditolak
                    </p>

                    <p>Silakan hubungi pustakawan untuk informasi lebih lanjut.</p>

                    <br/>
                    <p>Salam hangat,</p>
                    <p><b>Moon Books</b></p>
                    "
                );
            }
            catch (Exception ex)
            {
                throw new Exception("Email penolakan peminjaman gagal dikirim: " + ex.Message);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibraryManagementSystem.Models.ViewModel
{
    public class SummaryReportViewModel
    {
        public int TotalBooks { get; set; }
        public int TotalCategories { get; set; }
        public int TotalMembers { get; set; }

        public int ActiveMembers { get; set; }
        public int PendingMembers { get; set; }
        public int RejectedMembers { get; set; }

        public int TotalBorrowings { get; set; }
        public int RequestedBorrowings { get; set; }
        public int BorrowedBooks { get; set; }
        public int ReturnedBooks { get; set; }
        public int RejectedBorrowings { get; set; }

        public int TotalStock { get; set; }
        public int AvailableStock { get; set; }

        public decimal TotalFines { get; set; }
        public decimal PaidFines { get; set; }
        public decimal UnpaidFines { get; set; }
    }
}
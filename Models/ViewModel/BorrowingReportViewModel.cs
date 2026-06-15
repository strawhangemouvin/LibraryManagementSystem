using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibraryManagementSystem.Models.ViewModel
{
    public class BorrowingReportViewModel
    {
        public int BorrowingId { get; set; }
        public string MemberName { get; set; }
        public string BookTitle { get; set; }

        public DateTime RequestDate { get; set; }
        public DateTime? BorrowDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        public string Status { get; set; }
        public string Notes { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibraryManagementSystem.Models.ViewModel
{
    public class BorrowingViewModel
    {
        public int Id { get; set; }

        public int BookId { get; set; }
        public string BookTitle { get; set; }

        public int MemberId { get; set; }
        public string MemberName { get; set; }
        public string Username { get; set; }

        public DateTime RequestDate { get; set; }
        public DateTime? BorrowDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        public int? LateDays { get; set; }
        public decimal? FineAmount { get; set; }
        public string FineStatus { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }

        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public int? ReturnedBy { get; set; }
        public DateTime? ReturnedAt { get; set; }
    }
}
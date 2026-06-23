using System;

namespace LibraryManagementSystem.Models.ViewModel
{
    public class FineReportViewModel
    {
        public int BorrowingId { get; set; }
        public string MemberName { get; set; }
        public string Username { get; set; }
        public string BookTitle { get; set; }
        public DateTime? BorrowDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public int? LateDays { get; set; }
        public decimal? FineAmount { get; set; }
        public string FineStatus { get; set; }
    }
}

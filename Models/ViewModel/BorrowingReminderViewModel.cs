using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibraryManagementSystem.Models.ViewModel
{
    public class BorrowingReminderViewModel
    {
        public int BorrowingId { get; set; }
        public string BookTitle { get; set; }
        public DateTime? DueDate { get; set; }
        public int DaysRemaining { get; set; }
        public string ReminderStatus { get; set; }
    }
}

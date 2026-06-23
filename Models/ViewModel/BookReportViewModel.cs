using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibraryManagementSystem.Models.ViewModel
{
    public class BookReportViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string CategoryName { get; set; }
        public string Publisher { get; set; }
        public int? PublishYear { get; set; }
        public int Stock { get; set; }
        public int AvailableStock { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
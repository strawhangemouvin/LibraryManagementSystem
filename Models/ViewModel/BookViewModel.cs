using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace LibraryManagementSystem.Models.ViewModel
{
    public class BookViewModel
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public int? PublishYear { get; set; }
        public string ISBN { get; set; }
        public int Stock { get; set; }
        public int AvailableStock { get; set; }
        public string Description { get; set; }
        public string CoverImage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibraryManagementSystem.Models.ViewModel
{
    public class MemberViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }

        public string MemberCode { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string ClassName { get; set; }

        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibraryManagementSystem.Models.ViewModel
{
    public class RegisterRequest
    {
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public string Phone { get; set; }
        public string Address { get; set; }
        public string ClassName { get; set; }
    }
}
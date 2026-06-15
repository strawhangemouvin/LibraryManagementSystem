using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LibraryManagementSystem.Models.ViewModel
{
    public class LoginResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public string Token { get; set; }
        public string Message { get; set; }
    }
}
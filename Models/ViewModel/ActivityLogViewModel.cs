using System;

namespace LibraryManagementSystem.Models.ViewModel
{
    public class ActivityLogViewModel
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

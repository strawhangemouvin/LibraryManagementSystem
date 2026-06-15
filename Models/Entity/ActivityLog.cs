using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models.Entity
{
    [Table("ActivityLogs")]
    public class ActivityLog
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
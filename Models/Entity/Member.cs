using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models.Entity
{
    [Table("Members")]
    public class Member
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string MemberCode { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        [StringLength(50)]
        public string ClassName { get; set; }

        public int? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("ApprovedBy")]
        public virtual User ApprovedByUser { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models.Entity
{
    [Table("Books")]
    public class Book
    {
        [Key]
        public int Id { get; set; }

        public int CategoryId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [Required]
        [StringLength(100)]
        public string Author { get; set; }

        [StringLength(100)]
        public string Publisher { get; set; }

        public int? PublishYear { get; set; }

        [StringLength(50)]
        public string ISBN { get; set; }

        [Required]
        public int Stock { get; set; }

        [Required]
        public int AvailableStock { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        [StringLength(255)]
        public string CoverImage { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }
    }
}
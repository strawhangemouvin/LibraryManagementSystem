using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models.Entity
{
    [Table("Categories")]
    public class Category
    {
        [Key]
        public int Id { get; set; }

        public string CategoryName { get; set; }

        public string Description { get; set; }
    }
}
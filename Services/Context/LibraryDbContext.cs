using LibraryManagementSystem.Models.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace LibraryManagementSystem.Services.Context
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext() : base("LibraryDbContext")
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Borrowing> Borrowings { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
    }
}
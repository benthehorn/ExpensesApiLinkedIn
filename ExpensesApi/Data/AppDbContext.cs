using ExpensesApi.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ExpensesApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("name=ExpensesDb")
        {

        }
        public DbSet<Entry> Entries { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
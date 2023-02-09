using Microsoft.EntityFrameworkCore;
using OgeApp.Entyties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task = OgeApp.Entyties.Task;

namespace OgeApp
{
    public class EFDBContext : DbContext
    {
        public DbSet<Picture> Pictures { get; set; } = null!;
        public DbSet<Topic> Topics { get; set; } = null!;
        public DbSet<Task> Tasks { get; set; } = null!;
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=OgeDataBase.db");
        }
    }
}

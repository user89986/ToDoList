using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ToDoList.Models;
using Task = ToDoList.Models.Task;

namespace ToDoList.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=Bd.db");


        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Связь многие-ко-многим для Task и Tag
            modelBuilder.Entity<Task>()
                .HasMany(t => t.Tags)
                .WithMany(t => t.Tasks)
                .UsingEntity<Dictionary<string, object>>(
                    "TaskTags",
                    j => j.HasOne<Tag>().WithMany().HasForeignKey("TagId"),
                    j => j.HasOne<Task>().WithMany().HasForeignKey("TaskId")
                );
            modelBuilder.Entity<Task>()
           .Property(t => t.IsCompleted)
           .HasDefaultValue(false);

            // Индексы для ускорения поиска
            modelBuilder.Entity<Task>()
                .HasIndex(t => new { t.IsCompleted, t.Deadline });



        }

    }
}

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ToDoList.Data;
using ToDoList.Models;
using ToDoList.Services;
using Task = ToDoList.Models.Task;

namespace ToDoList.Services
{
    public class TaskService
    {
        private readonly AppDbContext _db;
        private readonly SyncService _syncService;
        private readonly TagService _tagService;

        public TaskService(AppDbContext db, SyncService syncService, TagService tagService)
        {
            _db = db;
            _syncService = syncService;
            _tagService = tagService;
        }

        public async System.Threading.Tasks.Task AddTaskAsync(string title, string description, DateTime? deadline, Category category, ObservableCollection<Task> tasks, Action clearForm, Action applyFilters)
        {
            var task = new Task
            {
                Title = title.Trim(),
                Description = description,
                Deadline = deadline,
                Category = category,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDirty = true
            };

            _db.Tasks.Add(task);
            await _db.SaveChangesAsync();

            tasks.Add(task);
            applyFilters?.Invoke();
            clearForm?.Invoke();
            await _syncService.FullSyncAsync();
        }

        public void EditTask(Task task, ObservableCollection<Task> tasks, ObservableCollection<Category> categories, Action applyFilters)
        {
            if (task == null) return;

            var taskCopy = new Task
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Deadline = task.Deadline,
                Category = task.Category,
                IsCompleted = task.IsCompleted,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                Tags = task.Tags != null ? new List<Tag>(task.Tags) : new List<Tag>()
            };

            var editWindow = new EditTaskWindow(taskCopy, categories, _tagService);
            if (editWindow.ShowDialog() == true)
            {
                try
                {
                    var taskFromDb = _db.Tasks
                        .Include(t => t.Category)
                        .Include(t => t.Tags)
                        .FirstOrDefault(t => t.Id == task.Id);

                    if (taskFromDb != null)
                    {
                        taskFromDb.Title = taskCopy.Title;
                        taskFromDb.Description = taskCopy.Description;
                        taskFromDb.Deadline = taskCopy.Deadline;
                        taskFromDb.Category = taskCopy.Category;
                        taskFromDb.UpdatedAt = DateTime.UtcNow;
                        taskFromDb.Tags = taskCopy.Tags;

                        _db.SaveChanges();

                        var index = tasks.IndexOf(task);
                        if (index >= 0)
                        {
                            tasks[index] = taskFromDb;
                        }

                        applyFilters?.Invoke();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void DeleteTask(Task task, ObservableCollection<Task> tasks, Action applyFilters)
        {
            _db.Tasks.Remove(task);
            _db.SaveChanges();
            tasks.Remove(task);
            applyFilters?.Invoke();
        }
    }
}
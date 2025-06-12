using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using ToDoList.Data;
using ToDoList.Models;

namespace ToDoList.Services
{
    public class TagService
    {
        private readonly AppDbContext _db;
        private readonly FirestoreDbContext _firestoreDb;

        public TagService(AppDbContext db, FirestoreDbContext firestoreDb)
        {
            _db = db;
            _firestoreDb = firestoreDb;
        }

        public async Task<List<Tag>> GetAllTagsAsync()
        {
            return await _db.Tags.AsNoTracking().ToListAsync();
        }

        public async Task<Tag?> AddTagAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            var tag = new Tag
            {
                Id = $"tag_{Guid.NewGuid()}", // Ensure consistent ID format
                Name = name.Trim()
            };

            _db.Tags.Add(tag);
            await _db.SaveChangesAsync();
            return tag;
        }

        public async Task<bool> AddTagToTaskAsync(string taskId, string tagId)
        {
            _db.ChangeTracker.Clear();

            var task = await _db.Tasks
                .Include(t => t.Tags)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null) return false;

            if (task.Tags.Any(t => t.Id == tagId)) return true;

            var trackedTag = await _db.Tags.FirstOrDefaultAsync(t => t.Id == tagId);
            if (trackedTag == null) return false;

            task.Tags.Add(trackedTag);
            task.IsDirty = true;

            try
            {
                await _db.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                // Отладочный вывод: какие объекты отслеживаются
                foreach (var entry in _db.ChangeTracker.Entries())
                {
                    Console.WriteLine($"Tracked entity: {entry.Entity.GetType().Name}, State: {entry.State}, ID: {entry.Property("Id").CurrentValue}");
                }

                Console.WriteLine($"Ошибка при сохранении: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveTagFromTaskAsync(string taskId, string tagId)
        {
            _db.ChangeTracker.Clear();

            var task = await _db.Tasks
                .Include(t => t.Tags)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null) return false;

            // Find the tag without causing tracking issues
            var tagToRemove = task.Tags.FirstOrDefault(t => t.Id == tagId);
            if (tagToRemove == null) return false;

            task.Tags.Remove(tagToRemove);
            task.IsDirty = true;

            try
            {
                await _db.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error removing tag: {ex.Message}");
                return false;
            }
        }
        public async Task<Tag> CreateTagWithUI(string defaultName = "")
        {
            var tagName = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите название тега:",
                "Новый тег",
                defaultName);

            if (!string.IsNullOrWhiteSpace(tagName))
            {
                return await AddTagAsync(tagName);
            }
            return null;
        }

        public async Task<bool> AddTagToTaskWithUI(Models.Task task, IEnumerable<Tag> availableTags)
        {
            if (task == null) return false;

            var tagName = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите название тега:",
                "Добавить тег к задаче",
                "");

            if (!string.IsNullOrWhiteSpace(tagName))
            {
                var tag = availableTags.FirstOrDefault(t => t.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase));
                if (tag == null)
                {
                    tag = await CreateTagWithUI(tagName);
                }

                if (tag != null)
                {
                    return await AddTagToTaskAsync(task.Id, tag.Id);
                }
            }
            return false;
        }
    }
}
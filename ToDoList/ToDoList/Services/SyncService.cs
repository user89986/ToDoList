using System;
using System.Linq;
using System.Threading.Tasks;
using ToDoList.Data;
using ToDoList.Models;

namespace ToDoList.Services
{
    public class SyncService
    {
        private readonly AppDbContext _localDb;
        private readonly FirestoreDbContext _firestoreDb;

        public SyncService(AppDbContext localDb, FirestoreDbContext firestoreDb)
        {
            _localDb = localDb;
            _firestoreDb = firestoreDb;
        }

        public async System.Threading.Tasks.Task FullSyncAsync()
        {
            // 1. Синхронизация из облака в локальную БД
            var cloudTasks = await _firestoreDb.GetAllTasksAsync();

            foreach (var cloudTask in cloudTasks)
            {
                var localTask = _localDb.Tasks.FirstOrDefault(t => t.Id == cloudTask.Id);

                if (localTask == null)
                {
                    // Добавляем новую задачу
                    _localDb.Tasks.Add(cloudTask);
                }
                else if (cloudTask.UpdatedAt > localTask.UpdatedAt)
                {
                    // Обновляем локальную задачу
                    localTask.Title = cloudTask.Title;
                    localTask.Description = cloudTask.Description;
                    localTask.IsCompleted = cloudTask.IsCompleted;
                    localTask.Deadline = cloudTask.Deadline;
                    localTask.UpdatedAt = cloudTask.UpdatedAt;
                    localTask.CategoryId = cloudTask.CategoryId;
                    localTask.Priority = cloudTask.Priority;
                    localTask.IsDirty = false;
                }
            }

            // 2. Синхронизация из локальной БД в облако
            var dirtyTasks = _localDb.Tasks.Where(t => t.IsDirty).ToList();

            foreach (var dirtyTask in dirtyTasks)
            {
                await _firestoreDb.SyncTaskAsync(dirtyTask);
                dirtyTask.IsDirty = false;
            }

            await _localDb.SaveChangesAsync();
        }
    }
}